using System.Net;
using System.Net.Mail;

namespace IdentityWebApp.Helpers
{
    public static class ResetPasswordHelper
    {
        public static void ResetPasswordSendEmail(string link)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress("x");
                mail.To.Add("x");
                mail.Subject = $"www.cengiz.com::Şifre yenileme";
                mail.Body = "<h2>Şifrenizi yenilemek için lütfen aşagıdaki linke tıklayınız.</h2><hr/>";
                mail.Body += $"<a href='{link}'>Şifre yenileme linki</a>";
                mail.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("x", "x");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }
    }
}
