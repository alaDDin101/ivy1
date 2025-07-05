using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dto
{
    public class PatientDto
    {
        public long Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string FatherName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public DateOnly? BirthDate { get; set; }
        public string? Address { get; set; }
        [Required]
        public string NationalNumber { get; set; } = null!;
        public string PatientCode { get; set; } = null!;
    }
}
