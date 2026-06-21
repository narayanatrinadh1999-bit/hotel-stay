using HotelStay.Api.Models;

namespace HotelStay.Api.Services;

public interface IDocumentValidator
{
    (bool IsValid, string ErrorMessage) ValidateDocument(
        string destination,
        DocumentType documentType
    );
}
