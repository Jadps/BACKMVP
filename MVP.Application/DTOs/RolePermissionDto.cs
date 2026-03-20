using MVP.Domain.Entities;
using System;

namespace MVP.Application.DTOs;

public class RolePermissionDto
{
    public Guid ModuleId { get; set; }
    public PermissionLevel Permission { get; set; }
}
