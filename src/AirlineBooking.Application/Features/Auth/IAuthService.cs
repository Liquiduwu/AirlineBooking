namespace AirlineBooking.Application.Features.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<StaffUserDto> RegisterStaffAsync(RegisterStaffRequest request, CancellationToken cancellationToken = default);
}
