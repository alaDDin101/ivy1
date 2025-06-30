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
        public async Task<PagedResult<PatientDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Patients.Include(e=>e.PersonNavigation).AsNoTracking();

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
    }
}
