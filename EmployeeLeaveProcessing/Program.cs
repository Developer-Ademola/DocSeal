using Azure.Communication.Email;
using EmployeeLeaveProcessing.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
 .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi()
            .AddDownstreamApi("DownstreamApi", builder.Configuration.GetSection("DownstreamApi"))
            .AddInMemoryTokenCaches();

builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Authority = "https://login.microsoftonline.com/215b7ce2-5263-4593-a622-da030405d151/v2.0";
    options.Audience = "6c87efe4-6da2-43e6-8f78-b151a0764d6b";
    options.RequireHttpsMetadata = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "email",
        RoleClaimType = "roles"
    };
});
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});
builder.Services.AddSingleton<EmailClient>(_ => new EmailClient(builder.Configuration.GetConnectionString("CommunicationServices")));


builder.Services.AddControllers();
//Add Database Context
builder.Services.AddDbContext<EmployeeDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DevConnection"));
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
 c =>
 {
     c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Employee Management System API", Version = "v1" });
     c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
     {
         Description = "Oauth2.0 which uses AuthorizationCode flow",
         Name = "Oauth2",
         Type = SecuritySchemeType.OAuth2,
         Flows = new OpenApiOAuthFlows
         {
             AuthorizationCode = new OpenApiOAuthFlow
             {
                 AuthorizationUrl = new Uri(builder.Configuration["SwaggerAzureAD:Authorization"]),
                 TokenUrl = new Uri(builder.Configuration["SwaggerAzureAD:TokenUrl"]),
                 Scopes = new Dictionary<string, string>
                {
                    {builder.Configuration["SwaggerAzureAD:Scope"], "Access API as User" }
                }
             }
         }

     });
     c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference{Type=ReferenceType.SecurityScheme,Id="oauth2"}
            },
            new[]{ builder.Configuration["SwaggerAzureAD:Scope"] }
        }
    });
 });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.OAuthClientId(builder.Configuration["SwaggerAzureAD: ClientId"]);
        c.OAuthUsePkce();
        c.OAuthScopeSeparator(" ");
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dispatch API V1");
        c.RoutePrefix = string.Empty;

    });
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
