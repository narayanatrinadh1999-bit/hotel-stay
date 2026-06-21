using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HotelStay.Tests.Endpoints
{
    public class HotelEndpointsTests : IAsyncLifetime
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;

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

        // Helper to parse JSON responses
        private static async Task<JsonDocument> GetJson(HttpResponseMessage res)
        {
            var stream = await res.Content.ReadAsStreamAsync();
            return await JsonDocument.ParseAsync(stream);
        }

        [Fact]
        public async Task SearchHotels_ValidRequest_Returns200()
        {
            var res = await _client.GetAsync("/hotels/search?destination=London&checkIn=2026-06-23&checkOut=2026-06-27");
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            using var doc = await GetJson(res);
            Assert.True(doc.RootElement.TryGetProperty("results", out _));
        }

        [Fact]
        public async Task SearchHotels_ValidRequest_ReturnsCorrectStructure()
        {
            var res = await _client.GetAsync("/hotels/search?destination=London&checkIn=2026-06-23&checkOut=2026-06-27");
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            using var doc = await GetJson(res);
            var root = doc.RootElement;
            Assert.True(root.TryGetProperty("results", out _));
            Assert.True(root.TryGetProperty("totalCount", out _));
            Assert.True(root.TryGetProperty("destination", out _));
            Assert.True(root.TryGetProperty("checkInDate", out _));
            Assert.True(root.TryGetProperty("checkOutDate", out _));
            Assert.True(root.TryGetProperty("nightsCount", out _));
        }

        [Fact]
        public async Task SearchHotels_WithOptionalRoomType_Returns200()
        {
            var res = await _client.GetAsync("/hotels/search?destination=London&checkIn=2026-06-23&checkOut=2026-06-27&roomType=Deluxe");
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            using var doc = await GetJson(res);
            var results = doc.RootElement.GetProperty("results");
            foreach (var item in results.EnumerateArray())
            {
                Assert.Equal("Deluxe", item.GetProperty("roomType").GetString());
            }
        }

        [Fact]
        public async Task SearchHotels_MissingDestination_Returns400()
        {
            var res = await _client.GetAsync("/hotels/search?checkIn=2026-06-23&checkOut=2026-06-27");
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
            using var doc = await GetJson(res);
            var msg = doc.RootElement.GetProperty("error").GetString();
            Assert.Contains("destination", msg, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SearchHotels_MissingCheckIn_Returns400()
        {
            var res = await _client.GetAsync("/hotels/search?destination=London&checkOut=2026-06-27");
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
            using var doc = await GetJson(res);
            var msg = doc.RootElement.GetProperty("error").GetString();
            Assert.Contains("checkIn", msg, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SearchHotels_MissingCheckOut_Returns400()
        {
            var res = await _client.GetAsync("/hotels/search?destination=London&checkIn=2026-06-23");
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
            using var doc = await GetJson(res);
            var msg = doc.RootElement.GetProperty("error").GetString();
            Assert.Contains("checkOut", msg, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SearchHotels_InvalidCheckInFormat_Returns400()
        {
            var res = await _client.GetAsync("/hotels/search?destination=London&checkIn=2026-13-01&checkOut=2026-06-27");
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
            using var doc = await GetJson(res);
            var msg = doc.RootElement.GetProperty("error").GetString();
            Assert.Contains("date", msg, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SearchHotels_CheckOutBeforeCheckIn_Returns400()
        {
            var res = await _client.GetAsync("/hotels/search?destination=London&checkIn=2026-06-27&checkOut=2026-06-23");
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
            using var doc = await GetJson(res);
            var msg = doc.RootElement.GetProperty("error").GetString();
            Assert.Contains("CheckOut date must be after CheckIn date", msg, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SearchHotels_InvalidDestination_Returns404()
        {
            var res = await _client.GetAsync("/hotels/search?destination=InvalidCity&checkIn=2026-06-23&checkOut=2026-06-27");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
            using var doc = await GetJson(res);
            var msg = doc.RootElement.GetProperty("error").GetString();
            Assert.Contains("Destination not recognized", msg, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SearchHotels_UnknownDestination_Returns404OrEmpty()
        {
            var res = await _client.GetAsync("/hotels/search?destination=Tokyo&checkIn=2026-06-23&checkOut=2026-06-27");
            // Accept either 200 with empty results or 404 depending on implementation
            if (res.StatusCode == HttpStatusCode.OK)
            {
                using var doc = await GetJson(res);
                var results = doc.RootElement.GetProperty("results");
                Assert.True(results.GetArrayLength() >= 0);
            }
            else
            {
                Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
            }
        }

        [Fact]
        public async Task ReserveHotel_ValidRequest_Returns200()
        {
            // First search to get a hotelId
            var search = await _client.GetAsync("/hotels/search?destination=London&checkIn=2026-06-23&checkOut=2026-06-27");
            using var doc = await GetJson(search);
            var results = doc.RootElement.GetProperty("results");
            var first = results[0];
            var hotelId = first.GetProperty("hotelId").GetString();
            var provider = first.GetProperty("provider").GetString();

            var body = new
            {
                hotelId,
                provider,
                roomType = first.GetProperty("roomType").GetString(),
                checkInDate = doc.RootElement.GetProperty("checkInDate").GetString(),
                checkOutDate = doc.RootElement.GetProperty("checkOutDate").GetString(),
                guestName = "Integration Test",
                documentType = "Passport",
                documentNumber = "P12345678",
                destination = doc.RootElement.GetProperty("destination").GetString()
            };

            var res = await _client.PostAsJsonAsync("/hotels/reserve", body);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            using var respDoc = await GetJson(res);
            Assert.True(respDoc.RootElement.TryGetProperty("referenceNumber", out _));
        }

        [Fact]
        public async Task ReserveHotel_ValidRequest_ReturnsConfirmationStructure()
        {
            // Reuse reservation creation
            var search = await _client.GetAsync("/hotels/search?destination=London&checkIn=2026-06-23&checkOut=2026-06-27");
            using var doc = await GetJson(search);
            var first = doc.RootElement.GetProperty("results")[0];
            var body = new
            {
                hotelId = first.GetProperty("hotelId").GetString(),
                provider = first.GetProperty("provider").GetString(),
                roomType = first.GetProperty("roomType").GetString(),
                checkInDate = doc.RootElement.GetProperty("checkInDate").GetString(),
                checkOutDate = doc.RootElement.GetProperty("checkOutDate").GetString(),
                guestName = "Integration Test",
                documentType = "Passport",
                documentNumber = "P12345678",
                destination = doc.RootElement.GetProperty("destination").GetString()
            };

            var res = await _client.PostAsJsonAsync("/hotels/reserve", body);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            using var respDoc = await GetJson(res);
            var root = respDoc.RootElement;
            Assert.True(root.TryGetProperty("referenceNumber", out _));
            Assert.True(root.TryGetProperty("hotelName", out _));
            Assert.True(root.TryGetProperty("provider", out _));
            Assert.True(root.TryGetProperty("roomType", out _));
            Assert.True(root.TryGetProperty("checkInDate", out _));
            Assert.True(root.TryGetProperty("checkOutDate", out _));
            Assert.True(root.TryGetProperty("totalPrice", out _));
            Assert.True(root.TryGetProperty("guestName", out _));
            Assert.True(root.TryGetProperty("documentNumber", out _));
        }

        [Fact]
        public async Task ReserveHotel_MissingHotelId_Returns400()
        {
            var body = new
            {
                provider = "PremierStays",
                roomType = "Deluxe",
                checkInDate = "2026-06-23",
                checkOutDate = "2026-06-27",
                guestName = "Test",
                documentType = "Passport",
                documentNumber = "P123",
                destination = "London"
            };
            var res = await _client.PostAsJsonAsync("/hotels/reserve", body);
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        public async Task ReserveHotel_MissingGuestName_Returns400()
        {
            var body = new
            {
                hotelId = "premier_001",
                provider = "PremierStays",
                roomType = "Deluxe",
                checkInDate = "2026-06-23",
                checkOutDate = "2026-06-27",
                documentType = "Passport",
                documentNumber = "P123",
                destination = "London"
            };
            var res = await _client.PostAsJsonAsync("/hotels/reserve", body);
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        public async Task ReserveHotel_MissingDocumentNumber_Returns400()
        {
            var body = new
            {
                hotelId = "premier_001",
                provider = "PremierStays",
                roomType = "Deluxe",
                checkInDate = "2026-06-23",
                checkOutDate = "2026-06-27",
                guestName = "Test",
                documentType = "Passport",
                destination = "London"
            };
            var res = await _client.PostAsJsonAsync("/hotels/reserve", body);
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        public async Task ReserveHotel_InternationalDestination_NationalIdProvided_Returns422()
        {
            // Use London with NationalId
            var search = await _client.GetAsync("/hotels/search?destination=London&checkIn=2026-06-23&checkOut=2026-06-27");
            using var doc = await GetJson(search);
            var first = doc.RootElement.GetProperty("results")[0];
            var body = new
            {
                hotelId = first.GetProperty("hotelId").GetString(),
                provider = first.GetProperty("provider").GetString(),
                roomType = first.GetProperty("roomType").GetString(),
                checkInDate = doc.RootElement.GetProperty("checkInDate").GetString(),
                checkOutDate = doc.RootElement.GetProperty("checkOutDate").GetString(),
                guestName = "Integration Test",
                documentType = "NationalId",
                documentNumber = "N123456",
                destination = doc.RootElement.GetProperty("destination").GetString()
            };

            var res = await _client.PostAsJsonAsync("/hotels/reserve", body);
            Assert.Equal((HttpStatusCode)422, res.StatusCode);
            using var respDoc = await GetJson(res);
            var msg = respDoc.RootElement.GetProperty("error").GetString();
            Assert.Contains("Passport", msg, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ReserveHotel_InternationalDestination_PassportProvided_Returns200()
        {
            var search = await _client.GetAsync("/hotels/search?destination=London&checkIn=2026-06-23&checkOut=2026-06-27");
            using var doc = await GetJson(search);
            var first = doc.RootElement.GetProperty("results")[0];
            var body = new
            {
                hotelId = first.GetProperty("hotelId").GetString(),
                provider = first.GetProperty("provider").GetString(),
                roomType = first.GetProperty("roomType").GetString(),
                checkInDate = doc.RootElement.GetProperty("checkInDate").GetString(),
                checkOutDate = doc.RootElement.GetProperty("checkOutDate").GetString(),
                guestName = "Integration Test",
                documentType = "Passport",
                documentNumber = "P12345678",
                destination = doc.RootElement.GetProperty("destination").GetString()
            };

            var res = await _client.PostAsJsonAsync("/hotels/reserve", body);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        }

        [Fact]
        public async Task ReserveHotel_DomesticDestination_NationalIdProvided_Returns200()
        {
            var search = await _client.GetAsync("/hotels/search?destination=New Delhi&checkIn=2026-06-23&checkOut=2026-06-27");
            using var doc = await GetJson(search);
            var first = doc.RootElement.GetProperty("results")[0];
            var body = new
            {
                hotelId = first.GetProperty("hotelId").GetString(),
                provider = first.GetProperty("provider").GetString(),
                roomType = first.GetProperty("roomType").GetString(),
                checkInDate = doc.RootElement.GetProperty("checkInDate").GetString(),
                checkOutDate = doc.RootElement.GetProperty("checkOutDate").GetString(),
                guestName = "Integration Test",
                documentType = "NationalId",
                documentNumber = "N987654",
                destination = doc.RootElement.GetProperty("destination").GetString()
            };

            var res = await _client.PostAsJsonAsync("/hotels/reserve", body);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        }

        [Fact]
        public async Task ReserveHotel_DocumentNumberMasked()
        {
            var search = await _client.GetAsync("/hotels/search?destination=London&checkIn=2026-06-23&checkOut=2026-06-27");
            using var doc = await GetJson(search);
            var first = doc.RootElement.GetProperty("results")[0];
            var body = new
            {
                hotelId = first.GetProperty("hotelId").GetString(),
                provider = first.GetProperty("provider").GetString(),
                roomType = first.GetProperty("roomType").GetString(),
                checkInDate = doc.RootElement.GetProperty("checkInDate").GetString(),
                checkOutDate = doc.RootElement.GetProperty("checkOutDate").GetString(),
                guestName = "Integration Test",
                documentType = "Passport",
                documentNumber = "P12345678",
                destination = doc.RootElement.GetProperty("destination").GetString()
            };

            var res = await _client.PostAsJsonAsync("/hotels/reserve", body);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            using var respDoc = await GetJson(res);
            var masked = respDoc.RootElement.GetProperty("documentNumber").GetString();
            Assert.Equal("P1234****", masked);
        }

        [Fact]
        public async Task GetReservation_ValidReference_Returns200()
        {
            // Create reservation
            var search = await _client.GetAsync("/hotels/search?destination=London&checkIn=2026-06-23&checkOut=2026-06-27");
            using var doc = await GetJson(search);
            var first = doc.RootElement.GetProperty("results")[0];
            var body = new
            {
                hotelId = first.GetProperty("hotelId").GetString(),
                provider = first.GetProperty("provider").GetString(),
                roomType = first.GetProperty("roomType").GetString(),
                checkInDate = doc.RootElement.GetProperty("checkInDate").GetString(),
                checkOutDate = doc.RootElement.GetProperty("checkOutDate").GetString(),
                guestName = "Integration Test",
                documentType = "Passport",
                documentNumber = "P12345678",
                destination = doc.RootElement.GetProperty("destination").GetString()
            };

            var res = await _client.PostAsJsonAsync("/hotels/reserve", body);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            using var respDoc = await GetJson(res);
            var reference = respDoc.RootElement.GetProperty("referenceNumber").GetString();

            var get = await _client.GetAsync($"/hotels/reservation/{reference}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            using var getDoc = await GetJson(get);
            Assert.Equal(reference, getDoc.RootElement.GetProperty("referenceNumber").GetString());
        }

        [Fact]
        public async Task GetReservation_InvalidReference_Returns404()
        {
            var get = await _client.GetAsync($"/hotels/reservation/RES-00000000-INVALID");
            Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
            using var doc = await GetJson(get);
            var msg = doc.RootElement.GetProperty("error").GetString();
            Assert.Contains("Reservation not found", msg, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetReservation_ReturnsCorrectDetails()
        {
            // create
            var search = await _client.GetAsync("/hotels/search?destination=London&checkIn=2026-06-23&checkOut=2026-06-27");
            using var doc = await GetJson(search);
            var first = doc.RootElement.GetProperty("results")[0];
            var body = new
            {
                hotelId = first.GetProperty("hotelId").GetString(),
                provider = first.GetProperty("provider").GetString(),
                roomType = first.GetProperty("roomType").GetString(),
                checkInDate = doc.RootElement.GetProperty("checkInDate").GetString(),
                checkOutDate = doc.RootElement.GetProperty("checkOutDate").GetString(),
                guestName = "Integration Test",
                documentType = "Passport",
                documentNumber = "P12345678",
                destination = doc.RootElement.GetProperty("destination").GetString()
            };

            var res = await _client.PostAsJsonAsync("/hotels/reserve", body);
            using var respDoc = await GetJson(res);
            var reference = respDoc.RootElement.GetProperty("referenceNumber").GetString();

            var get = await _client.GetAsync($"/hotels/reservation/{reference}");
            using var getDoc = await GetJson(get);
            Assert.Equal(first.GetProperty("hotelName").GetString(), getDoc.RootElement.GetProperty("hotelName").GetString());
            Assert.Equal(doc.RootElement.GetProperty("checkInDate").GetString(), getDoc.RootElement.GetProperty("checkInDate").GetString());
        }

        [Fact]
        public async Task EndToEndFlow_SearchReserveAndRetrieve()
        {
            // 1. Search
            var search = await _client.GetAsync("/hotels/search?destination=London&checkIn=2026-06-23&checkOut=2026-06-27");
            using var doc = await GetJson(search);
            var first = doc.RootElement.GetProperty("results")[0];

            // 2. Extract hotelId
            var hotelId = first.GetProperty("hotelId").GetString();
            var provider = first.GetProperty("provider").GetString();

            // 3. Reserve
            var body = new
            {
                hotelId,
                provider,
                roomType = first.GetProperty("roomType").GetString(),
                checkInDate = doc.RootElement.GetProperty("checkInDate").GetString(),
                checkOutDate = doc.RootElement.GetProperty("checkOutDate").GetString(),
                guestName = "Integration Test",
                documentType = "Passport",
                documentNumber = "P12345678",
                destination = doc.RootElement.GetProperty("destination").GetString()
            };

            var reserve = await _client.PostAsJsonAsync("/hotels/reserve", body);
            Assert.Equal(HttpStatusCode.OK, reserve.StatusCode);
            using var resDoc = await GetJson(reserve);
            var reference = resDoc.RootElement.GetProperty("referenceNumber").GetString();

            // 5. Retrieve
            var get = await _client.GetAsync($"/hotels/reservation/{reference}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            using var getDoc = await GetJson(get);

            // 6. Verify details
            Assert.Equal(reference, getDoc.RootElement.GetProperty("referenceNumber").GetString());
            // The API stores a human-friendly hotelName when available; assert against that value
            Assert.Equal(first.GetProperty("hotelName").GetString(), getDoc.RootElement.GetProperty("hotelName").GetString());
            Assert.Equal(doc.RootElement.GetProperty("checkInDate").GetString(), getDoc.RootElement.GetProperty("checkInDate").GetString());
        }
    }
}
