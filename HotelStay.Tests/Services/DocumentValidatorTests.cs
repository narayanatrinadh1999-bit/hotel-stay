using System;
using Xunit;
using HotelStay.Api.Services;
using HotelStay.Api.Models;

namespace HotelStay.Tests.Services
{
    public class DocumentValidatorTests
    {
        private readonly DocumentValidator _validator = new DocumentValidator();

        [Fact]
        public void ValidateDocument_InternationalDestination_Passport_ReturnsValid()
        {
            var (isValid, error) = _validator.ValidateDocument("London", DocumentType.Passport);
            Assert.True(isValid);
            Assert.True(string.IsNullOrEmpty(error));
        }

        [Fact]
        public void ValidateDocument_InternationalDestination_NationalId_ReturnsInvalid()
        {
            var (isValid, error) = _validator.ValidateDocument("London", DocumentType.NationalId);
            Assert.False(isValid);
            Assert.Contains("Passport required", error, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ValidateDocument_DomesticDestination_NationalId_ReturnsValid()
        {
            var (isValid, error) = _validator.ValidateDocument("New Delhi", DocumentType.NationalId);
            Assert.True(isValid);
            Assert.True(string.IsNullOrEmpty(error));
        }

        [Fact]
        public void ValidateDocument_DomesticDestination_Passport_ReturnsValid()
        {
            var (isValid, error) = _validator.ValidateDocument("New Delhi", DocumentType.Passport);
            Assert.True(isValid);
            Assert.True(string.IsNullOrEmpty(error));
        }

        [Theory]
        [InlineData("London")]
        [InlineData("Paris")]
        [InlineData("Tokyo")]
        public void ValidateDocument_AllInternationalDestinations_Passport_ReturnsValid(string destination)
        {
            var (isValid, error) = _validator.ValidateDocument(destination, DocumentType.Passport);
            Assert.True(isValid);
            Assert.True(string.IsNullOrEmpty(error));
        }

        [Theory]
        [InlineData("New Delhi")]
        [InlineData("Hyderabad")]
        public void ValidateDocument_AllDomesticDestinations_NationalId_ReturnsValid(string destination)
        {
            var (isValid, error) = _validator.ValidateDocument(destination, DocumentType.NationalId);
            Assert.True(isValid);
            Assert.True(string.IsNullOrEmpty(error));
        }

        [Fact]
        public void ValidateDocument_ErrorMessage_IsClear()
        {
            var (isValid, error) = _validator.ValidateDocument("Paris", DocumentType.NationalId);
            Assert.False(isValid);
            Assert.Contains("Paris", error, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Passport", error, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("london")]
        [InlineData("LONDON")]
        [InlineData("LoNdOn")]
        public void ValidateDocument_CaseInsensitive_Destination(string destination)
        {
            var (isValid, error) = _validator.ValidateDocument(destination, DocumentType.Passport);
            Assert.True(isValid);
            Assert.True(string.IsNullOrEmpty(error));
        }
    }
}
