using Api.Configuration;
using Api.Data;
using Api.Model;
using Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Stripe
var StripeSetting = builder.Configuration.GetSection("StripeSetting");
builder.Services.Configure<StripeOptions>(options =>
{
    options.PublishableKey = StripeSetting.GetSection("STRIPE_PUBLISHABLE_KEY").Value!;
    options.SecretKey = StripeSetting.GetSection("STRIPE_SECRET_KEY").Value!;
    options.WebhookSecret = StripeSetting.GetSection("STRIPE_WEBHOOK_SECRET").Value!;
});

////Add DbContext to Services
builder.Services.AddDbContext<AppDbContext>(options =>
                                        options.UseNpgsql(builder.Configuration.GetConnectionString("postgres")
));
builder.Services.AddIdentity<AppUser,IdentityRole>().AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

//Authentication
var JwtSetting = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(opt=>
{
    opt.DefaultAuthenticateScheme=JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme=JwtBearerDefaults.AuthenticationScheme;  
    opt.DefaultScheme=JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt =>
{
    opt.SaveToken = true;
    opt.RequireHttpsMetadata = false;
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew=TimeSpan.Zero,
        ValidateIssuerSigningKey = true,
        ValidAudience = JwtSetting["ValidAudience"],
        ValidIssuer= JwtSetting["ValidIssuer"],
        IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSetting["securityKey"]))
    };

});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//Email Service
var emailSettings = builder.Configuration.GetSection("EmailSettings");
var defaultFromEmail = emailSettings["DefaultFromEmail"];
var host = emailSettings["Host"];
var port = emailSettings.GetValue<int>("Port");
var userName = emailSettings["UserName"];
var password = emailSettings["Password"];
builder.Services.AddFluentEmail(defaultFromEmail)
    .AddSmtpSender(host, port, userName, password);
builder.Services.AddTransient<IEmailService,EmailService>();    
builder.Services.AddTransient<IEmailSender,EmailSender>();


var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseCors(options =>
{
    options.AllowAnyHeader();
    options.AllowAnyMethod();
    options.AllowAnyOrigin();
});

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
