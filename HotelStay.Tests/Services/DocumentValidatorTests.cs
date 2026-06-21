using Xunit;
using HotelStay.Api.Services;
using HotelStay.Api.Models;

namespace HotelStay.Tests.Services;

public class DocumentValidatorTests
{
    private readonly DocumentValidator _validator = new DocumentValidator();

    [Fact]
    public void ValidateDocument_InternationalDestination_Passport_ReturnsValid()
    {
        var (isValid, errorMessage) = _validator.ValidateDocument("London", DocumentType.Passport);

        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    [Fact]
    public void ValidateDocument_InternationalDestination_NationalId_ReturnsInvalid()
    {
        var (isValid, errorMessage) = _validator.ValidateDocument("London", DocumentType.NationalId);

        Assert.False(isValid);
        Assert.Contains("Passport is required", errorMessage);
    }

    [Fact]
    public void ValidateDocument_DomesticDestination_NationalId_ReturnsValid()
    {
        var (isValid, errorMessage) = _validator.ValidateDocument("New Delhi", DocumentType.NationalId);

        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    [Fact]
    public void ValidateDocument_DomesticDestination_Passport_ReturnsValid()
    {
        var (isValid, errorMessage) = _validator.ValidateDocument("New Delhi", DocumentType.Passport);

        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    [Theory]
    [InlineData("London")]
    [InlineData("Paris")]
    [InlineData("Tokyo")]
    public void ValidateDocument_AllInternationalDestinations_Passport_ReturnsValid(string destination)
    {
        var (isValid, _) = _validator.ValidateDocument(destination, DocumentType.Passport);

        Assert.True(isValid);
    }

    [Theory]
    [InlineData("New York")]
    [InlineData("Los Angeles")]
    [InlineData("New Delhi")]
    [InlineData("Hyderabad")]
    public void ValidateDocument_AllDomesticDestinations_NationalId_ReturnsValid(string destination)
    {
        var (isValid, _) = _validator.ValidateDocument(destination, DocumentType.NationalId);

        Assert.True(isValid);
    }

    [Fact]
    public void ValidateDocument_ErrorMessage_IsClear()
    {
        var (isValid, errorMessage) = _validator.ValidateDocument("Tokyo", DocumentType.NationalId);

        Assert.False(isValid);
        Assert.Contains("Tokyo", errorMessage);
        Assert.Contains("Passport is required", errorMessage);
    }

    [Theory]
    [InlineData("london")]
    [InlineData("LONDON")]
    [InlineData("LoNdOn")]
    public void ValidateDocument_CaseInsensitive_Destination(string destination)
    {
        var (isValid, errorMessage) = _validator.ValidateDocument(destination, DocumentType.NationalId);

        Assert.False(isValid);
        Assert.NotNull(errorMessage);
    }
}
