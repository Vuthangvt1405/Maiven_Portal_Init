using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;

namespace Maiven_Portal_Managment.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterStudentAsync(
        RegisterRequest request,
        CancellationToken cancellationToken);

    Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken);
}
