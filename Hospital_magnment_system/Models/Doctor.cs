using System;

namespace Hospital_magnment_system.Models
{
    public class Doctor
    {
        public int DoctorID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Specialization { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string LicenseNumber { get; set; }
        public string Department { get; set; }
        public DateTime JoinDate { get; set; }
        public string Status { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
} 