using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls; // Для Border
using System.Windows.Input;
using CoffeeVendingMachine.Models;
using CoffeeVendingMachine.Services;

namespace CoffeeVendingMachine
{
    public partial class MainWindow : Window
    {
        private Config _config;
        private decimal _currentBalance = 0;
        private bool _isBrewing = false;

        public MainWindow()
        {
            InitializeComponent();
            var menu = CoffeeLoader.LoadMenu();
            _config = CoffeeLoader.LoadConfig();
            DataContext = new { CoffeeList = menu };
        }

        private void InsertMoney_Click(object sender, RoutedEventArgs e)
        {
            if (_isBrewing) return;
            if (sender is FrameworkElement btn && decimal.TryParse(btn.Tag?.ToString(), out decimal amount))
            {
                _currentBalance += amount;
                InsertedMoneyText.Text = $"{_currentBalance} ₽";
                StatusText.Text = "Выберите напиток";
            }
        }

        private async void CoffeeCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (_isBrewing) return;
            if (sender is Border border && border.Tag is Drink selectedDrink)
            {
                if (_currentBalance < selectedDrink.BasePrice)
                {
                    MessageBox.Show($"Недостаточно средств! Внесите еще {selectedDrink.BasePrice - _currentBalance} ₽", "Ошибка оплаты", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var win = new CustomizationWindow(selectedDrink, _config) { Owner = this };
                if (win.ShowDialog() == true)
                {
                    decimal finalPrice = win.FinalPrice;
                    if (_currentBalance < finalPrice)
                    {
                        MessageBox.Show($"Не хватает {finalPrice - _currentBalance} ₽ на выбранные добавки!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    await BrewCoffeeAsync(selectedDrink, finalPrice);
                }
            }
        }

        private async Task BrewCoffeeAsync(Drink drink, decimal price)
        {
            _isBrewing = true;
            decimal change = _currentBalance - price;
            _currentBalance = 0;

            InsertedMoneyText.Text = "0 ₽";
            ChangeMoneyText.Text = $"{change} ₽";

            BrewingOverlay.Visibility = Visibility.Visible;
            BrewingText.Text = $"Готовим {drink.Name}...";
            BrewProgress.Value = 0;

            for (int i = 0; i <= 30; i++)
            {
                BrewProgress.Value = i * (100.0 / 30.0);
                await Task.Delay(100);
            }

            BrewingOverlay.Visibility = Visibility.Collapsed;
            MessageBox.Show($"Ваш {drink.Name} готов!\nЗаберите сдачу: {change} ₽", "Готово");

            await Task.Delay(5000);
            ChangeMoneyText.Text = "0 ₽";
            StatusText.Text = "Выберите напиток";
            _isBrewing = false;
        }
    }
}