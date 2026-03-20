using System.Threading.Tasks;
using MVP.Application.DTOs;
using MVP.Domain.Constants;

namespace MVP.Application.Interfaces;

public interface IOnboardingService
{
    Task<ApplicationResult> RegisterNewTenantAsync(OnboardingRequestDto request, string initialRoleName = AppRoles.TenantAdmin, bool isHost = false);
}
