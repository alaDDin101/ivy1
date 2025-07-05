using Application.Commons;
using Application.Dto;
using Application.Exceptions;
using Application.Interfaces.IRepositories.ClinicEmployee;
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

namespace Infrastructure.Repositories.ClinicEmployee
{
    public class ClinicEmployeeRepository : IClinicEmployeeRepository
    {
        private readonly IvyContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _identityContext;

        public ClinicEmployeeRepository(
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

        public async Task<ClinicEmployeeDto> CreateAsync(CreateClinicEmployeeDto dto)
        {
            if (!Regex.IsMatch(dto.NationalNumber, @"^\d{11}$"))
                throw new ValidationException("National number must be exactly 11 digits.");

            var transactionIvy = await _context.Database.BeginTransactionAsync();
            var transactionIdentity = await _identityContext.Database.BeginTransactionAsync();

            try
            {
                long personId;
                var person = await _context.People.FirstOrDefaultAsync(p => p.NationalNumber == dto.NationalNumber);

                if (person != null)
                {
                    if (await _context.ClinicEmplyees.AnyAsync(e => e.Person == person.Party))
                        throw new ValidationException("Clinic employee already exists with this national number.");

                    personId = person.Party;

                    _context.ClinicEmplyees.Add(new ClinicEmplyee
                    {
                        Person = personId,
                        Clinic = dto.ClinicId,
                        From = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)
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

                    _context.ClinicEmplyees.Add(new ClinicEmplyee
                    {
                        Person = party.Id,
                        Clinic = dto.ClinicId,
                        From = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)
                    });
                }

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

                var roleResult = await _userManager.AddToRoleAsync(identityUser, "clinic-staff");
                if (!roleResult.Succeeded)
                    throw new ValidationException("Failed to assign role: " + string.Join(", ", roleResult.Errors.Select(e => e.Description)));

                _context.Users.Add(new User
                {
                    Party = personId,
                    MembershipUser = identityUser.Id
                });

                await _context.SaveChangesAsync();
                await _identityContext.SaveChangesAsync();

                await transactionIdentity.CommitAsync();
                await transactionIvy.CommitAsync();

                return new ClinicEmployeeDto
                {
                    Id = personId,
                    FirstName = dto.FirstName,
                    FatherName = dto.FatherName,
                    LastName = dto.LastName,
                    BirthDate = dto.BirthDate,
                    Address = dto.Address,
                    NationalNumber = dto.NationalNumber,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    Role = dto.Role,
                    ClinicId = dto.ClinicId
                };
            }
            catch
            {
                await transactionIdentity.RollbackAsync();
                await transactionIvy.RollbackAsync();
                throw;
            }
        }


        public async Task<ClinicEmployeeDto> UpdateInfoAsync(UpdateClinicEmployeeInfoDto dto)
        {
            var person = await _context.People.FindAsync(dto.Id);
            if (person == null) throw new NotFoundException("Employee not found");

            person.FirstName = dto.FirstName;
            person.FatherName = dto.FatherName;
            person.LastName = dto.LastName;
            person.BirthDate = dto.BirthDate;
            person.Address = dto.Address;
            person.NationalNumber = dto.NationalNumber;

            var party = await _context.Parties.FindAsync(dto.Id);
            if (party != null)
                party.DispalyName = $"{dto.FirstName} {dto.FatherName} {dto.LastName}";

            await _context.SaveChangesAsync();

            return new ClinicEmployeeDto
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                FatherName = dto.FatherName,
                LastName = dto.LastName,
                BirthDate = dto.BirthDate,
                Address = dto.Address,
                NationalNumber = dto.NationalNumber
            };
        }

        public async Task UpdateEmailAsync(UpdateClinicEmployeeEmailDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Party == dto.Id);
            if (user == null) throw new NotFoundException("User not found");

            var identityUser = await _userManager.FindByIdAsync(user.MembershipUser);
            identityUser.Email = dto.Email;

            await _userManager.UpdateAsync(identityUser);
        }

        public async Task UpdatePhoneAsync(UpdateClinicEmployeePhoneDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Party == dto.Id);
            if (user == null) throw new NotFoundException("User not found");

            var identityUser = await _userManager.FindByIdAsync(user.MembershipUser);
            identityUser.PhoneNumber = dto.PhoneNumber;

            await _userManager.UpdateAsync(identityUser);
        }

        public async Task RemoveFromClinicAsync(long personId)
        {
            var entry = await _context.ClinicEmplyees.FirstOrDefaultAsync(e => e.Person == personId && e.To!=null);
            if (entry == null) throw new NotFoundException("Clinic employee not found");
            entry.To = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            _context.ClinicEmplyees.Update(entry);
            await _context.SaveChangesAsync();
        }
        public async Task<PagedResult<ClinicEmployeeDto>> GetPagedAsync(int page, int size)
        {
            var query = _context.ClinicEmplyees
                .Include(e => e.PersonNavigation)
                .AsNoTracking();

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(e => e.Person)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(e => new ClinicEmployeeDto
                {
                    Id = e.Person,
                    FirstName = e.PersonNavigation.FirstName,
                    FatherName = e.PersonNavigation.FatherName,
                    LastName = e.PersonNavigation.LastName,
                    BirthDate = e.PersonNavigation.BirthDate,
                    Address = e.PersonNavigation.Address,
                    NationalNumber = e.PersonNavigation.NationalNumber,
                    ClinicId = e.Clinic,
                    From = e.From,
                    To = e.To
                }).ToListAsync();

            return new PagedResult<ClinicEmployeeDto>
            {
                Items = items,
                TotalCount = total,
                PageNumber = page,
                PageSize = size
            };
        }
    }

}
