using MailKit.Net.Smtp;

using MimeKit;
using MimeKit.Text;

namespace PetProj.SLL;

public class EmailSender : IEmailSender
{
	private readonly IConfiguration _configuration;
	private const string HOSTNAME = "dlia24kino@yandex.ru";
	public EmailSender(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	public async Task SendConfirmationEmailForRegistrationAsync(string email, string code)
	{
		var letter = new MimeMessage(
			from: new[] { MailboxAddress.Parse(HOSTNAME) },
			to: new[] { MailboxAddress.Parse(email) },
			subject: "Confirm code",
			body: new TextPart(TextFormat.Html) { Text = $"<h1>Your code: {code}</h1>" });

		using var smtp = new SmtpClient();
		await smtp.ConnectAsync(
			host: "smtp.yandex.ru",
			port: 465,
			options: MailKit.Security.SecureSocketOptions.SslOnConnect);
		await smtp.AuthenticateAsync(HOSTNAME, "npfjbpxgyvyxitcu");
		await smtp.SendAsync(letter);
		await smtp.DisconnectAsync(true);
	}
}
