namespace HandyConnect.Api.Common;

/// <summary>
/// Error payload understood by the frontend: a stable machine code (translated client-side)
/// plus an English message for logs and API consumers.
/// </summary>
public sealed record ApiError(string Code, string Message)
{
    public static readonly ApiError InvalidCredentials = new("invalid_credentials", "Invalid credentials.");
    public static readonly ApiError DuplicateAccount = new("duplicate_account", "An account with this phone or email already exists.");
    public static readonly ApiError InvalidPhone = new("invalid_phone", "Enter a valid Tunisian phone number.");
    public static readonly ApiError InvalidInput = new("invalid_input", "Some fields are invalid.");
    public static readonly ApiError NotFound = new("not_found", "Resource not found.");
    public static readonly ApiError ContactRequired = new("contact_required", "Contact the artisan before reviewing.");
    public static readonly ApiError AlreadyReviewed = new("already_reviewed", "You already reviewed this artisan.");
}
