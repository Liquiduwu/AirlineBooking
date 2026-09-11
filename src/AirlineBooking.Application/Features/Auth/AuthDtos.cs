using AirlineBooking.Domain.Enums;

namespace AirlineBooking.Application.Features.Auth;

public record LoginRequest(string Username, string Password);

public record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, string Username, StaffRole Role);

public record RegisterStaffRequest(string Username, string Email, string Password, StaffRole Role);

public record StaffUserDto(int Id, string Username, string Email, StaffRole Role, bool IsActive);
