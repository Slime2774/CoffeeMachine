using System.Text.Json.Serialization;

namespace CoffeeVendingMachine.Models
{
    public class CoffeeDrink : Drink
    {
        // ИЗМЕНЕНО: Сделал "полноценное" свойство для работы таблицы
        private string _coffeeType = "Arabica";

        [JsonPropertyName("coffeeType")]
        public string CoffeeType
        {
            get => _coffeeType;
            set { _coffeeType = value; OnPropertyChanged(); }
        }

        [JsonIgnore]
        public override string PriceText => $"{BasePrice:0} ₽";
    }
}