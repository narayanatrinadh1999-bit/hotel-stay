using System;

namespace HotelStay.Api.Models
{
    /// <summary>
    /// Request model for searching available hotel rooms.
    /// </summary>
    public class HotelSearchRequest
    {
        /// <summary>
        /// Destination city name (case-insensitive). Required.
        /// Examples: "London", "New York".
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// Check-in date (YYYY-MM-DD). Required.
        /// </summary>
        public DateTime CheckInDate { get; set; }

        /// <summary>
        /// Check-out date (YYYY-MM-DD). Must be strictly later than CheckInDate. Required.
        /// </summary>
        public DateTime CheckOutDate { get; set; }

        /// <summary>
        /// Optional filter for room type (Standard, Deluxe, Suite).
        /// </summary>
        public RoomType? RoomType { get; set; }
    }
}
