using System;

#nullable enable
namespace HotelStay.Api.Models
{
    /// <summary>
    /// Unified hotel room model returned by search and included in reservation confirmations.
    /// This class normalizes provider-specific responses into a single representation used
    /// throughout the API and services.
    /// </summary>
    public class HotelRoom
    {
        /// <summary>
        /// Unique identifier for the hotel provided by the upstream provider (e.g. "premier_001").
        /// </summary>
        public string HotelId { get; set; }

        /// <summary>
        /// Human-friendly hotel name (normalized from provider data).
        /// </summary>
        public string HotelName { get; set; }

        /// <summary>
        /// Provider that supplied this room ("PremierStays" or "BudgetNests").
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Room type (Standard, Deluxe, Suite).
        /// </summary>
        public RoomType RoomType { get; set; }

        /// <summary>
        /// Price per night for a single night stay.
        /// </summary>
        public decimal PricePerNight { get; set; }

        /// <summary>
        /// Number of nights between check-in and check-out used to calculate totals.
        /// </summary>
        public int TotalNights { get; set; }

        /// <summary>
        /// Calculated total price for the stay: PricePerNight * TotalNights.
        /// </summary>
        public decimal TotalPrice => PricePerNight * TotalNights;

        /// <summary>
        /// Unified cancellation policy for this room.
        /// </summary>
        public CancellationPolicy CancellationPolicy { get; set; }

        /// <summary>
        /// Optional amenities (available for PremierStays; null for BudgetNests).
        /// Example: ["WiFi", "Pool", "Gym"].
        /// </summary>
        public string[] Amenities { get; set; }

        /// <summary>
        /// Star rating (e.g., 5). May be null for providers that don't supply this information.
        /// </summary>
        public int? StarRating { get; set; }

        /// <summary>
        /// Availability flag as returned by providers. When false the caller should filter the result out.
        /// Providers may omit this value (null) to indicate availability is unknown or always available.
        /// </summary>
        public bool? Available { get; set; }

        /// <summary>
        /// When this room was searched (UTC). Useful for caching, diagnostics and UI display.
        /// </summary>
        public DateTime SearchDate { get; set; }
    }
}
