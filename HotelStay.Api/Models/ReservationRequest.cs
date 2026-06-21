using System;

namespace HotelStay.Api.Models
{
    /// <summary>
    /// Request model used to create a reservation for a specific hotel room.
    /// </summary>
    public class ReservationRequest
    {
        /// <summary>
        /// Destination city for the reservation. Required for document validation.
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// Hotel identifier from the provider (e.g. "premier_001").
        /// </summary>
        public string HotelId { get; set; }

        /// <summary>
        /// Provider name ("PremierStays" or "BudgetNests").
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Room type being reserved.
        /// </summary>
        public RoomType RoomType { get; set; }

        /// <summary>
        /// Check-in date for the reservation.
        /// </summary>
        public DateTime CheckInDate { get; set; }

        /// <summary>
        /// Check-out date for the reservation.
        /// </summary>
        public DateTime CheckOutDate { get; set; }

        /// <summary>
        /// Guest full name. Required.
        /// </summary>
        public string GuestName { get; set; }

        /// <summary>
        /// Type of identity document provided (Passport or NationalId).
        /// </summary>
        public DocumentType DocumentType { get; set; }

        /// <summary>
        /// Document number (passport or national id). Required, non-empty.
        /// </summary>
        public string DocumentNumber { get; set; }
    }
}
