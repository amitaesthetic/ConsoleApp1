using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

IdentityModelEventSource.ShowPII = true;
var validIssuer = new List<string>() { "https://sts.windows.net/" + builder.Configuration["AzureAd:TenantId"] };

var configManager = new ConfigurationManager<OpenIdConnectConfiguration>($"{validIssuer.Last()}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());

var openidconfig = configManager.GetConfigurationAsync().Result;

builder.Services.AddAuthentication(option =>
{
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = false,
        ValidateAudience = true,
        ValidAudiences = new List<string>
        {
            "3e534de3-f91f-45f8-9a15-68b6fc2772a3"
        },
        ValidateIssuerSigningKey = true,
        IssuerSigningKeys = openidconfig.SigningKeys,
        //ValidIssuers = validIssuer
    };
    options.Events = new JwtBearerEvents()
    {
        OnAuthenticationFailed = AuthenticationFailed
    };
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


Task AuthenticationFailed(AuthenticationFailedContext context)
{
    var s = $"Failed : {context.Exception.Message}";
    context.Response.ContentLength = s.Length;
    var abc = context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(s), 0, s.Length);
    return Task.FromResult(0);
}