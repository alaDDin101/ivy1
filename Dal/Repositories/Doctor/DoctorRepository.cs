using Application.Commons;
using Application.Dto;
using Application.Exceptions;
using Application.Interfaces.IRepositories.Doctor;
using Domain.Entities;
using Infrastructure.Persistence;
using Ivy.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Ivy.Infrastructure.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly IvyContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _identityContext;

        public DoctorRepository(
            IvyContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            DataContext identityContext)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _identityContext = identityContext;
        }

        public async Task<PagedResult<DoctorDto>> GetPagedAsync(int page, int size)
        {
            var query = _context.Doctors
                .Include(d => d.PersonNavigation)
                .Include(d => d.SpecialtyNavigation)
                .AsNoTracking();

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(d => d.Person)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(d => new DoctorDto
                {
                    Id = d.Person,
                    FirstName = d.PersonNavigation.FirstName,
                    FatherName = d.PersonNavigation.FatherName,
                    LastName = d.PersonNavigation.LastName,
                    Address = d.PersonNavigation.Address,
                    BirthDate = d.PersonNavigation.BirthDate,
                    NationalNumber = d.PersonNavigation.NationalNumber,
                    Description = d.Description,
                    Image = d.Image,
                    SpecialtyId = d.Specialty,
                    SpecialtyName = d.SpecialtyNavigation.Name
                }).ToListAsync();

            return new PagedResult<DoctorDto>
            {
                Items = items,
                TotalCount = total,
                PageSize = size,
                PageNumber = page
            };
        }

        public async Task<DoctorDto> CreateAsync(CreateDoctorDto dto)
        {
            if (!Regex.IsMatch(dto.NationalNumber, @"^\d{11}$"))
                throw new ValidationException("National number must be exactly 11 digits.");


            var transactionIvy = await _context.Database.BeginTransactionAsync();
            var transactionIdentity = await _identityContext.Database.BeginTransactionAsync();


            try
            {
                // 1. Domain logic
                var person = await _context.People.FirstOrDefaultAsync(p => p.NationalNumber == dto.NationalNumber);
                long personId;

                if (person != null)
                {
                    if (await _context.Doctors.AnyAsync(d => d.Person == person.Party))
                        throw new ValidationException("Doctor already exists with this national number.");

                    personId = person.Party;

                    _context.Doctors.Add(new Doctor
                    {
                        Person = personId,
                        Description = dto.Description,
                        Image = dto.Image,
                        Specialty = dto.SpecialtyId
                    });
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

                    personId = party.Id;

                    _context.People.Add(new Person
                    {
                        Party = party.Id,
                        FirstName = dto.FirstName,
                        FatherName = dto.FatherName,
                        LastName = dto.LastName,
                        BirthDate = dto.BirthDate,
                        Address = dto.Address,
                        NationalNumber = dto.NationalNumber
                    });

                    _context.Doctors.Add(new Doctor
                    {
                        Person = party.Id,
                        Description = dto.Description,
                        Image = dto.Image,
                        Specialty = dto.SpecialtyId
                    });
                    _context.DoctorClinics.Add(new DoctorClinic()
                    {
                        Doctor = party.Id,
                        Clinic = dto.ClinicId,
                        From = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)
                    });
                }

                // 2. Identity logic
                var identityUser = new IdentityUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };

                var identityResult = await _userManager.CreateAsync(identityUser, dto.Password);
                if (!identityResult.Succeeded)
                    throw new ValidationException("Failed to create user: " + string.Join(", ", identityResult.Errors.Select(e => e.Description)));

                var roleResult = await _userManager.AddToRoleAsync(identityUser, "doctor");
                if (!roleResult.Succeeded)
                    throw new ValidationException("Failed to add role: " + string.Join(", ", roleResult.Errors.Select(e => e.Description)));

                _context.Users.Add(new User
                {
                    Party = personId,
                    MembershipUser = identityUser.Id
                });

                await _context.SaveChangesAsync();
                await _identityContext.SaveChangesAsync();

                

                await transactionIdentity.CommitAsync();
                await transactionIvy.CommitAsync();

                return new DoctorDto
                {
                    Id = personId,
                    Address = dto.Address,
                    BirthDate = dto.BirthDate,
                    Description = dto.Description,
                    Email = dto.Email,
                    FatherName = dto.FatherName,
                    FirstName = dto.FirstName,
                    Image = dto.Image,
                    LastName = dto.LastName,
                    NationalNumber = dto.NationalNumber,
                    Phonenumber = dto.PhoneNumber,
                    Password = dto.Password,
                    SpecialtyId = dto.SpecialtyId,
                };
            }
            catch
            {
                await transactionIdentity.RollbackAsync();
                await transactionIvy.RollbackAsync();
                throw;
            }
           
        }

        public async Task<DoctorDto> UpdateAsync(DoctorDto dto)
        {
            if (!Regex.IsMatch(dto.NationalNumber, @"^\d{11}$"))
                throw new ValidationException("National number must be exactly 11 digits.");

            using var tx = await _context.Database.BeginTransactionAsync();

            var doctor = await _context.Doctors
                .Include(d => d.PersonNavigation)
                .FirstOrDefaultAsync(d => d.Person == dto.Id);

            if (doctor == null)
                throw new NotFoundException("Doctor not found.");

            var person = doctor.PersonNavigation;
            var party = await _context.Parties.FindAsync(person.Party);

            person.FirstName = dto.FirstName;
            person.FatherName = dto.FatherName;
            person.LastName = dto.LastName;
            person.BirthDate = dto.BirthDate;
            person.Address = dto.Address;
            person.NationalNumber = dto.NationalNumber;

            if (party != null)
            {
                party.DispalyName = $"{dto.FirstName} {dto.FatherName} {dto.LastName}";
            }

            doctor.Description = dto.Description;
            doctor.Image = dto.Image;
            doctor.Specialty = dto.SpecialtyId;

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return dto;
        }
        public async Task EditDoctorInfoAsync(EditDoctorInfoDto dto)
        {
            var doctor = await _context.Doctors
                .Include(d => d.PersonNavigation)
                .FirstOrDefaultAsync(d => d.Person == dto.Id);

            if (doctor == null) throw new NotFoundException("Doctor not found");

            var person = doctor.PersonNavigation;
            person.FirstName = dto.FirstName;
            person.FatherName = dto.FatherName;
            person.LastName = dto.LastName;
            person.BirthDate = dto.BirthDate;
            person.Address = dto.Address;
            person.NationalNumber = dto.NationalNumber;

            doctor.Image = dto.Image;
            doctor.Description = dto.Description;
            doctor.Specialty = dto.SpecialtyId;

            var party = await _context.Parties.FindAsync(person.Party);
            if (party != null)
            {
                party.DispalyName = $"{dto.FirstName} {dto.FatherName} {dto.LastName}";
            }

            await _context.SaveChangesAsync();
        }
        public async Task EditDoctorClinicAsync(EditDoctorClinicDto dto)
        {
            var current = await _context.DoctorClinics
                .FirstOrDefaultAsync(dc => dc.Doctor == dto.DoctorId && dc.To == null);

            if (current != null)
            {
                current.To = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            }

            var newClinic = new DoctorClinic
            {
                Doctor = dto.DoctorId,
                Clinic = dto.NewClinicId,
                From = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)
            };

            _context.DoctorClinics.Add(newClinic);
            await _context.SaveChangesAsync();
        }
        public async Task EditDoctorPhoneAsync(EditDoctorPhoneDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Party == dto.DoctorId);
            if (user == null) throw new NotFoundException("User not found");

            var identityUser = await _userManager.FindByIdAsync(user.MembershipUser);
            if (identityUser == null) throw new NotFoundException("Identity user not found");

            identityUser.PhoneNumber = dto.NewPhoneNumber;
            identityUser.PhoneNumberConfirmed = true;

            await _userManager.UpdateAsync(identityUser);
        }
        public async Task EditDoctorEmailAsync(EditDoctorEmailDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Party == dto.DoctorId);
            if (user == null) throw new NotFoundException("User not found");

            var identityUser = await _userManager.FindByIdAsync(user.MembershipUser);
            if (identityUser == null) throw new NotFoundException("Identity user not found");

            identityUser.Email = dto.NewEmail;
            identityUser.EmailConfirmed = true;

            await _userManager.UpdateAsync(identityUser);
        }
    }
}
