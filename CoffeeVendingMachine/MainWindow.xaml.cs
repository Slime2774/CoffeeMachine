using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CoffeeVendingMachine.Models;
using CoffeeVendingMachine.Services;

namespace CoffeeVendingMachine
{
    public partial class MainWindow : Window
    {
        private Config _config;
        private bool _isBrewing = false;

        public MainWindow()
        {
            InitializeComponent();
            var menu = CoffeeLoader.LoadMenu();
            _config = CoffeeLoader.LoadConfig();
            DataContext = new { CoffeeList = menu };
        }

        private async void CoffeeCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (_isBrewing) return;

            if (sender is Border border && border.Tag is Drink selectedDrink)
            {
                // 1. Окно настройки (там есть своя кнопка "Отмена")
                var customWin = new CustomizationWindow(selectedDrink, _config) { Owner = this };
                if (customWin.ShowDialog() == true)
                {
                    decimal finalPrice = customWin.FinalPrice;

                    // 2. Окно оплаты (там тоже есть кнопка "Отмена")
                    var payWin = new PaymentWindow(finalPrice) { Owner = this };
                    if (payWin.ShowDialog() == true && payWin.IsPaymentSuccessful)
                    {
                        // 3. Готовим кофе (если дошли сюда, значит оплата прошла)
                        await BrewCoffeeAsync(selectedDrink, finalPrice, payWin.Change);
                    }
                }
            }
        }

        private async Task BrewCoffeeAsync(Drink drink, decimal price, decimal change)
        {
            _isBrewing = true;

            ChangeMoneyText.Text = $"{change} ₽"; // Показываем сдачу
            StatusText.Text = "Готовим ваш напиток...";

            // --- АНИМАЦИЯ ---
            EmptyCupPlaceholder.Visibility = Visibility.Collapsed;
            CoffeeCup.Opacity = 1;
            CoffeeLiquid.Height = 0;

            BrewProgress.Visibility = Visibility.Visible;
            BrewProgress.Value = 0;
            TakeCupText.Opacity = 0;

            for (int i = 0; i <= 30; i++)
            {
                BrewProgress.Value = i * (100.0 / 30.0);
                CoffeeLiquid.Height = (i / 30.0) * 80.0;
                await Task.Delay(100);
            }

            // --- КОНЕЦ АНИМАЦИИ ---

            TakeCupText.Opacity = 1;
            StatusText.Text = "Заберите напиток и сдачу";
            _isBrewing = false;

            // Ждем 5 секунд и сбрасываем
            await Task.Delay(5000);

            CoffeeCup.Opacity = 0;
            EmptyCupPlaceholder.Visibility = Visibility.Visible;
            TakeCupText.Opacity = 0;
            ChangeMoneyText.Text = "0 ₽";
            StatusText.Text = "Выберите напиток";
        }
    }
}