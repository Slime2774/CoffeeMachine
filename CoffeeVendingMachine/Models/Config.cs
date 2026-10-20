using System.Collections.Generic;
using System.Text.Json.Serialization;

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
        // ИСПРАВЛЕНО: Добавлены значения по умолчанию, чтобы не было нулей, если файл не прочитан
        [JsonPropertyName("syrupPrice")]
        public decimal SyrupPrice { get; set; } = 30;

        [JsonPropertyName("toppingPrice")]
        public decimal ToppingPrice { get; set; } = 20;

        [JsonPropertyName("extraEspressoShotPrice")]
        public decimal ExtraEspressoShotPrice { get; set; } = 40;

        [JsonPropertyName("cupSizes")]
        public List<CupSize> CupSizes { get; set; } = new()
        {
            new CupSize { Name = "Маленький (200 мл)", Volume = 200, ExtraPrice = 0 },
            new CupSize { Name = "Средний (300 мл)", Volume = 300, ExtraPrice = 20 },
            new CupSize { Name = "Большой (400 мл)", Volume = 400, ExtraPrice = 40 }
        };
    }
}