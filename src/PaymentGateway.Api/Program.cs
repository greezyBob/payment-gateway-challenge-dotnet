using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

using FluentValidation;
using FluentValidation.AspNetCore;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;

using PaymentGateway.Api.Configuration;
using PaymentGateway.Api.External;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IPaymentsRepository, PaymentsRepository>();

builder.Services
    .AddOptions<BankOptions>()
    .Bind(builder.Configuration.GetSection("BankOptions"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient<IBankClient, BankClient>()
    .ConfigureHttpClient((provider, client) =>
    {
        var options = provider.GetRequiredService<IOptions<BankOptions>>().Value;
        client.BaseAddress = new Uri(options.BankUrl);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });

builder.Services.AddTransient<IPaymentService, PaymentService>();

// Register FluentValidation validators
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddScoped<IValidator<PostPaymentRequest>, PostPaymentRequestValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseExceptionHandler("/error");

app.Map("/error", (HttpContext context) =>
{
    var feature = context.Features.Get<IExceptionHandlerFeature>();
    var exception = feature?.Error;

    if (exception is HttpRequestException httpEx)
    {
        context.Response.StatusCode = (int)(httpEx.StatusCode ?? HttpStatusCode.BadGateway);
        return Results.Json(new { error = httpEx.Message });
    }

    context.Response.StatusCode = 500;
    return Results.Json(new { error = "An unexpected error occurred." });
});

app.MapControllers();

app.Run();
