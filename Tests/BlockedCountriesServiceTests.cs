using System;
using System.Linq;
using Xunit;
using assignment.Services;
using assignment.Models;

namespace assignment.Tests
{
    public class BlockedCountriesServiceTests
    {
        private readonly BlockedCountriesService _service;

        public BlockedCountriesServiceTests()
        {
            _service = new BlockedCountriesService();
        }

        [Fact]
        public void BlockCountry_ValidCountry_ShouldBlock()
        {
            // Arrange
            var countryCode = "US";
            var countryName = "United States";

            // Act
            var result = _service.BlockCountry(countryCode, countryName);

            // Assert
            Assert.True(result);
            Assert.True(_service.IsCountryBlocked(countryCode));
        }

        [Fact]
        public void BlockCountry_InvalidCountry_ShouldNotBlock()
        {
            // Arrange
            var countryCode = "XX";
            var countryName = "Invalid Country";

            // Act
            var result = _service.BlockCountry(countryCode, countryName);

            // Assert
            Assert.False(result);
            Assert.False(_service.IsCountryBlocked(countryCode));
        }

        [Fact]
        public void UnblockCountry_BlockedCountry_ShouldUnblock()
        {
            // Arrange
            var countryCode = "US";
            _service.BlockCountry(countryCode, "United States");

            // Act
            var result = _service.UnblockCountry(countryCode);

            // Assert
            Assert.True(result);
            Assert.False(_service.IsCountryBlocked(countryCode));
        }

        [Fact]
        public void AddTemporalBlock_ValidInput_ShouldBlock()
        {
            // Arrange
            var countryCode = "US";
            var durationMinutes = 60;

            // Act
            var result = _service.AddTemporalBlock(countryCode, durationMinutes);

            // Assert
            Assert.True(result);
            Assert.True(_service.IsTemporarilyBlocked(countryCode));
        }

        [Fact]
        public void AddTemporalBlock_InvalidDuration_ShouldNotBlock()
        {
            // Arrange
            var countryCode = "US";
            var durationMinutes = 1500; // > 1440

            // Act
            var result = _service.AddTemporalBlock(countryCode, durationMinutes);

            // Assert
            Assert.False(result);
            Assert.False(_service.IsTemporarilyBlocked(countryCode));
        }

        [Fact]
        public void GetBlockedCountries_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            _service.BlockCountry("US", "United States");
            _service.BlockCountry("GB", "United Kingdom");
            _service.BlockCountry("CA", "Canada");

            // Act
            var (countries, totalCount) = _service.GetBlockedCountries(page: 1, pageSize: 2);

            // Assert
            Assert.Equal(2, countries.Count());
            Assert.Equal(3, totalCount);
        }

        [Fact]
        public void GetBlockedCountries_WithSearch_ShouldFilterResults()
        {
            // Arrange
            _service.BlockCountry("US", "United States");
            _service.BlockCountry("GB", "United Kingdom");
            _service.BlockCountry("CA", "Canada");

            // Act
            var (countries, totalCount) = _service.GetBlockedCountries(searchTerm: "United");

            // Assert
            Assert.Equal(2, countries.Count());
            Assert.Equal(2, totalCount);
        }
    }
} 