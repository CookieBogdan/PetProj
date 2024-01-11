namespace PetProj.SLL;

public interface IEmailSender
{
	public Task SendConfirmationEmailForRegistrationAsync(string email, int code);
}
