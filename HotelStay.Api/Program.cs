using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HotelStay.Api.Models;
using HotelStay.Api.Services;
using HotelStay.Api.Providers;

#nullable enable

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddScoped<IDocumentValidator, DocumentValidator>();
builder.Services.AddScoped<IReservationStore, InMemoryReservationStore>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IHotelSearchService, HotelSearchService>();
builder.Services.AddScoped<IHotelProvider, PremierStaysProvider>();
builder.Services.AddScoped<IHotelProvider, BudgetNestsProvider>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors();

app.MapGet("/", () => Results.Json(new { message = "HotelStay API" }));

app.MapGet("/hotels/search", async (IHotelSearchService searchService, HttpRequest req) =>
{
    var query = req.Query;
    var destination = query["destination"].ToString();
    var checkInStr = query["checkIn"].ToString();
    var checkOutStr = query["checkOut"].ToString();
    var roomTypeStr = query["roomType"].ToString();

    if (string.IsNullOrWhiteSpace(destination))
    {
        return Results.Json(new { error = "Missing required parameter: destination" }, statusCode: 400);
    }

    if (!DateTime.TryParse(checkInStr, out var checkIn))
    {
        return Results.Json(new { error = "Invalid date format for checkIn. Use YYYY-MM-DD" }, statusCode: 400);
    }

    if (!DateTime.TryParse(checkOutStr, out var checkOut))
    {
        return Results.Json(new { error = "Invalid date format for checkOut. Use YYYY-MM-DD" }, statusCode: 400);
    }

    RoomType? roomType = null;
    if (!string.IsNullOrWhiteSpace(roomTypeStr))
    {
        if (Enum.TryParse<RoomType>(roomTypeStr, true, out var rt))
        {
            roomType = rt;
        }
        else
        {
            return Results.Json(new { error = "Invalid roomType. Allowed: Standard, Deluxe, Suite" }, statusCode: 400);
        }
    }

    var request = new HotelSearchRequest
    {
        Destination = destination,
        CheckInDate = checkIn,
        CheckOutDate = checkOut,
        RoomType = roomType
    };

    try
    {
        var resp = await searchService.SearchAsync(request);
        return Results.Json(resp);
    }
    catch (ArgumentException ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 400);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 404);
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 500);
    }
});

app.MapPost("/hotels/reserve", async (IReservationService reservationService, ReservationRequest request) =>
{
    if (request == null)
    {
        return Results.Json(new { error = "Missing request body" }, statusCode: 400);
    }

    // Basic required fields
    if (string.IsNullOrWhiteSpace(request.HotelId) || string.IsNullOrWhiteSpace(request.Provider) || string.IsNullOrWhiteSpace(request.GuestName) || string.IsNullOrWhiteSpace(request.DocumentNumber))
    {
        return Results.Json(new { error = "Missing required field" }, statusCode: 400);
    }

    try
    {
        var conf = await reservationService.ReserveAsync(request);
        return Results.Json(conf);
    }
    catch (InvalidOperationException ex)
    {
        // Document validation failure -> 422
        return Results.Json(new { error = ex.Message }, statusCode: 422);
    }
    catch (ArgumentException ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 400);
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 500);
    }
});

app.MapGet("/hotels/reservation/{reference}", async (IReservationService reservationService, string reference) =>
{
    try
    {
        var conf = await reservationService.GetReservationAsync(reference);
        return Results.Json(conf);
    }
    catch (KeyNotFoundException)
    {
        return Results.Json(new { error = "Reservation not found" }, statusCode: 404);
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 500);
    }
});

app.Run();

public partial class Program { }
