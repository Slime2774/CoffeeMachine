using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CoffeeVendingMachine.Models
{

    // Абстрактный класс. Могу создать только конкретный напиток
    public abstract class Drink
    {
        [JsonPropertyName("name")] // Берём "name" из JSON файла и получаем его здесь
        public string Name { get; set; } = string.Empty; // string.Empty защита, если имя не указано

        [JsonPropertyName("basePrice")]
        public decimal BasePrice { get; set; } // decimal - Нужен для избежания проблем с округлением денег(точность 25-29 десятичных знаков)

        [JsonPropertyName("icon")]
        public string Icon { get; set; } = "☕"; // Если в JSON нет иконки, "☕" - будет использоваться по умолчанию

        [JsonIgnore]
        public virtual string PriceText => $"от {BasePrice:0} ₽"; // Форматирует число: 150 в 150 ₽
    }
}
