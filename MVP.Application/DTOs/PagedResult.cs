using System.Collections.Generic;

namespace MVP.Application.DTOs;

public record PagedResult<T>(List<T> Items, int TotalCount, int PageNumber, int PageSize);
