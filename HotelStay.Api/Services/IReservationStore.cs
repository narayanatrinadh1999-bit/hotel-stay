using HotelStay.Api.Models;

namespace HotelStay.Api.Services
{
    /// <summary>
    /// Simple persistence abstraction for reservation confirmations.
    /// </summary>
    public interface IReservationStore
    {
        /// <summary>
        /// Save a reservation confirmation by reference number.
        /// </summary>
        void Save(string referenceNumber, ReservationConfirmation confirmation);

        /// <summary>
        /// Retrieve a saved confirmation by reference number, or null if not found.
        /// </summary>
        ReservationConfirmation GetByReference(string referenceNumber);

        /// <summary>
        /// Check whether a reference already exists.
        /// </summary>
        bool Exists(string referenceNumber);
    }
}
