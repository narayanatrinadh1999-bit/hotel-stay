using HotelStay.Api.Models;

namespace HotelStay.Api.Services;

public class DocumentValidator : IDocumentValidator
{
    private static readonly HashSet<string> InternationalDestinations = new(StringComparer.OrdinalIgnoreCase)
    {
        "London", "Paris", "Tokyo"
    };

    private static readonly HashSet<string> DomesticDestinations = new(StringComparer.OrdinalIgnoreCase)
    {
        "New York", "Los Angeles", "New Delhi", "Hyderabad"
    };

    public (bool IsValid, string ErrorMessage) ValidateDocument(
        string destination,
        DocumentType documentType)
    {
        if (InternationalDestinations.Contains(destination))
        {
            if (documentType != DocumentType.Passport)
            {
                return (false, $"{destination} is an international destination. Passport is required, NationalId is not accepted.");
            }
            return (true, null!);
        }

        if (DomesticDestinations.Contains(destination))
        {
            return (true, null!);
        }

        return (true, null!);
    }
}
