using Xunit;
using CoffeeVendingMachine.Models;
using System.Text.Json;

namespace CoffeeVendingMachine.Tests
{
    public class CoffeeLoaderTests
    {
        [Fact]
        public void Deserialize_ConfigJson_ReturnsCorrectValues()
        {
            // Arrange
            string json = @"{
                ""syrupPrice"": 30,
                ""toppingPrice"": 20,
                ""extraEspressoShotPrice"": 40,
                ""cupSizes"": [ { ""name"": ""Маленький"", ""volume"": 200, ""extraPrice"": 0 } ]
            }";

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Act
            var config = JsonSerializer.Deserialize<Config>(json, options);

            // Assert
            Assert.NotNull(config);
            Assert.Equal(30, config.SyrupPrice);
            Assert.Single(config.CupSizes);
            Assert.Equal("Маленький", config.CupSizes[0].Name);
        }

        [Fact]
        public void Deserialize_CoffeeDrinkJson_ReturnsCorrectType()
        {
            // Arrange
            string json = @"{
                ""type"": ""CoffeeDrink"",
                ""name"": ""Капучино"",
                ""basePrice"": 150,
                ""coffeeType"": ""Arabica""
            }";

            // Act
            var drink = JsonSerializer.Deserialize<CoffeeDrink>(json);

            // Assert
            Assert.NotNull(drink);
            Assert.Equal("Капучино", drink.Name);
            Assert.Equal(150, drink.BasePrice);
            Assert.Equal("Arabica", drink.CoffeeType);
        }
    }
}