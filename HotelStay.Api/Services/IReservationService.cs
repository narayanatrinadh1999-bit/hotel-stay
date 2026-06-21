using HotelStay.Api.Models;

namespace HotelStay.Api.Services;

public interface IReservationService
{
    Task<ReservationConfirmation> ReserveAsync(ReservationRequest request);
    Task<ReservationConfirmation> GetReservationAsync(string referenceNumber);
}
