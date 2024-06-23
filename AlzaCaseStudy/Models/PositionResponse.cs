namespace AlzaCaseStudy.Models
{
    public class PositionResponse
    {
        public string? Name { get; set; }
        public string? SeoName { get; set; }
        public string? Url { get; set; }
        public string? Workplace { get; set; }
        public bool ForStudents { get; set; }
        public EmployeeMeta? GestorUser { get; set; }
        public EmployeeMeta? ExecutiveUser { get; set; }
        public Meta? People { get; set; }
        public PositionItems? PositionItems { get; set; }
        public Department? Department { get; set; }
        public string? PlaceOfEmplyment { get; set; }
    }
}
