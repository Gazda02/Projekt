using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using WorkshopManager.API.Data;
using WorkshopManager.API.Services;
using WorkshopManager.API.Services.Interfaces;
using NLog.Web;
using Rotativa.AspNetCore;
using Microsoft.AspNetCore.Authorization;

var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Add services to the container.
    builder.Services.AddRazorPages();
    builder.Services.AddControllers();

    // Configure DbContext
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
        ));

    // Configure Identity
    builder.Services.AddIdentity<IdentityUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    // Configure password requirements
    builder.Services.Configure<IdentityOptions>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
    });

    // Configure JWT Authentication
    // builder.Services.AddAuthentication(options =>
    // {
    //     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    //     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    //     options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    // })
    // .AddJwtBearer(options =>
    // {
    //     options.SaveToken = true;
    //     options.RequireHttpsMetadata = false;
    //     options.TokenValidationParameters = new TokenValidationParameters()
    //     {
    //         ValidateIssuer = true,
    //         ValidateAudience = true,
    //         ValidAudience = builder.Configuration["JWT:ValidAudience"],
    //         ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
    //         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
    //     };
    // });
    
    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAssertion(_ => true) // pozwala na wszystko
            .Build();
    });

    builder.Services
        .AddControllers()
        .AddJsonOptions(options =>
        {
            // pozwól na ignorowanie cykli (nie zwraca ponownie referencji, tylko je pomija)
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            // opcjonalnie możesz zwiększyć maksymalną głębokość (domyślnie 32)
            options.JsonSerializerOptions.MaxDepth = 64;
        });

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Register services
    builder.Services.AddScoped<ICustomerService, CustomerService>();
    builder.Services.AddScoped<IVehicleService, VehicleService>();
    builder.Services.AddScoped<IServiceOrderService, ServiceOrderService>();
    builder.Services.AddScoped<IPartService, PartService>();
    builder.Services.AddScoped<IFileStorageService, FileStorageService>();
    builder.Services.AddScoped<IServiceTaskService, ServiceTaskService>();
    builder.Services.AddScoped<IReportService, ReportService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRouting();
    app.UseHttpsRedirection();
    app.UseStaticFiles();

    // Add authentication & authorization
    app.UseAuthentication();
    app.UseAuthorization();

    
    RotativaConfiguration.Setup(app.Environment.ContentRootPath, "Rotativa");

    
    app.MapRazorPages();
    app.MapControllers();

    // Create default roles
    using (var scope = app.Services.CreateScope())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roles = new[] { "Admin", "Mechanic", "Receptionist" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Create default admin user
        string adminEmail = "admin@workshop.com";
        string adminPassword = "Admin@1234";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                logger.Error("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    // Ensure database is created and migrations are applied
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "An error occurred while migrating the database.");
        }
    }

    app.Run();
}

catch (Exception ex)
{
    logger.Error(ex, "Stopped program because of exception");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
