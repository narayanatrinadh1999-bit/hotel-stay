using Xunit;
using Moq;
using HotelStay.Api.Services;
using HotelStay.Api.Models;
using HotelStay.Api.Store;
using System.Text.RegularExpressions;

namespace HotelStay.Tests.Services;

public class ReservationServiceTests
{
    private readonly Mock<IDocumentValidator> _validatorMock;
    private readonly Mock<IReservationStore> _storeMock;
    private readonly ReservationService _service;

    public ReservationServiceTests()
    {
        _validatorMock = new Mock<IDocumentValidator>();
        _storeMock = new Mock<IReservationStore>();

        _validatorMock
            .Setup(v => v.ValidateDocument(It.IsAny<string>(), It.IsAny<DocumentType>()))
            .Returns((true, (string)null!));

        _service = new ReservationService(_validatorMock.Object, _storeMock.Object);
    }

    private static ReservationRequest CreateValidRequest(
        string destination = "New Delhi",
        DocumentType documentType = DocumentType.NationalId,
        string documentNumber = "N12345678",
        decimal pricePerNight = 100m)
    {
        return new ReservationRequest
        {
            HotelId = "premier_001",
            Provider = "PremierStays",
            RoomType = RoomType.Deluxe,
            CheckInDate = new DateTime(2026, 6, 23),
            CheckOutDate = new DateTime(2026, 6, 27),
            GuestName = "John Doe",
            DocumentType = documentType,
            DocumentNumber = documentNumber,
            Destination = destination,
            HotelName = "Grand Plaza Hotel",
            PricePerNight = pricePerNight,
            CancellationPolicy = new CancellationPolicy
            {
                Type = CancellationPolicyType.FreeCancellation,
                HoursBeforeCheckIn = 48
            }
        };
    }

    [Fact]
    public async Task ReserveAsync_ValidRequest_ReturnsConfirmation()
    {
        var request = CreateValidRequest();

        var confirmation = await _service.ReserveAsync(request);

        Assert.NotNull(confirmation);
        Assert.NotEmpty(confirmation.ReferenceNumber);
    }

    [Fact]
    public async Task ReserveAsync_GeneratesUniqueReferenceNumber()
    {
        var request = CreateValidRequest();

        var confirmation = await _service.ReserveAsync(request);

        Assert.Matches(@"^RES-\d{8}-[A-Z0-9]{6}$", confirmation.ReferenceNumber);
    }

    [Fact]
    public async Task ReserveAsync_MasksDocumentNumber()
    {
        var request = CreateValidRequest(documentNumber: "P12345678");

        var confirmation = await _service.ReserveAsync(request);

        Assert.Equal("P1234****", confirmation.DocumentNumber);
    }

    [Fact]
    public async Task ReserveAsync_DocumentValidationFails_ThrowsInvalidOperationException()
    {
        _validatorMock
            .Setup(v => v.ValidateDocument(It.IsAny<string>(), It.IsAny<DocumentType>()))
            .Returns((false, "Document validation failed"));

        var request = CreateValidRequest();

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ReserveAsync(request));
    }

    [Fact]
    public async Task ReserveAsync_InternationalDestination_NationalId_ThrowsInvalidOperation()
    {
        _validatorMock
            .Setup(v => v.ValidateDocument("London", DocumentType.NationalId))
            .Returns((false, "London is an international destination. Passport is required, NationalId is not accepted."));

        var request = CreateValidRequest(destination: "London", documentType: DocumentType.NationalId);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ReserveAsync(request));
        Assert.Contains("Passport is required", ex.Message);
    }

    [Fact]
    public async Task ReserveAsync_DomesticDestination_NationalId_Succeeds()
    {
        var request = CreateValidRequest(destination: "New Delhi", documentType: DocumentType.NationalId);

        var confirmation = await _service.ReserveAsync(request);

        Assert.NotNull(confirmation);
    }

    [Fact]
    public async Task ReserveAsync_StoresReservationInStore()
    {
        var request = CreateValidRequest();

        await _service.ReserveAsync(request);

        _storeMock.Verify(s => s.Save(It.IsAny<Reservation>()), Times.Once);
    }

    [Fact]
    public async Task ReserveAsync_CalculatesTotalPrice()
    {
        var request = CreateValidRequest(pricePerNight: 100m);

        var confirmation = await _service.ReserveAsync(request);

        Assert.Equal(400m, confirmation.TotalPrice);
    }

    [Fact]
    public async Task ReserveAsync_IncludesHotelDetails()
    {
        var request = CreateValidRequest();

        var confirmation = await _service.ReserveAsync(request);

        Assert.Equal("Grand Plaza Hotel", confirmation.HotelName);
        Assert.Equal("PremierStays", confirmation.Provider);
        Assert.Equal(RoomType.Deluxe, confirmation.RoomType);
        Assert.Equal(new DateTime(2026, 6, 23), confirmation.CheckInDate);
        Assert.Equal(new DateTime(2026, 6, 27), confirmation.CheckOutDate);
    }

    [Fact]
    public async Task GetReservationAsync_ValidReference_ReturnsConfirmation()
    {
        var request = CreateValidRequest();
        var reservation = new Reservation
        {
            ReferenceNumber = "RES-20260623-ABC123",
            Request = request,
            ReservedAt = DateTime.UtcNow,
            TotalPrice = 400m,
            CancellationPolicy = request.CancellationPolicy!
        };

        _storeMock
            .Setup(s => s.GetByReference("RES-20260623-ABC123"))
            .Returns(reservation);

        var confirmation = await _service.GetReservationAsync("RES-20260623-ABC123");

        Assert.NotNull(confirmation);
        Assert.Equal("RES-20260623-ABC123", confirmation.ReferenceNumber);
    }

    [Fact]
    public async Task GetReservationAsync_InvalidReference_ThrowsKeyNotFoundException()
    {
        _storeMock
            .Setup(s => s.GetByReference("RES-00000000-INVALID"))
            .Returns((Reservation?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.GetReservationAsync("RES-00000000-INVALID"));
    }

    [Theory]
    [InlineData(DocumentType.Passport)]
    [InlineData(DocumentType.NationalId)]
    public async Task ReserveAsync_DifferentDocumentTypes_ReturnsCorrectType(DocumentType documentType)
    {
        var request = CreateValidRequest(documentType: documentType);

        var confirmation = await _service.ReserveAsync(request);

        Assert.Equal(documentType, confirmation.DocumentType);
    }
}
