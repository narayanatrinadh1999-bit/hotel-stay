using HotelStay.Api.Models;
using HotelStay.Api.Store;

namespace HotelStay.Api.Services;

public class ReservationService : IReservationService
{
    private readonly IDocumentValidator _documentValidator;
    private readonly IReservationStore _store;

    public ReservationService(IDocumentValidator documentValidator, IReservationStore store)
    {
        _documentValidator = documentValidator;
        _store = store;
    }

    public async Task<ReservationConfirmation> ReserveAsync(ReservationRequest request)
    {
        var (isValid, errorMessage) = _documentValidator.ValidateDocument(
            request.Destination, request.DocumentType);

        if (!isValid)
        {
            throw new InvalidOperationException(errorMessage);
        }

        var referenceNumber = GenerateReferenceNumber();
        var reservedAt = DateTime.UtcNow;
        var nightsCount = (request.CheckOutDate - request.CheckInDate).Days;
        var totalPrice = request.PricePerNight * nightsCount;

        var cancellationPolicy = request.CancellationPolicy ?? new CancellationPolicy
        {
            Type = CancellationPolicyType.NonRefundable,
            HoursBeforeCheckIn = 0
        };

        var reservation = new Reservation
        {
            ReferenceNumber = referenceNumber,
            Request = request,
            ReservedAt = reservedAt,
            TotalPrice = totalPrice,
            CancellationPolicy = cancellationPolicy
        };

        _store.Save(reservation);

        var confirmation = new ReservationConfirmation
        {
            ReferenceNumber = referenceNumber,
            ReservedAt = reservedAt,
            HotelName = request.HotelName,
            Provider = request.Provider,
            RoomType = request.RoomType,
            CheckInDate = request.CheckInDate,
            CheckOutDate = request.CheckOutDate,
            NightsCount = nightsCount,
            PricePerNight = request.PricePerNight,
            TotalPrice = totalPrice,
            CancellationPolicy = cancellationPolicy,
            GuestName = request.GuestName,
            DocumentType = request.DocumentType,
            DocumentNumber = MaskDocumentNumber(request.DocumentNumber)
        };

        return await Task.FromResult(confirmation);
    }

    public async Task<ReservationConfirmation> GetReservationAsync(string referenceNumber)
    {
        var reservation = _store.GetByReference(referenceNumber);
        if (reservation == null)
        {
            throw new KeyNotFoundException($"Reservation not found");
        }

        var request = reservation.Request;
        var nightsCount = (request.CheckOutDate - request.CheckInDate).Days;

        var confirmation = new ReservationConfirmation
        {
            ReferenceNumber = reservation.ReferenceNumber,
            ReservedAt = reservation.ReservedAt,
            HotelName = request.HotelName,
            Provider = request.Provider,
            RoomType = request.RoomType,
            CheckInDate = request.CheckInDate,
            CheckOutDate = request.CheckOutDate,
            NightsCount = nightsCount,
            PricePerNight = request.PricePerNight,
            TotalPrice = reservation.TotalPrice,
            CancellationPolicy = reservation.CancellationPolicy,
            GuestName = request.GuestName,
            DocumentType = request.DocumentType,
            DocumentNumber = MaskDocumentNumber(request.DocumentNumber)
        };

        return await Task.FromResult(confirmation);
    }

    private static string MaskDocumentNumber(string documentNumber)
    {
        if (string.IsNullOrEmpty(documentNumber) || documentNumber.Length < 5)
        {
            return documentNumber;
        }
        return documentNumber.Substring(0, 5) + "****";
    }

    private static string GenerateReferenceNumber()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var unique = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        return $"RES-{date}-{unique}";
    }
}
