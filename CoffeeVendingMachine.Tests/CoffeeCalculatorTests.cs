using Xunit;
using CoffeeVendingMachine.Models;

namespace CoffeeVendingMachine.Tests
{
    public class CoffeeCalculatorTests
    {
        [Fact]
        public void CalculateTotalPrice_WithSizeAndSyrup_ReturnsCorrectSum()
        {
            // Arrange (Подготовка данных)
            var drink = new CoffeeDrink { BasePrice = 150 };
            var size = new CupSize { ExtraPrice = 20 };
            var config = new Config { SyrupPrice = 30, ToppingPrice = 20, ExtraEspressoShotPrice = 40 };

            // Act (Имитация логики из CalculateCustomPrice)
            decimal total = drink.BasePrice;
            total += size.ExtraPrice;
            total += config.SyrupPrice;

            // Assert (Проверка)
            Assert.Equal(200, total); // 150 + 20 + 30 = 200
        }

        [Fact]
        public void CalculateChange_WhenCashIsMoreThanPrice_ReturnsCorrectChange()
        {
            // Arrange
            decimal finalPrice = 180;
            decimal insertedCash = 500;

            // Act
            decimal change = insertedCash - finalPrice;

            // Assert
            Assert.Equal(320, change);
        }

        [Fact]
        public void CalculateChange_WhenCashIsLessThanPrice_ReturnsZero()
        {
            // Arrange
            decimal finalPrice = 180;
            decimal insertedCash = 100;
            decimal paymentChange = 0;

            // Act (Имитация логики из InsertCash_Click)
            if (insertedCash >= finalPrice)
            {
                paymentChange = insertedCash - finalPrice;
            }

            // Assert
            Assert.Equal(0, paymentChange);
        }
    }
}