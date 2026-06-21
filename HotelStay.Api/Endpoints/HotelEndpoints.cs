using HotelStay.Api.Models;
using HotelStay.Api.Services;

namespace HotelStay.Api.Endpoints;

public static class HotelEndpoints
{
    public static void MapHotelEndpoints(this WebApplication app)
    {
        app.MapGet("/hotels/search", SearchHotels);
        app.MapPost("/hotels/reserve", ReserveHotel);
        app.MapGet("/hotels/reservation/{referenceNumber}", GetReservation);
    }

    private static async Task<IResult> SearchHotels(
        string? destination,
        string? checkIn,
        string? checkOut,
        string? roomType,
        IHotelSearchService searchService)
    {
        if (string.IsNullOrWhiteSpace(destination))
        {
            return Results.BadRequest(new { error = "Missing required parameter: destination" });
        }

        if (string.IsNullOrWhiteSpace(checkIn))
        {
            return Results.BadRequest(new { error = "Missing required parameter: checkIn" });
        }

        if (string.IsNullOrWhiteSpace(checkOut))
        {
            return Results.BadRequest(new { error = "Missing required parameter: checkOut" });
        }

        if (!DateTime.TryParse(checkIn, out var checkInDate))
        {
            return Results.BadRequest(new { error = "Invalid date format. Use YYYY-MM-DD" });
        }

        if (!DateTime.TryParse(checkOut, out var checkOutDate))
        {
            return Results.BadRequest(new { error = "Invalid date format. Use YYYY-MM-DD" });
        }

        RoomType? roomTypeFilter = null;
        if (!string.IsNullOrWhiteSpace(roomType))
        {
            if (!Enum.TryParse<RoomType>(roomType, true, out var parsedRoomType))
            {
                return Results.BadRequest(new { error = $"Invalid room type: {roomType}" });
            }
            roomTypeFilter = parsedRoomType;
        }

        var request = new HotelSearchRequest
        {
            Destination = destination,
            CheckInDate = checkInDate,
            CheckOutDate = checkOutDate,
            RoomType = roomTypeFilter
        };

        try
        {
            var response = await searchService.SearchAsync(request);
            return Results.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ReserveHotel(
        ReservationRequest? request,
        IReservationService reservationService)
    {
        if (request == null)
        {
            return Results.BadRequest(new { error = "Request body is required" });
        }

        if (string.IsNullOrWhiteSpace(request.HotelId))
        {
            return Results.BadRequest(new { error = "Missing required field: hotelId" });
        }

        if (string.IsNullOrWhiteSpace(request.GuestName))
        {
            return Results.BadRequest(new { error = "Missing required field: guestName" });
        }

        if (string.IsNullOrWhiteSpace(request.DocumentNumber))
        {
            return Results.BadRequest(new { error = "Missing required field: documentNumber" });
        }

        try
        {
            var confirmation = await reservationService.ReserveAsync(request);
            return Results.Ok(confirmation);
        }
        catch (InvalidOperationException ex)
        {
            return Results.UnprocessableEntity(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetReservation(
        string referenceNumber,
        IReservationService reservationService)
    {
        try
        {
            var confirmation = await reservationService.GetReservationAsync(referenceNumber);
            return Results.Ok(confirmation);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound(new { error = "Reservation not found" });
        }
    }
}
