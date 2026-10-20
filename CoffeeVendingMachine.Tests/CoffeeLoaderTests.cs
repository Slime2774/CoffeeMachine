using Xunit;
using CoffeeVendingMachine.Models;
using System.Text.Json;
using System.Collections.Generic;

namespace CoffeeVendingMachine.Tests
{
    public class CoffeeLoaderTests
    {
        [Fact]
        public void Deserialize_ConfigJson_ReturnsCorrectValues()
        {
            // Arrange (Кусочек вашего config.json)
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
        public void Deserialize_CoffeeMenuJson_ReturnsListOfDrinks()
        {
            // Arrange (Кусочек вашего menu.json)
            string json = @"[
                { ""type"": ""CoffeeDrink"", ""name"": ""Капучино"", ""basePrice"": 150, ""coffeeType"": ""Arabica"" },
                { ""type"": ""CoffeeDrink"", ""name"": ""Латте"", ""basePrice"": 160, ""coffeeType"": ""Arabica"" }
            ]";

            // Act
            var menu = JsonSerializer.Deserialize<List<CoffeeDrink>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert
            Assert.NotNull(menu);
            Assert.Equal(2, menu.Count);
            Assert.Equal("Капучино", menu[0].Name);
            Assert.Equal("Латте", menu[1].Name);
        }
    }
}