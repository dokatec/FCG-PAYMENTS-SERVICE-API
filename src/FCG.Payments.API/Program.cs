using System.Text;
using FCG.Payments.API.Consumers;
using FCG.Payments.Application.Services;
using FCG.Payments.Domain.Interfaces;
using FCG.Payments.Infrastructure.Data;
using FCG.Payments.Infrastructure.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

builder.Services.AddDbContext<PaymentsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMassTransit(x =>
{

    x.AddConsumer<OrderPlacedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration["MessageBroker:Host"] ?? "localhost";

        cfg.Host(host, "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });


        cfg.ReceiveEndpoint("order-placed-queue", e =>
        {
            e.ConfigureConsumer<OrderPlacedConsumer>(context);
        });
    });
});

// --- Configuração de Autenticação JWT ---
var key = Encoding.ASCII.GetBytes("ChaveSuperSecretaDaFiapCloudGames2026!");

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true
    };
});
// --- Configuração de Autorização por Níveis ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("User", policy => policy.RequireRole("User"));
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "FCG Payments API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira apenas o token JWT."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// --- INÍCIO DO BLOCO DE AUTO-MIGRATION ---
// No Program.cs, após o var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    // Tenta aplicar a migration com política de retry simples
    for (int i = 0; i < 5; i++)
    {
        try
        {
            var context = services.GetRequiredService<PaymentsDbContext>();
            logger.LogInformation("--> Verificando banco de dados (Tentativa {0})...", i + 1);

            context.Database.Migrate();

            logger.LogInformation("--> Migrations aplicadas com sucesso!");
            break; // Sucesso, sai do loop
        }
        catch (Exception ex)
        {
            logger.LogWarning("--> Banco ainda não disponível. Aguardando 5 segundos...");
            Thread.Sleep(5000); // Aguarda o Postgres "acordar"

            if (i == 4) // Se for a última tentativa e falhar...
            {
                logger.LogCritical(ex, "--> Erro fatal ao tentar migrar o banco.");
                throw;
            }
        }
    }
}
// --- FIM DO BLOCO ---

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
