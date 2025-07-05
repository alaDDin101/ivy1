using Application.Commons;
using Application.Dto;
using Application.Exceptions;
using Application.Interfaces.IRepositories.Patient;
using Domain.Entities;
using Ivy.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Infrastructure.Repositories
{
    public class PetientRepository : IPatientRepository
    {
        private readonly IvyContext _context;

        public PetientRepository(IvyContext context)
        {
            _context = context;
        }
        #region GetPagedAsync
        public async Task<PagedResult<PatientDto>> GetPagedAsync(int pageNumber, int pageSize, string userId, List<string> roles)
        {
            IQueryable<Patient> query;

            if (roles.Contains("Admin"))
            {
                // Admins see all patients
                query = _context.Patients.Include(p => p.PersonNavigation);
            }
            else if (roles.Contains("Doctor"))
            {
                // Get the doctor's Person ID from the Users table
                var doctorPartyId = await _context.Users
                    .Where(u => u.MembershipUser == userId)
                    .Select(u => u.Party)
                    .FirstOrDefaultAsync();

                // Get patients that have appointments with the doctor
                var doctorPatients = _context.Appointments
                    .Where(a => a.DoctorClinicNavigation.DoctorNavigation.Person == doctorPartyId)
                    .Select(a => a.Patient)
                    .Distinct();

                query = _context.Patients
                    .Where(p => doctorPatients.Contains(p.Person))
                    .Include(p => p.PersonNavigation);
            }
            else if (roles.Contains("ClinicEmployee"))
            {
                // Get the employee's Person ID
                var staffPartyId = await _context.Users
                    .Where(u => u.MembershipUser == userId)
                    .Select(u => u.Party)
                    .FirstOrDefaultAsync();

                // Get clinics the employee works in
                var clinicIds = await _context.ClinicEmplyees
                    .Where(e => e.Person == staffPartyId)
                    .Select(e => e.Clinic)
                    .ToListAsync();

                // Get patients with appointments in these clinics
                var clinicPatients = _context.Appointments
                    .Where(a => clinicIds.Contains(a.DoctorClinicNavigation.Clinic))
                    .Select(a => a.Patient)
                    .Distinct();

                query = _context.Patients
                    .Where(p => clinicPatients.Contains(p.Person))
                    .Include(p => p.PersonNavigation);
            }
            else
            {
                // Unauthorized
                return new PagedResult<PatientDto>
                {
                    Items = new List<PatientDto>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Person)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PatientDto
                {
                    Address = p.PersonNavigation.Address,
                    BirthDate = p.PersonNavigation.BirthDate,
                    FatherName = p.PersonNavigation.FatherName,
                    FirstName = p.PersonNavigation.FirstName,
                    LastName = p.PersonNavigation.LastName,
                    Id = p.PersonNavigation.Party,
                    NationalNumber = p.PersonNavigation.NationalNumber,
                    PatientCode = p.PatientCode
                })
                .ToListAsync();

            return new PagedResult<PatientDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageSize = pageSize,
                PageNumber = pageNumber
            };
        }

        #endregion

        # region CreatePatientAsync
        public async Task<PatientDto> CreatePatientAsync(PatientDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var person = await _context.People
                .FirstOrDefaultAsync(p => p.NationalNumber == dto.NationalNumber);
                if (person != null)
                {
                    var patientExists = await _context.Patients
                    .AnyAsync(p => p.Person == person.Party);

                    if (patientExists)
                    {
                        throw new PatientAlreadyExistsException(dto.NationalNumber);
                    }
                    var patient = new Patient
                    {
                        Person = person.Party,
                        PatientCode = dto.PatientCode
                    };
                    _context.Patients.Add(patient);
                    
                   
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                   
                    return new PatientDto() { 
                        Id = patient.Person,
                        Address = person.Address,
                        BirthDate = person.BirthDate,
                        FatherName = person.FatherName,
                        FirstName = person.FirstName,
                        LastName = person.LastName,
                        NationalNumber = person.NationalNumber,
                        PatientCode = patient.PatientCode
                    };
                }
                else
                {
                    var party = new Party
                    {
                        DispalyName = $"{dto.FirstName} {dto.FatherName} {dto.LastName}",
                        IsActive = true
                    };
                    _context.Parties.Add(party);
                    await _context.SaveChangesAsync(); 

                    var newPerson = new Person
                    {
                        Party = party.Id,
                        FirstName = dto.FirstName,
                        FatherName = dto.FatherName,
                        LastName = dto.LastName,
                        BirthDate = dto.BirthDate,
                        Address = dto.Address,
                        NationalNumber = dto.NationalNumber
                    };
                    _context.People.Add(newPerson);

                    var newPatient = new Patient
                    {
                        Person = party.Id,
                        PatientCode = dto.PatientCode
                    };
                    _context.Patients.Add(newPatient);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    dto.Id = newPatient.Person;
                    return dto;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw ex;
            }
        }
        #endregion
        #region UpdatePatientAsync
        public async Task<PatientDto> UpdatePatientAsync(PatientDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var patient = await _context.Patients
                    .Include(p => p.PersonNavigation)
                    .ThenInclude(pn => pn.PartyNavigation)
                    .FirstOrDefaultAsync(p => p.Person == dto.Id);

                if (patient == null)
                {
                    throw new NotFoundException("Patient not found.");
                }

                // Update person data
                var person = patient.PersonNavigation;
                person.FirstName = dto.FirstName;
                person.FatherName = dto.FatherName;
                person.LastName = dto.LastName;
                person.BirthDate = dto.BirthDate;
                person.Address = dto.Address;
                person.NationalNumber = dto.NationalNumber;

                // Update party display name
                var party = await _context.Parties.FindAsync(person.Party);
                if (party != null)
                {
                    party.DispalyName = $"{dto.FirstName} {dto.FatherName} {dto.LastName}";
                }

                // Update patient data
                patient.PatientCode = dto.PatientCode;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return dto;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw ex;
            }
        }
        #endregion
    }
}
