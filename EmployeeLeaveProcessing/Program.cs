using EmployeeLeaveProcessing.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Azure;
using Azure.Communication.Email;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using EmployeeLeaveProcessing.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;

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
    options.Audience = "e2ff7cc6-aa36-431e-86d2-648c7d94dcfb";
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

//string connectionString = Environment.GetEnvironmentVariable("endpoint=!--https://employee.unitedstates.communication.azure.com/;accesskey=wHphdlhYST5AWQbTYkWUeeCiN+FoNuZtLmNAH23+MVKcNH5tSg8CUV3B5JmiQGL90RPlT0uCV6CrRb9+jPWFDw==");
//EmailClient emailClient = new EmailClient(connectionString);
/*builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = "!--https://login.microsoftonline.com/{215b7ce2-5263-4593-a622-da030405d151}";
    options.Audience = "{7a9158f7-ad8c-472d-886a-1e43e75ff8f9}";

    // Other configurations...

    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name",
        RoleClaimType = "roles",
    };
});*/
builder.Services.AddControllers();

/*builder.Services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
        .AddAzureAD(options => builder.Configuration.Bind("AzureAd", options));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("!--http://yourfrontenddomain.com")
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});*/

builder.Services.AddDbContext<EmployeeDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DevConnection"));
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    });


// Learn more about configuring Swagger/OpenAPI at !--https://aka.ms/aspnetcore/swashbuckle
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
/*builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserPolicy", policy => policy.RequireClaim("email"));
});*/

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
        //c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dispatch API V1");
        //c.RoutePrefix = string.Empty;

    });
}
//app.UseCors("AllowSpecificOrigin");
app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
