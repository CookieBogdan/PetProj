using MailKit.Net.Smtp;

using MimeKit;
using MimeKit.Text;

namespace PetProj.SLL;

public class EmailSender : IEmailSender
{
	public EmailSender(IConfiguration configuration)
	{
		SenderAddress = configuration["EmailSettings:SenderAddress"]!;
		SenderPassword = configuration["EmailSettings:SenderPassword"]!;
		SmtpHostname = configuration["EmailSettings:SmtpHostname"]!;
		SmtpPort = int.Parse(configuration["EmailSettings:SmtpPort"]!);
	}

	private string SenderAddress { get; }
	private string SenderPassword { get; }
	private string SmtpHostname { get; }
	private int SmtpPort { get; }

	public async Task SendConfirmationEmailForRegistrationAsync(string email, string code)
	{
		var letter = new MimeMessage(
			from: new[] { MailboxAddress.Parse(SenderAddress) },
			to: new[] { MailboxAddress.Parse(email) },
			subject: "Confirm code",
			body: new TextPart(TextFormat.Html) { Text = $"<h1>Your code: {code}</h1>" });

		using var smtp = new SmtpClient();
		await smtp.ConnectAsync(
			host: SmtpHostname,
			port: SmtpPort,
			options: MailKit.Security.SecureSocketOptions.SslOnConnect);
		await smtp.AuthenticateAsync(SenderAddress, SenderPassword);
		await smtp.SendAsync(letter);
		await smtp.DisconnectAsync(true);
	}
}
