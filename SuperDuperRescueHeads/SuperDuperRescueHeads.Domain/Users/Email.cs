namespace SuperDuperRescueHeads.Domain.Users;

/// <summary>
/// Value object representing an email address
/// </summary>
public record Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        if (email.Length > 256)
            throw new ArgumentException("Email cannot exceed 256 characters", nameof(email));

        return new Email(email.ToLowerInvariant().Trim());
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public override string ToString() => Value;
}
