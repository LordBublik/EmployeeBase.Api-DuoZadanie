using System;
using System.Collections.Generic;

namespace EmployeeBase.Api.Data;

public partial class Employee
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Profession { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public int DepartmentId { get; set; }

    public int? RoleId { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual Role? Role { get; set; }
}
