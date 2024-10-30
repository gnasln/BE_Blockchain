using System.Net;
using System.Net.Mail;
using FluentEmail.Core;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Base_BE.Services;

public class EmailSender : IEmailSender
{
    private readonly SmtpClient _client;
    private readonly string _fromAddress;
    private readonly IFluentEmail _fluentEmail;

    public EmailSender(IConfiguration configuration, IFluentEmail fluentEmail)
    {
        _client = new SmtpClient(configuration["EmailSettings:Host"])
        {
            Port = int.Parse(configuration["EmailSettings:Port"]),
            Credentials = new NetworkCredential(configuration["EmailSettings:Username"], configuration["EmailSettings:Password"]),
            EnableSsl = bool.Parse(configuration["EmailSettings:EnableSsl"])
        };
        _fromAddress = configuration["EmailSettings:FromAddress"];
        _fluentEmail = fluentEmail;
    }

    public async Task SendEmailAsync(string email, string name, string otp)
    {
        var result = await _fluentEmail
            .To(email, name)
            .Subject("Xác Minh Email")
            .UsingTemplateFromFile($"{Directory.GetCurrentDirectory()}/Resources/Templates/Send_OTP.cshtml",
                new { Name = name, OtpCode = otp })
            .SendAsync();

        if (!result.Successful)
        {
            throw new Exception($"Failed to send email: {result.ErrorMessages.FirstOrDefault()}");
        }
    }

    public async Task SendEmailRegisterAsync(string email, string fullname, string userName, string password, string keyPrivate)
    {
        var result = await _fluentEmail
            .To(email, fullname)
            .Subject("Thông báo tạo tài khoản")
            .UsingTemplateFromFile($"{Directory.GetCurrentDirectory()}/Resources/Templates/Send_Password.cshtml",
                new { Name = fullname, UserName = userName, Password = password, PrivateKey = keyPrivate })
            .SendAsync();

        if (!result.Successful)
        {
            throw new Exception($"Failed to send email: {result.ErrorMessages.FirstOrDefault()}");
        }
    }

}
