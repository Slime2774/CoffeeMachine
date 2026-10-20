using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.ComponentModel; // ИЗМЕНЕНО: Добавил для работы интерфейса INotifyPropertyChanged
using System.Runtime.CompilerServices; // ИЗМЕНЕНО: Нужно для CallerMemberName

namespace CoffeeVendingMachine.Models
{
    // Абстрактный класс. Могу создать только конкретный напиток
    // ИЗМЕНЕНО: Добавил наследование от INotifyPropertyChanged, чтобы DataGrid в Админке видел изменения
    public abstract class Drink : INotifyPropertyChanged
    {
        // ИЗМЕНЕНО: Создал скрытые поля (переменные) для свойств, чтобы можно было отслеживать их изменение
        private string _name = string.Empty;
        private decimal _basePrice;
        private string _icon = "☕";

        [JsonPropertyName("name")] // Берём "name" из JSON файла и получаем его здесь
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); } // ИЗМЕНЕНО: Добавил вызов OnPropertyChanged()
        }

        [JsonPropertyName("basePrice")]
        public decimal BasePrice
        {
            get => _basePrice;
            set { _basePrice = value; OnPropertyChanged(); } // ИЗМЕНЕНО: Добавил вызов OnPropertyChanged()
        }

        [JsonPropertyName("icon")]
        public string Icon
        {
            get => _icon;
            set { _icon = value; OnPropertyChanged(); } // ИЗМЕНЕНО: Добавил вызов OnPropertyChanged()
        }

        [JsonIgnore]
        public virtual string PriceText => $"от {BasePrice:0} ₽"; // Форматирует число: 150 в 150 ₽

        // ИЗМЕНЕНО: Всё, что ниже, добавлено мной для корректной работы таблицы (DataGrid)
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}