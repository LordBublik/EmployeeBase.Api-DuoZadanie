namespace EmployeeBase.Shared.DTOs
{
    public class UserInfoDto
    {
        public string NameToShow { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}