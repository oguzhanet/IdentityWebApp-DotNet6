using System.Net;
using System.Net.Mail;

namespace IdentityWebApp.Helpers
{
    public static class EmailConfirmation
    {
        public static void SendEmail(string link, string email)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress("aiyana.heidenreich@ethereal.email");
                mail.To.Add("aiyana.heidenreich@ethereal.email");
                mail.Subject = $"www.cengiz.com::Email doğrulama";
                mail.Body = "<h2>Email doğrulamak için lütfen aşagıdaki linke tıklayınız.</h2><hr/>";
                mail.Body += $"<a href='{link}'>Email doğrulama linki</a>";
                mail.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient("smtp.ethereal.email", 587))
                {
                    smtp.Credentials = new NetworkCredential("aiyana.heidenreich@ethereal.email", "1hxerNwqNbwUEpW5bF");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }
    }
}
