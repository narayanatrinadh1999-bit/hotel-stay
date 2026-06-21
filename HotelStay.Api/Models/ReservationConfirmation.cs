using System;

namespace HotelStay.Api.Models
{
    /// <summary>
    /// Response model returned after a successful reservation.
    /// Contains reservation metadata and a masked document number for privacy.
    /// </summary>
    public class ReservationConfirmation
    {
        /// <summary>
        /// Unique reservation reference (format: RES-YYYYMMDD-XXXXXX).
        /// </summary>
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Timestamp when the reservation was created (UTC).
        /// </summary>
        public DateTime ReservedAt { get; set; }

        /// <summary>
        /// Human-friendly hotel name.
        /// </summary>
        public string HotelName { get; set; }

        /// <summary>
        /// Provider that fulfilled the reservation.
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Room type reserved.
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
        /// Number of nights for the reservation.
        /// </summary>
        public int NightsCount { get; set; }

        /// <summary>
        /// Price per night charged.
        /// </summary>
        public decimal PricePerNight { get; set; }

        /// <summary>
        /// Total price for the reservation.
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Cancellation policy that applies to this reservation.
        /// </summary>
        public CancellationPolicy CancellationPolicy { get; set; }

        /// <summary>
        /// Guest full name used for the reservation.
        /// </summary>
        public string GuestName { get; set; }

        /// <summary>
        /// Document type provided by the guest.
        /// </summary>
        public DocumentType DocumentType { get; set; }

        /// <summary>
        /// Masked document number for privacy (e.g. "P1234****").
        /// </summary>
        public string DocumentNumber { get; set; }
    }
}
