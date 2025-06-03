using System.Net.Mail;
using System.Net;

namespace BlockShare.Services
{
	public class EmailService
	{
		public async Task SendConfirmationEmail(string toEmail, string confirmLink)
		{
			var message = new MailMessage();
			message.To.Add(toEmail);
			message.Subject = "Підтвердження email";
			message.Body = $"Для підтвердження акаунта перейдіть за посиланням: {confirmLink}";
			message.IsBodyHtml = false;
			message.From = new MailAddress("dean.hagenes@ethereal.email");

			using var smtp = new SmtpClient("smtp.ethereal.email")
			{
				Port = 587,
				Credentials = new NetworkCredential("dean.hagenes@ethereal.email", "u3nx6s48mfdGJPwrrX"),
				EnableSsl = true
			};

			await smtp.SendMailAsync(message);
		}

        public async Task SendPasswordResetEmail(string toEmail, string resetLink)
        {
            var message = new MailMessage();
            message.To.Add(toEmail);
            message.Subject = "Відновлення пароля";
            message.Body = $"Для скидання паролю перейдіть за посиланням: {resetLink}";
            message.IsBodyHtml = false;
            message.From = new MailAddress("dean.hagenes@ethereal.email");

            using var smtp = new SmtpClient("smtp.ethereal.email")
            {
                Port = 587,
                Credentials = new NetworkCredential("dean.hagenes@ethereal.email", "u3nx6s48mfdGJPwrrX"),
                EnableSsl = true
            };

            await smtp.SendMailAsync(message);
        }


    }
}
