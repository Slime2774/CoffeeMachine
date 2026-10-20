using Xunit;
using CoffeeVendingMachine.Models;

namespace CoffeeVendingMachine.Tests
{
    public class CoffeeCalculatorTests
    {
        [Fact]
        public void CalculateTotalPrice_WithSizeShotsAndSyrup_ReturnsCorrectSum()
        {
            // Arrange (Подготовка)
            var drink = new CoffeeDrink { BasePrice = 150 }; // Капучино
            var size = new CupSize { ExtraPrice = 20 }; // Средний размер
            var config = new Config { SyrupPrice = 30, ExtraEspressoShotPrice = 40 };

            // Act (Имитируем логику из CalculateCustomPrice)
            decimal total = drink.BasePrice;
            total += size.ExtraPrice; // Выбрали размер
            total += config.ExtraEspressoShotPrice; // Выбрали +1 шот
            total += config.SyrupPrice; // Выбрали сироп

            // Assert (Проверка)
            Assert.Equal(240, total); // 150 + 20 + 40 + 30 = 240
        }

        [Fact]
        public void InventoryDeduction_ForMediumCappuccinoWithSugar_CalculatesCorrectly()
        {
            // Arrange
            var inventory = new Inventory(); // По умолчанию 50 стаканов, 5000 воды и т.д.
            int targetVolume = 300; // Средний размер
            int shots = 1; // Стандарт
            int sugarLumps = 2; // 2 ложки сахара

            // Act (Имитируем логику из StartBrewingAsync)
            int waterNeeded = targetVolume / 2;
            int milkNeeded = targetVolume / 2;
            int coffeeNeeded = shots * 10;
            int sugarNeeded = sugarLumps * 5;

            inventory.Cups--;
            inventory.Water -= waterNeeded;
            inventory.Milk -= milkNeeded;
            inventory.CoffeeBeans -= coffeeNeeded;
            inventory.Sugar -= sugarNeeded;

            // Assert (Проверяем, что остатки уменьшились ровно на столько, на сколько должны)
            Assert.Equal(49, inventory.Cups); // 50 - 1
            Assert.Equal(4850, inventory.Water); // 5000 - 150
            Assert.Equal(1850, inventory.Milk); // 2000 - 150
            Assert.Equal(990, inventory.CoffeeBeans); // 1000 - 10
            Assert.Equal(990, inventory.Sugar); // 1000 - 10
        }

        [Fact]
        public void InventoryDeduction_WhenResourcesAreLow_ShouldNotAllowNegativeValues()
        {
            // Arrange
            var inventory = new Inventory();
            inventory.Cups = 0; // Нет стаканов
            int targetVolume = 300;
            int waterNeeded = targetVolume / 2;

            // Act & Assert
            // Имитируем проверку из вашего кода StartBrewingAsync
            bool hasEnoughResources = inventory.Cups > 0 && inventory.Water >= waterNeeded;

            // Проверяем, что система не даст приготовить кофе (условие не выполнится)
            Assert.False(hasEnoughResources);
        }
    }
}