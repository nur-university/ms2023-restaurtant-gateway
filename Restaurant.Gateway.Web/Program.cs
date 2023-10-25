using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Restaurant.Gateway.Web.Aggregator;
using Restaurant.Gateway.Web.Config;
using Restaurant.Gateway.Web.Handlers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

JwtOptions jwtoptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(jwtOptions =>
{
    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtoptions.SecretKey));
    jwtOptions.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ValidateIssuer = jwtoptions.ValidateIssuer,
        ValidateAudience = jwtoptions.ValidateAudience,
        ValidIssuer = jwtoptions.ValidIssuer,
        ValidAudience = jwtoptions.ValidAudience
    };
});

builder.Configuration.AddJsonFile("ocelot.json");
// Add services to the container.
builder.Services.AddOcelot()
                .AddSingletonDefinedAggregator<MenuItemDetailAggregator>()
                .AddDelegatingHandler<LogDelegatingHandler>(true);


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

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
await app.UseOcelot();

app.MapControllers();


app.Run();
