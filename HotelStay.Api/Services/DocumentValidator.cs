using System;
using System.Collections.Generic;
using HotelStay.Api.Models;

namespace HotelStay.Api.Services
{
    /// <summary>
    /// Validates document requirements per destination.
    /// International destinations require Passport; domestic accept Passport or NationalId.
    /// </summary>
    public class DocumentValidator : IDocumentValidator
    {
        private static readonly HashSet<string> International = new(StringComparer.OrdinalIgnoreCase)
        {
            "London",
            "Paris",
            "Tokyo"
        };

        private static readonly HashSet<string> Domestic = new(StringComparer.OrdinalIgnoreCase)
        {
            "New Delhi",
            "Hyderabad"
        };


        /// <inheritdoc />
        public (bool IsValid, string ErrorMessage) ValidateDocument(string destination, DocumentType documentType)
        {
            if (string.IsNullOrWhiteSpace(destination))
            {
                return (false, "Destination is required for document validation.");
            }

            if (International.Contains(destination))
            {
                if (documentType == DocumentType.Passport)
                {
                    return (true, string.Empty);
                }

                // Include the phrase 'Passport required' to match test expectations
                return (false, $"{destination} is an international destination. Passport required. {documentType} is not accepted.");
            }

            if (Domestic.Contains(destination))
            {
                // Both Passport and NationalId accepted
                return (true, string.Empty);
            }

            return (false, $"Destination '{destination}' not recognized. Supported cities: New Delhi, Hyderabad, London, Paris, Tokyo");
        }
    }
}
