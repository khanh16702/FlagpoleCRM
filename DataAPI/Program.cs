using Common.Constant;
using DataAPI.Middlewares;
using DataServiceLib;
using FlagpoleCRM.DTO;
using FlagpoleCRM.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var webUrl = builder.Configuration.GetSection("WebUrl").Value;
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration["ConnectionString"];
builder.Services.AddDbContext<FlagpoleCRMContext>(x => x.UseSqlServer(connectionString));

builder.Services.AddIdentity<AccountDTO, IdentityRole>()
    .AddEntityFrameworkStores<FlagpoleCRMContext>()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IConnectionMultiplexer>(options => ConnectionMultiplexer.Connect(new ConfigurationOptions
{
    EndPoints = { $"{builder.Configuration["RedisConnection:Host"]}:{builder.Configuration["RedisConnection:Port"]}" }
}));

builder.Services.AddDataServices();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Tokens:Key"])),
            ValidateIssuer = false,
            ValidateAudience = false
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.Response.Redirect(webUrl + "/login/authentication/index");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .WithOrigins(webUrl)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseMiddleware<CheckHeaderPrivilege>();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
