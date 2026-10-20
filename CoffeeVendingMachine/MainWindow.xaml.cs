using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CoffeeVendingMachine.Models;
using CoffeeVendingMachine.Services;

namespace CoffeeVendingMachine
{
    public partial class MainWindow : Window
    {
        private Config _config;
        private bool _isBrewing = false;
        private bool _isCupReady = false;

        // Переменная для хранения текущей сдачи
        private decimal _currentChange = 0;

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

            // 1. Проверка на стакан (остается)
            if (_isCupReady)
            {
                StatusText.Text = "Сначала заберите готовый напиток!";
                StatusText.Foreground = Brushes.Red;
                await Task.Delay(2000);

                StatusText.Foreground = Brushes.SaddleBrown;
                StatusText.Text = _currentChange > 0 ? "Заберите сдачу!" : "Выберите напиток";
                return;
            }

            // 2. Проверка на сдачу (УБРАНА БЛОКИРОВКА)
            // Теперь мы просто показываем напоминание, но не возвращаем return
            if (_currentChange > 0)
            {
                StatusText.Text = "Не забудьте забрать сдачу!";
                StatusText.Foreground = Brushes.DarkOrange;
                await Task.Delay(2000); // Показываем 2 секунды и идем дальше
            }

            // Разрешаем делать заказ
            if (sender is Border border && border.Tag is Drink selectedDrink)
            {
                var customWin = new CustomizationWindow(selectedDrink, _config) { Owner = this };
                if (customWin.ShowDialog() == true)
                {
                    decimal finalPrice = customWin.FinalPrice;
                    var payWin = new PaymentWindow(finalPrice) { Owner = this };
                    if (payWin.ShowDialog() == true && payWin.IsPaymentSuccessful)
                    {
                        await BrewCoffeeAsync(selectedDrink, finalPrice, payWin.Change);
                    }
                }
            }
        }

        private async Task BrewCoffeeAsync(Drink drink, decimal price, decimal change)
        {
            _isBrewing = true;
            StatusText.Text = "Готовим ваш напиток...";

            // Анимация
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

            TakeCupText.Opacity = 1;
            BrewProgress.Visibility = Visibility.Collapsed;

            _isCupReady = true;

            // Если сдача была не забрана с прошлого раза, она просто прибавится/перезапишется
            _currentChange = change;
            if (_currentChange > 0)
            {
                ChangeMoneyText.Text = $"{_currentChange} ₽";
                StatusText.Text = "Заберите напиток и сдачу!";
            }
            else
            {
                // Если сдачи нет, но она была (например, закинули ровно нужную сумму), обнулим лоток
                ChangeMoneyText.Text = "0 ₽";
                StatusText.Text = "Заберите напиток!";
            }

            _isBrewing = false;
        }

        // Клик по стакану
        private void Cup_Click(object sender, MouseButtonEventArgs e)
        {
            if (!_isCupReady) return;

            CoffeeCup.Opacity = 0;
            TakeCupText.Opacity = 0;
            EmptyCupPlaceholder.Visibility = Visibility.Visible;
            _isCupReady = false;

            // Если сдача еще лежит, напоминаем
            if (_currentChange > 0)
            {
                // Запускаем таймер напоминания на 5 секунд
                ShowTemporaryStatus("Заберите сдачу!", 5000);
            }
            else
            {
                StatusText.Text = "Выберите напиток";
                StatusText.Foreground = Brushes.SaddleBrown;
            }
        }

        // Клик по лотку сдачи
        private void Change_Click(object sender, MouseButtonEventArgs e)
        {
            if (_currentChange == 0) return;

            ChangeMoneyText.Text = "0 ₽";
            _currentChange = 0;

            if (_isCupReady)
            {
                StatusText.Text = "Заберите напиток!";
                StatusText.Foreground = Brushes.SaddleBrown;
            }
            else
            {
                StatusText.Text = "Выберите напиток";
                StatusText.Foreground = Brushes.SaddleBrown;
            }
        }

        // Вспомогательный метод для таймера напоминания
        private async void ShowTemporaryStatus(string message, int durationMs)
        {
            StatusText.Text = message;
            StatusText.Foreground = Brushes.DarkOrange;
            await Task.Delay(durationMs);

            // Если за это время ничего не изменилось, возвращаем обычный статус
            if (!_isCupReady && _currentChange > 0)
            {
                StatusText.Text = "Выберите напиток"; // Или оставляем напоминание, но серым
                StatusText.Foreground = Brushes.SaddleBrown;
            }
        }
    }
}