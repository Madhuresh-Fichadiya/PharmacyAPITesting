using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pharmacy.API.Data;
using Pharmacy.API.Services;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddSignalR();
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddDbContext<PharmacyDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure();
                }
            )
        );
        builder.Services.AddScoped<IInventoryService, InventoryService>();
        builder.Services.AddScoped<IMedicineRepository, MedicineRepository>();
        builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

        builder.Services.AddHttpClient<SupabaseService>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSignalR", policy =>
            {
                policy.WithOrigins("https://localhost:7128") // The URL of your frontend
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials(); // Required for SignalR!
            });
        });
        var app = builder.Build();
        // Use the CORS policy
        app.UseCors("AllowSignalR");

        // This creates the URL that the client uses to connect (e.g., /notificationHub)
        app.MapHub<NotificationHub>("/notificationHub");
        app.MapHub<ChatHub>("/chatHub");
        app.MapHub<WhatsAppHub>("/whatsAppHub");
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        //app.UseSwagger();
        //app.UseSwaggerUI(c => {
        //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Leave Management API V1");
        //    c.RoutePrefix = string.Empty;
        //});
        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();
        //using (var scope = app.Services.CreateScope())
        //{
        //    var db = scope.ServiceProvider.GetRequiredService<PharmacyDbContext>();
        //    db.Database.Migrate();
        //}
        app.Run();
    }
}