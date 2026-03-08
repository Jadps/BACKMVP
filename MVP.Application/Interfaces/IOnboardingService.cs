using System.Threading.Tasks;
using MVP.Application.DTOs;

namespace MVP.Application.Interfaces;

public interface IOnboardingService
{
    Task<ApplicationResult> RegistrarNuevoTenantAsync(OnboardingRequestDTO request);
}
