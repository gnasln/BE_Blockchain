namespace Base_BE.Identity;

public class VerifyOTPRequest
{
    public string Email { get; set; }  // Trường này lưu trữ địa chỉ email của người dùng
    public string OTP { get; set; }    // Trường này lưu trữ mã OTP mà người dùng nhập vào để xác thực
}
