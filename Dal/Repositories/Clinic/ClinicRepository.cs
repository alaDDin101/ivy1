using Application.Commons;
using Application.Dto;
using Application.Interfaces.IRepositories.Clinic;
using Domain.Entities;
using Ivy.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Infrastructure.Repositories
{
    public class ClinicRepository : IClinicRepository
    {
        private readonly IvyContext _context;

        public ClinicRepository(IvyContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ClinicListDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Clinics
                .Include(c => c.Addresses).ThenInclude(a => a.CityNavigation)
                .Include(c => c.DoctorClinics).ThenInclude(dc => dc.DoctorNavigation).ThenInclude(d => d.PersonNavigation)
                .AsNoTracking();

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(c => c.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ClinicListDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    PhoneNumber = c.PhoneNumber,
                    Description = c.Description,
                    Address = c.Addresses.FirstOrDefault()!.DetailedAddress,
                    CityName = c.Addresses.FirstOrDefault()!.CityNavigation.Name,
                    Doctors = c.DoctorClinics.Select(dc => dc.DoctorNavigation.PersonNavigation.FirstName + " " + dc.DoctorNavigation.PersonNavigation.LastName).ToList()
                })
                .ToListAsync();

            return new PagedResult<ClinicListDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageSize = pageSize,
                PageNumber = pageNumber
            };
        }

        public async Task<ClinicListDto> CreateClinicAsync(CreateClinicDto dto)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                var clinic = new Domain.Entities.Clinic
                {
                    Name = dto.Name,
                    PhoneNumber = dto.PhoneNumber,
                    Description = dto.Description
                };
                _context.Clinics.Add(clinic);
                await _context.SaveChangesAsync();

                var address = new Address
                {
                    Clinic = clinic.Id,
                    City = dto.CityId,
                    DetailedAddress = dto.DetailedAddress
                };
                _context.Addresses.Add(address);

                foreach (var doctorId in dto.DoctorIds)
                {
                    _context.DoctorClinics.Add(new DoctorClinic
                    {
                        Clinic = clinic.Id,
                        Doctor = doctorId,

                        From = dto.From
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var city = await _context.Cities.FindAsync(dto.CityId);
                var doctors = await _context.People.Where(p => dto.DoctorIds.Contains(p.Party))
                    .Select(p => p.FirstName + " " + p.LastName).ToListAsync();

                return new ClinicListDto
                {
                    Id = clinic.Id,
                    Name = clinic.Name,
                    PhoneNumber = clinic.PhoneNumber,
                    Description = clinic.Description,
                    Address = dto.DetailedAddress,
                    CityName = city!.Name,
                    Doctors = doctors
                };
            }
            catch (Exception ex) 
            {
                throw ex;
            }
        }

        public async Task<ClinicListDto> UpdateClinicAsync(UpdateClinicDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var clinic = await _context.Clinics.Include(c => c.Addresses).FirstOrDefaultAsync(c => c.Id == dto.Id)
                         ?? throw new Exception("Clinic not found");

            clinic.Name = dto.Name;
            clinic.PhoneNumber = dto.PhoneNumber;
            clinic.Description = dto.Description;

            var address = clinic.Addresses.FirstOrDefault();
            if (address != null)
            {
                address.DetailedAddress = dto.DetailedAddress;
                address.City = dto.CityId;
            }
            else
            {
                _context.Addresses.Add(new Address
                {
                    Clinic = clinic.Id,
                    DetailedAddress = dto.DetailedAddress,
                    City = dto.CityId
                });
            }

            var existingDoctors = _context.DoctorClinics.Where(dc => dc.Clinic == clinic.Id);
            _context.DoctorClinics.RemoveRange(existingDoctors);

            foreach (var doctorId in dto.DoctorIds)
            {
                _context.DoctorClinics.Add(new DoctorClinic
                {
                    Clinic = clinic.Id,
                    Doctor = doctorId,
                    From = dto.From
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var city = await _context.Cities.FindAsync(dto.CityId);
            var doctors = await _context.People.Where(p => dto.DoctorIds.Contains(p.Party))
                .Select(p => p.FirstName + " " + p.LastName).ToListAsync();

            return new ClinicListDto
            {
                Id = clinic.Id,
                Name = clinic.Name,
                PhoneNumber = clinic.PhoneNumber,
                Description = clinic.Description,
                Address = dto.DetailedAddress,
                CityName = city!.Name,
                Doctors = doctors
            };
        }
    }
}
