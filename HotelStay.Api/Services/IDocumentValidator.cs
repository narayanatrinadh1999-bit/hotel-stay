using HotelStay.Api.Models;

namespace HotelStay.Api.Services
{
    /// <summary>
    /// Validates whether a provided document type is acceptable for a destination.
    /// </summary>
    public interface IDocumentValidator
    {
        /// <summary>
        /// Validate document type against destination rules.
        /// </summary>
        /// <param name="destination">Destination city name (case-insensitive).</param>
        /// <param name="documentType">Type of document provided.</param>
        /// <returns>Tuple of (IsValid, ErrorMessage). ErrorMessage is null or empty when valid.</returns>
        (bool IsValid, string ErrorMessage) ValidateDocument(string destination, DocumentType documentType);
    }
}
