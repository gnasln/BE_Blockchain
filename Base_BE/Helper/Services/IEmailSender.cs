using Base_BE.Domain.Entities;

namespace Base_BE.Helper.Services;

public interface IEmailSender
{
    Task SendEmailAsync(string email, string name, string otp);
    Task SendEmailRegisterAsync(string email, string fullname, string userName, string password,
        string keyPrivate);
    Task SendEmailNotificationAsync(List<ApplicationUser> user, string content, string voteName,
        string candidateNames, DateTime startDate, DateTime expiredDate);
    Task SendEmailNotificationCandidateAsync(List<ApplicationUser> user, string content, string voteName,
        string candidateNames, DateTime startDate, DateTime expiredDate);
}