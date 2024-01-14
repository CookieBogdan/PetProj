using PetProj.Utils;

namespace PetProj;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);
		var services = builder.Services;

		services.AddControllers();
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen();

		services.AddCors(options =>
		{
			options.AddPolicy("AllowAnyHost", builder =>
			{
				builder.AllowAnyOrigin()
						.AllowAnyMethod()
						.AllowAnyHeader();
			});
		});

		Authentication.SetAuthentication(services, builder.Configuration);
		ServiceRegistry.Register(services, builder.Configuration);

		var app = builder.Build();
		app.UseCors("AllowAnyHost");

		// Configure the HTTP request pipeline.
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}
		//app.UseRouting();
		app.UseHttpsRedirection();

		app.UseAuthentication();
		app.UseAuthorization();

		app.MapControllers();

		app.Run();
	}
}