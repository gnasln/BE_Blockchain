namespace Base_BE.Identity
{
    public record ForgotPassword
    {
        public required  string UserName { get; init; }
        public required string Email { get; init; }

    }
}
