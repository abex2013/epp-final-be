using Excellerent.Usermanagement.Domain.Entities;
using Excellerent.UserManagement.Presentation.Models.GetModels;
using System;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Excellerent.UserManagement.Presentation.Helper
{
    public static class MailHelper
    {
        public static async Task<bool> SendAccountNotificationMail(MailSenderOptions option, UserEntity user, UserNotificationType type)
        {
            try
            {
             MailMessage msg = new MailMessage();
            msg.To.Add(new MailAddress(user.Email, $"{user.LastName} {user.FirstName} "));
            msg.From = new MailAddress(option.FromAddress, "Excellerent");
            msg.Subject = "Your Excellerent EPP Account";
                string body = string.Empty;

                StringBuilder s = new StringBuilder();
                if (type == UserNotificationType.AccountCreated)
                {
                    s.Append($"<h1>Welcome to Excellerent Enterprise Project Portfolio!</h1>");
                    s.Append($"Dear <em>{user.LastName} { user.FirstName},</em>");
                    s.Append($"<p>You have been invited to Enterprise Project Porfolio. This email includes your account details, so keep it safe!</p>");
                    s.Append($"<p></p>");
                    s.Append($"<dd>User name: <em>{user.Email}</em></dd><br />");
                    s.Append($"<dd>Password: <em>{user.Password}</em></dd>");
                    s.Append($"<p></p>");

                }
                else
                {
                    s.Append($"<h2>Enterprise Project Portfolio(EPP) - Password reset notification</h2>");
                    s.Append($"Dear <em>{user.LastName} { user.FirstName},</em>");
                    s.Append($"<p>The password for your Enterprise Project Porfolio account ({user.Email}) is reset.</p>");
                    s.Append($"<p></p>");
                    s.Append($"<dd>New password: <em>{user.Password}</em></dd>");
                    s.Append($"<p></p>");

                }
                msg.Body = s.ToString();
            msg.IsBodyHtml = true;

            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(option.UserName, option.Password);
            client.Port = option.Port;
            client.Host = option.Server;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
                await client.SendMailAsync(msg);
                msg.Dispose();
                client.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

    }
}
