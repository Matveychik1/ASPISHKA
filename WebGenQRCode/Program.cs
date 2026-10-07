using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quartz;
using System.Text;
using WebGenQRCode.Data;
using WebGenQRCode.Data.Entities.Identity;
using WebGenQRCode.Extensions;
using WebGenQRCode.Interfaces;
using WebGenQRCode.Jobs;
using WebGenQRCode.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppQrDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("QrConnection")));

builder.Services.AddIdentity<UserEntity, RoleEntity>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
})
    .AddEntityFrameworkStores<AppQrDbContext>()
    .AddDefaultTokenProviders();

//Тут ми проводимо валідацію токена сервері.
//Сервер має перевірити чи він видавав даний токен
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddScoped<IDbSeeder, DbSeeder>();
builder.Services.AddScoped<IImageService, ImageOptimizationService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAccountService, AccountService>();

// Add services to the container.
//builder.Services.AddSwaggerGen(); //Додаємо swagger - кажемо, що він є
builder.Services.AddSwaggerGen(opt =>
{

    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

builder.Services.AddControllers();

const string reactCorsPolicy = "ReactClient";

var reactCorsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(reactCorsPolicy, policy =>
    {
        policy.WithOrigins(reactCorsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddQuartz(q => {
    var jobKey = new JobKey(nameof(DbSeedJob));
    q.AddJob<DbSeedJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity($"{nameof(DbSeedJob)}-trigger")
        .StartNow());
});
builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

var app = builder.Build();

try
{
    var myImage = builder.Configuration.GetRequiredSection("ImagesDir").Get<string>() ?? "myimages";
    string path = Path.Combine(Directory.GetCurrentDirectory(), myImage);
    Directory.CreateDirectory(path); //автоматично стоврить images

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(path),
        RequestPath = $"/{myImage}"
    });
}
catch (Exception ex)
{
    Console.WriteLine("Помилка запуску" + ex.Message);
}

app.UseCors(reactCorsPolicy); //дозволяємо використання cors правил

app.UseSwagger(); //Використай Swagger
app.UseSwaggerUI(); //Додай графічний інтерфейс

// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
//не потрібно бо працює через Quartz
//await app.SeedData();

app.Run();