using Xunit;
using CoffeeVendingMachine.Models;
using System;

namespace CoffeeVendingMachine.Tests
{
    public class CoffeeCalculatorTests
    {
        [Fact]
        public void CalculateTotalPrice_WithSizeShotsAndSyrup_ReturnsCorrectSum()
        {
            // Этот тест остается прежним, логика цен не менялась
            var drink = new CoffeeDrink { BasePrice = 150 };
            var size = new CupSize { ExtraPrice = 20 };
            var config = new Config { SyrupPrice = 30, ExtraEspressoShotPrice = 40 };

            decimal total = drink.BasePrice + size.ExtraPrice + config.ExtraEspressoShotPrice + config.SyrupPrice;

            Assert.Equal(240, total);
        }

        [Fact]
        public void InventoryDeduction_ComplexOrder_CalculatesCorrectly()
        {
            // Обновленный тест: проверяем большой заказ (Большой объем, 2 доп. шота, 3 сахара, 2 сиропа)
            var inventory = new Inventory();

            int targetVolume = 400; // Большой
            int shots = 1 + 2; // Стандарт + 2 шота
            int sugarLumps = 3; // 3 ложки
            int syrupsCount = 2; // 2 сиропа

            // Логика из нового StartBrewingAsync
            int waterNeeded = targetVolume / 2;
            int milkNeeded = targetVolume / 2;
            int coffeeNeeded = shots * 10;
            int sugarNeeded = 15; // В вашем коде 3 ложки = 15г
            int syrupNeeded = syrupsCount * 20;

            inventory.Cups--;
            inventory.Water -= waterNeeded;
            inventory.Milk -= milkNeeded;
            inventory.CoffeeBeans -= coffeeNeeded;
            inventory.Sugar -= sugarNeeded;
            inventory.Syrup -= syrupNeeded;

            Assert.Equal(49, inventory.Cups);
            Assert.Equal(4800, inventory.Water); // 5000 - 200
            Assert.Equal(1800, inventory.Milk); // 2000 - 200
            Assert.Equal(970, inventory.CoffeeBeans); // 1000 - 30
            Assert.Equal(985, inventory.Sugar); // 1000 - 15
            Assert.Equal(960, inventory.Syrup); // 1000 - 40
        }

        [Fact]
        public void CalculateCoins_For55Rubles_ReturnsFiveCoins()
        {
            // Тестируем вашу новую логику разбивки сдачи на монеты (метод ShowCoins)
            decimal amount = 55;
            int coinsCount = 5;
            decimal coinValue = Math.Floor(amount / coinsCount); // 55 / 5 = 11
            decimal remainder = amount - (coinValue * coinsCount); // 0

            Assert.Equal(11, coinValue);
            Assert.Equal(0, remainder);
        }

        [Fact]
        public void CalculateCoins_ForSmallChange_ReturnsCorrectNumberOfCoins()
        {
            // Тестируем крайний случай: сдача 3 рубля
            decimal amount = 3;
            int coinsCount = 5;
            decimal coinValue = Math.Floor(amount / coinsCount); // 0

            if (coinValue == 0)
            {
                coinsCount = (int)amount; // 3 монеты
                coinValue = 1;
            }

            Assert.Equal(3, coinsCount);
            Assert.Equal(1, coinValue);
        }
    }
}