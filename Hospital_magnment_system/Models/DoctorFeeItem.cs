namespace Hospital_magnment_system.Models
{
    public class DoctorFeeItem
    {
        public int DoctorID { get; set; }
        public string DoctorName { get; set; }
        public string ServiceDescription { get; set; }
        public string ConsultationType { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; }
    }
} 