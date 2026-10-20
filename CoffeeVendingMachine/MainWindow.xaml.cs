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
        private decimal _currentChange = 0;

        // Текущий выбранный напиток и его финальная цена
        private Drink _selectedDrink;
        private decimal _finalPrice = 0;
        private CupSize? _selectedSize;

        // Переменные для оплаты
        private decimal _insertedCash = 0;
        private decimal _paymentChange = 0;

        public MainWindow()
        {
            InitializeComponent();
            var menu = CoffeeLoader.LoadMenu();
            _config = CoffeeLoader.LoadConfig();
            DataContext = new { CoffeeList = menu };
        }

        // --- 1. ВЫБОР НАПИТКА И ПЕРЕХОД К НАСТРОЙКЕ ---
        private void CoffeeCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (_isBrewing) return;
            if (_isCupReady) { ShowTempStatus("Сначала заберите напиток!"); return; }

            if (sender is Border border && border.Tag is Drink selectedDrink)
            {
                _selectedDrink = selectedDrink;
                _selectedSize = null;
                _finalPrice = selectedDrink.BasePrice;

                // Настраиваем экран
                CustomCoffeeName.Text = selectedDrink.Name;
                CustomDrinkType.Text = selectedDrink is CoffeeDrink c ? $"Кофе ({c.CoffeeType})" : "Напиток";

                // Очищаем старые кнопки размеров и генерируем новые
                SizesPanel.Children.Clear();
                foreach (var size in _config.CupSizes)
                {
                    var rb = new RadioButton
                    {
                        Content = size.Name,
                        Tag = size,
                        Foreground = Brushes.White,
                        Margin = new Thickness(0, 4, 15, 4)
                    };
                    rb.Checked += Size_Changed;
                    SizesPanel.Children.Add(rb);
                }

                SyrupHeader.Text = $"Сиропы (+{_config.SyrupPrice} ₽):";
                ToppingHeader.Text = $"Топпинги (+{_config.ToppingPrice} ₽):";

                // Сбрасываем чекбоксы
                foreach (var child in SyrupPanel.Children) if (child is CheckBox cb) cb.IsChecked = false;
                foreach (var child in ToppingPanel.Children) if (child is CheckBox cb) cb.IsChecked = false;
                ShotStandard.IsChecked = true;

                CalculateCustomPrice();

                // Переключаем экраны
                MenuView.Visibility = Visibility.Collapsed;
                CustomizationView.Visibility = Visibility.Visible;
                PaymentView.Visibility = Visibility.Collapsed;
            }
        }

        // --- 2. НАСТРОЙКА НАПИТКА ---
        private void Size_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is CupSize size)
            {
                _selectedSize = size;
                CalculateCustomPrice();
            }
        }

        private void Option_Changed(object sender, RoutedEventArgs e)
        {
            CalculateCustomPrice();
        }

        private void CalculateCustomPrice()
        {
            if (_selectedDrink == null) return;

            decimal total = _selectedDrink.BasePrice;
            if (_selectedSize != null) total += _selectedSize.ExtraPrice;

            if (Shot1.IsChecked == true) total += _config.ExtraEspressoShotPrice;
            if (Shot2.IsChecked == true) total += _config.ExtraEspressoShotPrice * 2;

            foreach (var child in SyrupPanel.Children)
                if (child is CheckBox cb && cb.IsChecked == true) total += _config.SyrupPrice;
            foreach (var child in ToppingPanel.Children)
                if (child is CheckBox cb && cb.IsChecked == true) total += _config.ToppingPrice;

            _finalPrice = total;
            TotalPriceText.Text = $"Итого: {_finalPrice} ₽";
        }

        private void CancelCustomization_Click(object sender, RoutedEventArgs e)
        {
            // Возврат в меню
            MenuView.Visibility = Visibility.Visible;
            CustomizationView.Visibility = Visibility.Collapsed;
        }

        // --- 3. ПЕРЕХОД К ОПЛАТЕ ---
        private void ConfirmCustomization_Click(object sender, RoutedEventArgs e)
        {
            // Переключаем экраны
            CustomizationView.Visibility = Visibility.Collapsed;
            PaymentView.Visibility = Visibility.Visible;

            // Сбрасываем данные оплаты
            PaymentPriceText.Text = $"{_finalPrice} ₽";
            _insertedCash = 0;
            _paymentChange = 0;
            InsertedText.Text = "0 ₽";
            PaymentChangeText.Text = "0 ₽";
            ConfirmCashBtn.IsEnabled = false;
            PayByCardBtn.IsEnabled = true;
            PayByCardBtn.Content = "💳 ОПЛАТИТЬ КАРТОЙ";
        }

        // --- 4. ОПЛАТА ---
        private async void PayByCard_Click(object sender, RoutedEventArgs e)
        {
            PayByCardBtn.IsEnabled = false;
            PayByCardBtn.Content = "Обработка...";

            await Task.Delay(1500); // Имитация оплаты

            // Успешная оплата, сразу готовим
            await StartBrewingAsync(0);
        }

        private void InsertCash_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && decimal.TryParse(btn.Tag?.ToString(), out decimal amount))
            {
                _insertedCash += amount;
                InsertedText.Text = $"{_insertedCash} ₽";

                if (_insertedCash >= _finalPrice)
                {
                    _paymentChange = _insertedCash - _finalPrice;
                    PaymentChangeText.Text = $"{_paymentChange} ₽";
                    ConfirmCashBtn.IsEnabled = true;
                }
                else
                {
                    PaymentChangeText.Text = "0 ₽";
                    ConfirmCashBtn.IsEnabled = false;
                }
            }
        }

        private async void ConfirmCash_Click(object sender, RoutedEventArgs e)
        {
            if (_insertedCash >= _finalPrice)
            {
                await StartBrewingAsync(_paymentChange);
            }
        }

        private void CancelPayment_Click(object sender, RoutedEventArgs e)
        {
            // Возврат к настройке
            PaymentView.Visibility = Visibility.Collapsed;
            CustomizationView.Visibility = Visibility.Visible;
        }

        // --- 5. ПРИГОТОВЛЕНИЕ ---
        private async Task StartBrewingAsync(decimal change)
        {
            // Возвращаем меню (пользователь видит процесс)
            PaymentView.Visibility = Visibility.Collapsed;
            MenuView.Visibility = Visibility.Visible;

            _isBrewing = true;
            _currentChange = change;
            StatusText.Text = "Готовим...";

            ProgressContainer.Visibility = Visibility.Visible;
            BrewProgress.Value = 0;
            BrewingText.Text = "Готовим...";

            EmptyCupPlaceholder.Visibility = Visibility.Collapsed;
            CoffeeCup.Opacity = 1;
            CoffeeLiquid.Height = 0;

            for (int i = 0; i <= 30; i++)
            {
                BrewProgress.Value = i * (100.0 / 30.0);
                CoffeeLiquid.Height = (i / 30.0) * 60.0;
                await Task.Delay(100);
            }

            ProgressContainer.Visibility = Visibility.Collapsed;
            TakeCupText.Opacity = 1;
            _isCupReady = true;

            if (_currentChange > 0)
            {
                ChangeMoneyText.Text = $"{_currentChange} ₽";
                StatusText.Text = "Заберите напиток и сдачу!";
            }
            else
            {
                ChangeMoneyText.Text = "0 ₽";
                StatusText.Text = "Заберите напиток!";
            }

            _isBrewing = false;
        }

        // --- 6. ЗАБРАТЬ СТАКАН И СДАЧУ ---
        private void Cup_Click(object sender, MouseButtonEventArgs e)
        {
            if (!_isCupReady) return;

            CoffeeCup.Opacity = 0;
            TakeCupText.Opacity = 0;
            EmptyCupPlaceholder.Visibility = Visibility.Visible;
            _isCupReady = false;

            if (_currentChange > 0)
            {
                ShowTempStatus("Не забудьте забрать сдачу!", 5000);
            }
            else
            {
                StatusText.Text = "Выберите напиток";
            }
        }

        private void Change_Click(object sender, MouseButtonEventArgs e)
        {
            if (_currentChange == 0) return;

            ChangeMoneyText.Text = "0 ₽";
            _currentChange = 0;

            if (_isCupReady) StatusText.Text = "Заберите напиток!";
            else StatusText.Text = "Выберите напиток";
        }

        // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ---
        private async void ShowTempStatus(string message, int durationMs = 2000)
        {
            StatusText.Text = message;
            StatusText.Foreground = Brushes.Red;
            await Task.Delay(durationMs);

            if (!_isCupReady && _currentChange == 0)
            {
                StatusText.Text = "Выберите напиток";
                StatusText.Foreground = Brushes.SaddleBrown;
            }
            StatusText.Foreground = Brushes.SaddleBrown; // Возвращаем цвет
        }
    }
}