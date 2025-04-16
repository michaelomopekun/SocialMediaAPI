public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string body);
    // Task SendEmailConfirmationAsync(string email, string token);
    // Task SendPasswordResetAsync(string email, string token);
    // Task SendWelcomeEmailAsync(string email, string firstName);
    // Task SendAccountDeactivationEmailAsync(string email);
    // Task SendAccountReactivationEmailAsync(string email);
}