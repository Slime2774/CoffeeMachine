using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace CoffeeVendingMachine.Models
{
    // Абстрактный класс. Могу создать только конкретный напиток
    // ИЗМЕНЕНО: Добавил наследование от INotifyPropertyChanged, чтобы DataGrid в Админке видел изменения
    public abstract class Drink : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private decimal _basePrice;
        private string _icon = "☕";

        [JsonPropertyName("name")] // Берём "name" из JSON файла и получаем его здесь
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        [JsonPropertyName("basePrice")]
        public decimal BasePrice
        {
            get => _basePrice;
            set
            {
                _basePrice = value;
                OnPropertyChanged();
                // ИЗМЕНЕНО: Сообщаем интерфейсу, что вычисляемый текст цены тоже изменился
                OnPropertyChanged(nameof(PriceText));
            }
        }

        [JsonPropertyName("icon")]
        public string Icon
        {
            get => _icon;
            set { _icon = value; OnPropertyChanged(); }
        }

        [JsonIgnore]
        public virtual string PriceText => $"от {BasePrice:0} ₽";

        // ИЗМЕНЕНО: Всё, что ниже, добавлено мной для корректной работы таблицы (DataGrid)
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}