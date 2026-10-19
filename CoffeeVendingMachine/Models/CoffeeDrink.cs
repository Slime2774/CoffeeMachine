using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization; 

namespace CoffeeVendingMachine.Models
{
    public class CoffeeDrink : Drink
    {
        [JsonPropertyName("coffeeType")]
        public string CoffeeType { get; set; } = "Arabica"; // Нет в базовом классе, Добавил сорт зерна

        [JsonIgnore]
        public override string PriceText => $"{BasePrice:0} ₽";
    }
}
