using System.Threading.Tasks;
using HotelStay.Api.Models;

namespace HotelStay.Api.Services
{
    /// <summary>
    /// Manages reservations: creating and retrieving confirmations.
    /// </summary>
    public interface IReservationService
    {
        /// <summary>
        /// Create a reservation from the provided request. Validates documents and stores the reservation.
        /// </summary>
        Task<ReservationConfirmation> ReserveAsync(ReservationRequest request);

        /// <summary>
        /// Retrieve an existing reservation by reference number.
        /// </summary>
        Task<ReservationConfirmation> GetReservationAsync(string referenceNumber);
    }
}
