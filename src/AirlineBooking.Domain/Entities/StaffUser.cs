using AirlineBooking.Domain.Common;
using AirlineBooking.Domain.Enums;

namespace AirlineBooking.Domain.Entities;

/// <summary>
/// An internal staff account (booking agent or admin) - the actual authenticated principal
/// behind every JWT this API issues. Deliberately a plain entity + hand-rolled auth rather
/// than the full ASP.NET Core Identity membership system: Identity brings ~7 tables, external
/// login providers, 2FA, email confirmation, etc. that this API doesn't need. We reuse
/// Identity's <c>PasswordHasher&lt;T&gt;</c> for the one part worth not reinventing - salted,
/// versioned password hashing - without adopting the rest of the framework.
/// </summary>
public class StaffUser : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public StaffRole Role { get; set; }
    public bool IsActive { get; set; } = true;
}
