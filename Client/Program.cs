using Microsoft.EntityFrameworkCore;
using Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Shipment API", Version = "v1" });
});

builder.Services.AddHealthChecks();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql =>
        {
            sql.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        });

    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

CustomInjection(builder);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shipment API v1");
        c.RoutePrefix = "swagger";
    });

    app.UseCors("AllowAll");
}

app.UseHttpsRedirection();

app.UseMiddleware<Manager.MultiTenantManagerMiddleware>();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();

static void CustomInjection(WebApplicationBuilder builder)
{
    // Middlewares
    builder.Services.AddScoped<Manager.MultiTenantManagerMiddleware>();

    // AutoMapper profiles
    builder.Services.AddAutoMapper(
        typeof(Client.Utils.AutoMapperClientProfile),
        typeof(Manager.Utils.AutoMapperManagerProfile),
        typeof(Resources.Utils.AutoMapperResourceProfile));

    builder.Services.AddScoped<Services.CurrentTenantManager>();
    builder.Services.AddScoped<Infrastructure.Services.ITenantGetter>(
        sp => sp.GetRequiredService<Services.CurrentTenantManager>());
    builder.Services.AddScoped<Infrastructure.Services.ITenantSetter>(
        sp => sp.GetRequiredService<Services.CurrentTenantManager>());

    builder.Services.AddScoped<Resources.IShipmentResource, Resources.ShipmentResource>();
    builder.Services.AddScoped<Manager.IShipmentManager, Manager.ShipmentManager>();
}
