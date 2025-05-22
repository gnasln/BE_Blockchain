using System.Net;
using System.Net.Mail;
using Base_BE.Domain.Entities;
using FluentEmail.Core;
using Base_BE.Helper.Services;

namespace Base_BE.Services;

public class EmailSender : IEmailSender
{
    private readonly SmtpClient _client;
    private readonly string _fromAddress;
    private readonly IFluentEmail _fluentEmail;
    private readonly IFluentEmailFactory _fluentEmailFactory;
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration, IFluentEmail fluentEmail, IFluentEmailFactory fluentEmailFactory)
    {
        _client = new SmtpClient(configuration["EmailSettings:Host"])
        {
            Port = int.Parse(configuration["EmailSettings:Port"]),
            Credentials = new NetworkCredential(configuration["EmailSettings:Username"], configuration["EmailSettings:Password"]),
            EnableSsl = bool.Parse(configuration["EmailSettings:EnableSsl"])
        };
        _fromAddress = configuration["EmailSettings:FromAddress"];
        _fluentEmail = fluentEmail;
        _fluentEmailFactory = fluentEmailFactory;
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string name, string otp)
    {
        var companyName = _configuration["EmailSettings:CompanyName"] ?? "Your Company";
        var fromAddress = _fromAddress; // already set from configuration
        
        var result = await _fluentEmail
            .SetFrom(fromAddress, companyName)
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
        
        var companyName = _configuration["EmailSettings:CompanyName"] ?? "Your Company";
        var fromAddress = _fromAddress; // already set from configuration
        
        var result = await _fluentEmail
            .SetFrom(fromAddress, companyName)
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
    
    public async Task SendEmailNotificationAsync(List<ApplicationUser> users, string content, string voteName, string candidateNames, DateTime startDate, DateTime expiredDate)
    {
        var companyName = _configuration["EmailSettings:CompanyName"] ?? "Your Company";
        var fromAddress = _fromAddress;
        
        foreach (var user in users)
        {
            var result = await _fluentEmailFactory
                .Create()
                .SetFrom(fromAddress, companyName)
                .To(user.Email ?? user.NewEmail, user.FullName)
                .Subject("System Notification")
                .UsingTemplateFromFile($"{Directory.GetCurrentDirectory()}/Resources/Templates/system-notification.cshtml",
                    new 
                    { 
                        Name = user.FullName, 
                        Content = content, 
                        VoteName = voteName, 
                        CandidateNames = candidateNames, 
                        StartDate = startDate, 
                        ExpiredDate = expiredDate 
                    })
                .SendAsync();

            if (!result.Successful)
            {
                throw new Exception($"Failed to send email: {result.ErrorMessages.FirstOrDefault()}");
            }
        }
    }
    
    public async Task SendEmailNotificationCandidateAsync(List<ApplicationUser> users, string content, string voteName, string candidateNames, DateTime startDate, DateTime expiredDate)
    {
        var companyName = _configuration["EmailSettings:CompanyName"] ?? "Your Company";
        var fromAddress = _fromAddress; // already set from configuration
        
        foreach (var user in users)
        {
            var result = await _fluentEmailFactory
                .Create()
                .SetFrom(fromAddress, companyName)
                .To(user.Email ?? user.NewEmail, user.FullName)
                .Subject("System Notification")
                .UsingTemplateFromFile($"{Directory.GetCurrentDirectory()}/Resources/Templates/system-notification.cshtml",
                    new 
                    { 
                        Name = user.FullName, 
                        Content = content, 
                        VoteName = voteName, 
                        CandidateNames = candidateNames, 
                        StartDate = startDate, 
                        ExpiredDate = expiredDate 
                    })
                .SendAsync();

            if (!result.Successful)
            {
                throw new Exception($"Failed to send email: {result.ErrorMessages.FirstOrDefault()}");
            }
        }
    }

}
