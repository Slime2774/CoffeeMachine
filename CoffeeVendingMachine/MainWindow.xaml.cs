using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CoffeeVendingMachine.Models;
using CoffeeVendingMachine.Services;

namespace CoffeeVendingMachine
{
    public partial class MainWindow : Window
    {
        private Config _config;

        // Текущий баланс (внесенные деньги)
        private decimal _currentBalance = 0;

        // Флаг, чтобы не давать нажимать кнопки во время готовки
        private bool _isBrewing = false;

        public MainWindow()
        {
            InitializeComponent();

            // Загружаем данные из JSON
            var menu = CoffeeLoader.LoadMenu();
            _config = CoffeeLoader.LoadConfig();

            // Отображаем карточки на экране
            DataContext = new { CoffeeList = menu };
        }

        // 1. Обработчик кнопок +50, +100, +500
        private void InsertMoney_Click(object sender, RoutedEventArgs e)
        {
            if (_isBrewing) return;

            // Получаем сумму из Tag кнопки (50, 100 или 500)
            if (sender is FrameworkElement btn && decimal.TryParse(btn.Tag?.ToString(), out decimal amount))
            {
                _currentBalance += amount;

                // Обновляем текст на экране
                InsertedMoneyText.Text = $"{_currentBalance} ₽";
                StatusText.Text = "Выберите напиток";
            }
        }

        // 2. Клик по карточке кофе
        private async void CoffeeCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (_isBrewing) return;

            if (sender is Border border && border.Tag is Drink selectedDrink)
            {
                // Проверка: хватает ли денег хотя бы на базовый напиток?
                if (_currentBalance < selectedDrink.BasePrice)
                {
                    MessageBox.Show($"Недостаточно средств! Внесите еще {selectedDrink.BasePrice - _currentBalance} ₽",
                                    "Ошибка оплаты", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Открываем окно настройки (передаем напиток и конфиг)
                var win = new CustomizationWindow(selectedDrink, _config) { Owner = this };

                if (win.ShowDialog() == true)
                {
                    decimal finalPrice = win.FinalPrice;

                    // Проверка: хватает ли денег после добавок (сиропы, объем)?
                    if (_currentBalance < finalPrice)
                    {
                        MessageBox.Show($"Не хватает {finalPrice - _currentBalance} ₽ на выбранные добавки!",
                                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Запускаем готовку
                    await BrewCoffeeAsync(selectedDrink, finalPrice);
                }
            }
        }

        // 3. Логика приготовления и выдачи сдачи
        private async Task BrewCoffeeAsync(Drink drink, decimal price)
        {
            _isBrewing = true;

            // Считаем сдачу
            decimal change = _currentBalance - price;
            _currentBalance = 0;

            // Обновляем экран
            InsertedMoneyText.Text = "0 ₽";
            ChangeMoneyText.Text = $"{change} ₽";

            // Включаем анимацию
            BrewingOverlay.Visibility = Visibility.Visible;
            BrewingText.Text = $"Готовим {drink.Name}...";
            BrewProgress.Value = 0;

            // Имитация 3 секунд
            for (int i = 0; i <= 30; i++)
            {
                BrewProgress.Value = i * (100.0 / 30.0);
                await Task.Delay(100);
            }

            // Выключаем анимацию
            BrewingOverlay.Visibility = Visibility.Collapsed;

            MessageBox.Show($"Ваш {drink.Name} готов!\nЗаберите сдачу: {change} ₽", "Готово");

            // Ждем 5 секунд, чтобы пользователь "забрал" сдачу
            await Task.Delay(5000);
            ChangeMoneyText.Text = "0 ₽";
            StatusText.Text = "Выберите напиток";

            _isBrewing = false;
        }
    }
}