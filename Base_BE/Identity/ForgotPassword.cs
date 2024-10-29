namespace Base_BE.Identity
{
    public record ForgotPassword
    {
        public required string UserName { get; init; }
        public required string new_password { get; init; }
        public required string confirmed_password { get; init; }

    }
}
