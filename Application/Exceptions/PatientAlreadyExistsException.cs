using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Exceptions
{
    public class PatientAlreadyExistsException : Exception
    {
        public PatientAlreadyExistsException(string nationalNumber)
            : base($"A patient with national number '{nationalNumber}' already exists.")
        {
        }
    }
}
