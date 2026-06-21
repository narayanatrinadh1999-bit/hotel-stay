using System;

namespace HotelStay.Api.Models
{
    /// <summary>
    /// Internal storage model for a reservation saved by the application.
    /// This is used by the in-memory reservation store and service layer.
    /// </summary>
    public class Reservation
    {
        /// <summary>
        /// Unique reservation reference (format: RES-YYYYMMDD-XXXXXX).
        /// </summary>
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// The original reservation request provided by the client.
        /// </summary>
        public ReservationRequest Request { get; set; }

        /// <summary>
        /// UTC timestamp when the reservation was created.
        /// </summary>
        public DateTime ReservedAt { get; set; }

        /// <summary>
        /// Total price charged for the reservation (PricePerNight * NightsCount).
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Cancellation policy that applies to this reservation.
        /// </summary>
        public CancellationPolicy CancellationPolicy { get; set; }
    }
}
