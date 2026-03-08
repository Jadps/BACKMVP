using System.Threading.Tasks;
using SGEDI.Application.DTOs;

namespace SGEDI.Application.Interfaces;

public interface IOnboardingService
{
    Task<ApplicationResult> RegistrarNuevoTenantAsync(OnboardingRequestDTO request);
}
