using System;
using System.Collections.Generic;

namespace HotelStay.Api.Models
{
    /// <summary>
    /// Response model for hotel search operations. Contains normalized results from
    /// multiple providers and request metadata.
    /// </summary>
    public class HotelSearchResponse
    {
        /// <summary>
        /// List of normalized hotel rooms matching the search criteria.
        /// </summary>
        public List<HotelRoom> Results { get; set; }

        /// <summary>
        /// Total number of results returned.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Destination used for the search.
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// Check-in date from the original request.
        /// </summary>
        public DateTime CheckInDate { get; set; }

        /// <summary>
        /// Check-out date from the original request.
        /// </summary>
        public DateTime CheckOutDate { get; set; }

        /// <summary>
        /// Number of nights calculated as (CheckOutDate - CheckInDate).Days.
        /// </summary>
        public int NightsCount { get; set; }

        /// <summary>
        /// Optional client-side sorting indicator, e.g. "TotalPrice_Asc".
        /// </summary>
        public string SortedBy { get; set; }
    }
}
