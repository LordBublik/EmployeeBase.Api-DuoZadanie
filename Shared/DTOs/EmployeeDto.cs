namespace EmployeeBase.Shared.DTOs
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Profession { get; set; } = null!;
        public int DepartmentId { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
    }
}