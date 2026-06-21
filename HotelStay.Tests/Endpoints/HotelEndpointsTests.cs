using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using HotelStay.Api.Models;

namespace HotelStay.Tests.Endpoints;

public class HotelEndpointsTests : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        _factory?.Dispose();
        await Task.CompletedTask;
    }

    // ── Search Endpoint Tests ──────────────────────────────────────────

    [Fact]
    public async Task SearchHotels_ValidRequest_Returns200()
    {
        var response = await _client.GetAsync(
            "/hotels/search?destination=London&checkIn=2026-06-23&checkOut=2026-06-27");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SearchHotels_ValidRequest_ReturnsCorrectStructure()
    {
        var response = await _client.GetAsync(
            "/hotels/search?destination=London&checkIn=2026-06-23&checkOut=2026-06-27");

        var json = await response.Content.ReadAsStringAsync();
        var obj = JsonNode.Parse(json)!.AsObject();

        Assert.NotNull(obj["results"]);
        Assert.NotNull(obj["totalCount"]);
        Assert.NotNull(obj["destination"]);
        Assert.NotNull(obj["checkInDate"]);
        Assert.NotNull(obj["checkOutDate"]);
        Assert.NotNull(obj["nightsCount"]);
    }

    [Fact]
    public async Task SearchHotels_WithOptionalRoomType_Returns200()
    {
        var response = await _client.GetAsync(
            "/hotels/search?destination=London&checkIn=2026-06-23&checkOut=2026-06-27&roomType=Deluxe");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var obj = JsonNode.Parse(json)!.AsObject();
        var results = obj["results"]!.AsArray();

        foreach (var room in results)
        {
            Assert.Equal("Deluxe", room!["roomType"]!.GetValue<string>());
        }
    }

    [Fact]
    public async Task SearchHotels_MissingDestination_Returns400()
    {
        var response = await _client.GetAsync(
            "/hotels/search?checkIn=2026-06-23&checkOut=2026-06-27");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("destination", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SearchHotels_MissingCheckIn_Returns400()
    {
        var response = await _client.GetAsync(
            "/hotels/search?destination=London&checkOut=2026-06-27");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("checkIn", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SearchHotels_MissingCheckOut_Returns400()
    {
        var response = await _client.GetAsync(
            "/hotels/search?destination=London&checkIn=2026-06-23");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("checkOut", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SearchHotels_InvalidCheckInFormat_Returns400()
    {
        var response = await _client.GetAsync(
            "/hotels/search?destination=London&checkIn=2026-13-01&checkOut=2026-06-27");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SearchHotels_CheckOutBeforeCheckIn_Returns400()
    {
        var response = await _client.GetAsync(
            "/hotels/search?destination=London&checkIn=2026-06-27&checkOut=2026-06-23");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SearchHotels_InvalidDestination_Returns404()
    {
        var response = await _client.GetAsync(
            "/hotels/search?destination=InvalidCity&checkIn=2026-06-23&checkOut=2026-06-27");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SearchHotels_DomesticDestination_Returns200()
    {
        var response = await _client.GetAsync(
            "/hotels/search?destination=New%20Delhi&checkIn=2026-06-23&checkOut=2026-06-27");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── Reserve Endpoint Tests ─────────────────────────────────────────

    [Fact]
    public async Task ReserveHotel_ValidRequest_Returns200()
    {
        var request = CreateValidReservationRequest();

        var response = await _client.PostAsJsonAsync("/hotels/reserve", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ReserveHotel_ValidRequest_ReturnsConfirmationStructure()
    {
        var request = CreateValidReservationRequest();

        var response = await _client.PostAsJsonAsync("/hotels/reserve", request);
        var json = await response.Content.ReadAsStringAsync();
        var obj = JsonNode.Parse(json)!.AsObject();

        Assert.NotNull(obj["referenceNumber"]);
        Assert.NotNull(obj["hotelName"]);
        Assert.NotNull(obj["provider"]);
        Assert.NotNull(obj["roomType"]);
        Assert.NotNull(obj["checkInDate"]);
        Assert.NotNull(obj["checkOutDate"]);
        Assert.NotNull(obj["guestName"]);
        Assert.NotNull(obj["documentNumber"]);
    }

    [Fact]
    public async Task ReserveHotel_MissingHotelId_Returns400()
    {
        var request = new
        {
            Provider = "PremierStays",
            RoomType = "Deluxe",
            CheckInDate = "2026-06-23",
            CheckOutDate = "2026-06-27",
            GuestName = "John Doe",
            DocumentType = "Passport",
            DocumentNumber = "P12345678",
            Destination = "London",
            HotelName = "Grand Plaza Hotel",
            PricePerNight = 250
        };

        var response = await _client.PostAsJsonAsync("/hotels/reserve", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ReserveHotel_MissingGuestName_Returns400()
    {
        var request = new
        {
            HotelId = "premier_001",
            Provider = "PremierStays",
            RoomType = "Deluxe",
            CheckInDate = "2026-06-23",
            CheckOutDate = "2026-06-27",
            DocumentType = "Passport",
            DocumentNumber = "P12345678",
            Destination = "London",
            HotelName = "Grand Plaza Hotel",
            PricePerNight = 250
        };

        var response = await _client.PostAsJsonAsync("/hotels/reserve", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ReserveHotel_MissingDocumentNumber_Returns400()
    {
        var request = new
        {
            HotelId = "premier_001",
            Provider = "PremierStays",
            RoomType = "Deluxe",
            CheckInDate = "2026-06-23",
            CheckOutDate = "2026-06-27",
            GuestName = "John Doe",
            DocumentType = "Passport",
            Destination = "London",
            HotelName = "Grand Plaza Hotel",
            PricePerNight = 250
        };

        var response = await _client.PostAsJsonAsync("/hotels/reserve", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ReserveHotel_InternationalDestination_NationalIdProvided_Returns422()
    {
        var request = new
        {
            HotelId = "premier_001",
            Provider = "PremierStays",
            RoomType = "Deluxe",
            CheckInDate = "2026-06-23",
            CheckOutDate = "2026-06-27",
            GuestName = "John Doe",
            DocumentType = "NationalId",
            DocumentNumber = "N12345678",
            Destination = "London",
            HotelName = "Grand Plaza Hotel",
            PricePerNight = 250
        };

        var response = await _client.PostAsJsonAsync("/hotels/reserve", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("Passport is required", json);
    }

    [Fact]
    public async Task ReserveHotel_InternationalDestination_PassportProvided_Returns200()
    {
        var request = new
        {
            HotelId = "premier_001",
            Provider = "PremierStays",
            RoomType = "Deluxe",
            CheckInDate = "2026-06-23",
            CheckOutDate = "2026-06-27",
            GuestName = "John Doe",
            DocumentType = "Passport",
            DocumentNumber = "P12345678",
            Destination = "London",
            HotelName = "Grand Plaza Hotel",
            PricePerNight = 250
        };

        var response = await _client.PostAsJsonAsync("/hotels/reserve", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ReserveHotel_DomesticDestination_NationalIdProvided_Returns200()
    {
        var request = new
        {
            HotelId = "budget_001",
            Provider = "BudgetNests",
            RoomType = "Standard",
            CheckInDate = "2026-06-23",
            CheckOutDate = "2026-06-27",
            GuestName = "Jane Doe",
            DocumentType = "NationalId",
            DocumentNumber = "N12345678",
            Destination = "New Delhi",
            HotelName = "Budget Hostel & Rooms",
            PricePerNight = 60
        };

        var response = await _client.PostAsJsonAsync("/hotels/reserve", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ReserveHotel_DocumentNumberMasked()
    {
        var request = new
        {
            HotelId = "premier_001",
            Provider = "PremierStays",
            RoomType = "Deluxe",
            CheckInDate = "2026-06-23",
            CheckOutDate = "2026-06-27",
            GuestName = "John Doe",
            DocumentType = "Passport",
            DocumentNumber = "P12345678",
            Destination = "London",
            HotelName = "Grand Plaza Hotel",
            PricePerNight = 250
        };

        var response = await _client.PostAsJsonAsync("/hotels/reserve", request);

        var json = await response.Content.ReadAsStringAsync();
        var obj = JsonNode.Parse(json)!.AsObject();

        Assert.Equal("P1234****", obj["documentNumber"]!.GetValue<string>());
    }

    // ── Get Reservation Endpoint Tests ────────────────────────────────

    [Fact]
    public async Task GetReservation_ValidReference_Returns200()
    {
        var reserveRequest = CreateValidReservationRequest();
        var reserveResponse = await _client.PostAsJsonAsync("/hotels/reserve", reserveRequest);
        var reserveJson = await reserveResponse.Content.ReadAsStringAsync();
        var reserveObj = JsonNode.Parse(reserveJson)!.AsObject();
        var reference = reserveObj["referenceNumber"]!.GetValue<string>();

        var getResponse = await _client.GetAsync($"/hotels/reservation/{reference}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetReservation_InvalidReference_Returns404()
    {
        var response = await _client.GetAsync("/hotels/reservation/RES-00000000-INVALID");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("Reservation not found", json);
    }

    [Fact]
    public async Task GetReservation_ReturnsCorrectDetails()
    {
        var reserveRequest = CreateValidReservationRequest();
        var reserveResponse = await _client.PostAsJsonAsync("/hotels/reserve", reserveRequest);
        var reserveJson = await reserveResponse.Content.ReadAsStringAsync();
        var reserveObj = JsonNode.Parse(reserveJson)!.AsObject();
        var reference = reserveObj["referenceNumber"]!.GetValue<string>();

        var getResponse = await _client.GetAsync($"/hotels/reservation/{reference}");
        var getJson = await getResponse.Content.ReadAsStringAsync();
        var getObj = JsonNode.Parse(getJson)!.AsObject();

        Assert.Equal(reference, getObj["referenceNumber"]!.GetValue<string>());
        Assert.Equal("Grand Plaza Hotel", getObj["hotelName"]!.GetValue<string>());
    }

    // ── End-to-End Flow Test ──────────────────────────────────────────

    [Fact]
    public async Task EndToEndFlow_SearchReserveAndRetrieve()
    {
        // 1. Search for hotels
        var searchResponse = await _client.GetAsync(
            "/hotels/search?destination=London&checkIn=2026-06-23&checkOut=2026-06-27");
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);

        var searchJson = await searchResponse.Content.ReadAsStringAsync();
        var searchObj = JsonNode.Parse(searchJson)!.AsObject();
        var results = searchObj["results"]!.AsArray();
        Assert.NotEmpty(results);

        var firstResult = results[0]!.AsObject();
        var hotelId = firstResult["hotelId"]!.GetValue<string>();
        var hotelName = firstResult["hotelName"]!.GetValue<string>();
        var provider = firstResult["provider"]!.GetValue<string>();
        var pricePerNight = firstResult["pricePerNight"]!.GetValue<decimal>();

        // 2. Reserve the hotel
        var reserveRequest = new
        {
            HotelId = hotelId,
            Provider = provider,
            RoomType = "Standard",
            CheckInDate = "2026-06-23",
            CheckOutDate = "2026-06-27",
            GuestName = "John Doe",
            DocumentType = "Passport",
            DocumentNumber = "P12345678",
            Destination = "London",
            HotelName = hotelName,
            PricePerNight = pricePerNight
        };

        var reserveResponse = await _client.PostAsJsonAsync("/hotels/reserve", reserveRequest);
        Assert.Equal(HttpStatusCode.OK, reserveResponse.StatusCode);

        var reserveJson = await reserveResponse.Content.ReadAsStringAsync();
        var reserveObj = JsonNode.Parse(reserveJson)!.AsObject();
        var referenceNumber = reserveObj["referenceNumber"]!.GetValue<string>();
        Assert.NotEmpty(referenceNumber);

        // 3. Retrieve reservation
        var getResponse = await _client.GetAsync($"/hotels/reservation/{referenceNumber}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var getJson = await getResponse.Content.ReadAsStringAsync();
        var getObj = JsonNode.Parse(getJson)!.AsObject();

        Assert.Equal(referenceNumber, getObj["referenceNumber"]!.GetValue<string>());
        Assert.Equal(hotelName, getObj["hotelName"]!.GetValue<string>());
    }

    private static object CreateValidReservationRequest() => new
    {
        HotelId = "premier_001",
        Provider = "PremierStays",
        RoomType = "Deluxe",
        CheckInDate = "2026-06-23",
        CheckOutDate = "2026-06-27",
        GuestName = "John Doe",
        DocumentType = "Passport",
        DocumentNumber = "P12345678",
        Destination = "London",
        HotelName = "Grand Plaza Hotel",
        PricePerNight = 250,
        CancellationPolicy = new
        {
            Type = "FreeCancellation",
            HoursBeforeCheckIn = 48
        }
    };
}
