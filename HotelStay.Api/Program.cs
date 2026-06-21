using System.Text.Json.Serialization;
using HotelStay.Api.Endpoints;
using HotelStay.Api.Providers;
using HotelStay.Api.Services;
using HotelStay.Api.Store;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddScoped<IHotelProvider, PremierStaysProvider>();
builder.Services.AddScoped<IHotelProvider, BudgetNestsProvider>();
builder.Services.AddScoped<IHotelSearchService, HotelSearchService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IDocumentValidator, DocumentValidator>();
builder.Services.AddSingleton<IReservationStore, InMemoryReservationStore>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHotelEndpoints();

app.Run();

public partial class Program { }
