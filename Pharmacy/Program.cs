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
        var app = builder.Build();

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