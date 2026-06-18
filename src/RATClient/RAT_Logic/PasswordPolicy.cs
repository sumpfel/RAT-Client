using System.Linq;

namespace RAT_Logic
{
    //KI start (Claude Opus 4.8, prompt 22): shared password policy, mirrored by the backend (validate_password).
    // Rules: at least 8 characters, at least one letter and one digit. The client validates first (instant,
    // friendly message) and the backend enforces the same so a weak password can never be stored.
    public static class PasswordPolicy
    {
        public const int MinLength = 8;

        /// <summary>
        /// Validates a password against the policy. Returns true if ok; otherwise false and an explanatory message.
        /// </summary>
        public static bool Validate(string? password, out string error)
        {
            if (string.IsNullOrEmpty(password) || password.Length < MinLength)
            {
                error = $"Password must be at least {MinLength} characters long.";
                return false;
            }
            if (!password.Any(char.IsLetter))
            {
                error = "Password must contain at least one letter.";
                return false;
            }
            if (!password.Any(char.IsDigit))
            {
                error = "Password must contain at least one digit.";
                return false;
            }
            error = "";
            return true;
        }
    }
    //KI end
}
