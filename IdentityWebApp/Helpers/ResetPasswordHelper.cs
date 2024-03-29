﻿using System.Net;
using System.Net.Mail;

namespace IdentityWebApp.Helpers
{
    public static class ResetPasswordHelper
    {
        public static void ResetPasswordSendEmail(string link, string email)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress("aiyana.heidenreich@ethereal.email");
                mail.To.Add(email);
                mail.Subject = $"www.cengiz.com::Şifre yenileme";
                mail.Body = "<h2>Şifrenizi yenilemek için lütfen aşagıdaki linke tıklayınız.</h2><hr/>";
                mail.Body += $"<a href='{link}'>Şifre yenileme linki</a>";
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
