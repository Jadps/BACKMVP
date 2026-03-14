using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MVP.Application.DTOs;

namespace MVP.Application.Interfaces.Users;

public interface IUserService
{
    Task<ApplicationResult<List<UserDto>>> GetAllActiveAsync();
    Task<ApplicationResult<PagedResult<UserDto>>> GetPagedAsync(int pageNumber, int pageSize);
    Task<ApplicationResult<UserDto>> GetByUidAsync(Guid uid);
    Task<ApplicationResult> CreateAsync(UserDto dto);
    Task<ApplicationResult> UpdateAsync(UserDto dto);
    Task<ApplicationResult> DeleteAsync(Guid id);
}
