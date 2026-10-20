using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CoffeeVendingMachine.Models
{
    public class Inventory : INotifyPropertyChanged
    {
        private int _cups = 50;
        private int _coffeeBeans = 1000;
        private int _milk = 2000;
        private int _water = 5000;
        private int _sugar = 1000; // ИЗМЕНЕНО: Добавлен сахар
        private int _syrup = 1000; // ИЗМЕНЕНО: Добавлен сироп

        public int Cups { get => _cups; set { _cups = value; OnPropertyChanged(); } }
        public int CoffeeBeans { get => _coffeeBeans; set { _coffeeBeans = value; OnPropertyChanged(); } }
        public int Milk { get => _milk; set { _milk = value; OnPropertyChanged(); } }
        public int Water { get => _water; set { _water = value; OnPropertyChanged(); } }
        public int Sugar { get => _sugar; set { _sugar = value; OnPropertyChanged(); } } // ИЗМЕНЕНО
        public int Syrup { get => _syrup; set { _syrup = value; OnPropertyChanged(); } } // ИЗМЕНЕНО

        public int MaxCups => 50;
        public int MaxCoffee => 1000;
        public int MaxMilk => 2000;
        public int MaxWater => 5000;
        public int MaxSugar => 1000; // ИЗМЕНЕНО
        public int MaxSyrup => 1000; // ИЗМЕНЕНО

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}