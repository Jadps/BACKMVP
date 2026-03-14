using System;
using System.Collections.Generic;

namespace MVP.Application.DTOs;

public class UserDto
{
    public Guid? Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? SecondLastName { get; set; }
    public string? Password { get; set; }
    public Guid? TenantId { get; set; }
    public List<RoleDto> Roles { get; set; } = new();
    public string FriendlyName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public int CatStatusAccountId { get; set; }
    public string? FullName { get; set; }
}
