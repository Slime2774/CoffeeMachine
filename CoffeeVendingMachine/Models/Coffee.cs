using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization; //

namespace CoffeeVendingMachine.Models
{
    public class Coffee
    {
        [JsonPropertyName("name")] // Берём "name" из JSON файла и получаем его здесь
        public string Name { get; set; } = string.Empty; // string.Empty защита, если имя не указано

        [JsonPropertyName("price")] 
        public decimal Priece { get; set; } // decimal - Нужен для избежания проблем с округлением денег(точность 25-29 десятичных знаков)

        [JsonPropertyName("icon")]
        public string Icon { get; set; } = "☕"; // Если в JSON нет иконки, "☕" - будет использоваться по умолчанию

        [JsonIgnore]
        public string PriceText => $"{Priece:0} ₽"; // Форматирует число: 150 в 150 ₽

    }
}
