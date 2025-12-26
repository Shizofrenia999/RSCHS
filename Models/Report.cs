namespace RSCHS.Models
{
    public class Report
    {
        public int ID { get; set; }
        public int AssignmentID { get; set; }
        public string Content { get; set; }
        public DateTime CreationDate { get; set; }

        // Связанные данные для отображения
        public string AssignmentDetails { get; set; }
        public string IncidentLocation { get; set; }
        public string EmployeeName { get; set; }
        public string TransportInfo { get; set; }
    }
}