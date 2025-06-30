using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dto
{
    public class PatientDto
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string FatherName { get; set; }
        public string LastName { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string? Address { get; set; }
        public string NationalNumber { get; set; } = null!;
        public string PatientCode { get; set; } = null!;
    }
}
