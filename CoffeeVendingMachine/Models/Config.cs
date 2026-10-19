using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


// Класс чтения настроек из config.json
namespace CoffeeVendingMachine.Models
{
    public class CupSize
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("volume")]
        public int Volume { get; set; }

        [JsonPropertyName("extraPrice")]
        public decimal ExtraPrice { get; set; }
    }

    public class Config
    {
        //Цена одного сиропа
        [JsonPropertyName("syrupPrice")]
        public decimal SyrupPrice { get; set; }

        //Цена одного топпинга
        [JsonPropertyName("toppingPrice")]
        public decimal ToppingPrice { get; set; }

        //Цена доп. шота эспрессо
        [JsonPropertyName("extraEspressoShotPrice")]
        public decimal ExtraEspressoShotPrice { get; set; }

        //Список доступных объёмов
        [JsonPropertyName("cupSizes")]
        public List<CupSize> CupSizes { get; set; } = new();
    }

}
