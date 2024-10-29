namespace Base_BE.Identity
{
    public record ChangePassword
    {
        public required string oldPassword { get; set; }
        public required string newPassword { get; set; }
        public required string comfirmedPassword { get; set; }

    }
}
