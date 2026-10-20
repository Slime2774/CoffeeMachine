using CoffeeVendingMachine.Models;
using CoffeeVendingMachine.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CoffeeVendingMachine
{
    public partial class CoffeeMachineView : UserControl
    {
        public Inventory Inventory { get; set; } = new Inventory();
        public string MachineId { get; set; }
        public ObservableCollection<Drink> MenuList { get; set; } = new ObservableCollection<Drink>();

        private Config _config;
        private bool _isBrewing = false;
        private bool _isCupReady = false;
        private decimal _currentChange = 0;
        private Drink? _selectedDrink;
        private decimal _finalPrice = 0;
        private CupSize? _selectedSize;
        private decimal _insertedCash = 0;
        private decimal _paymentChange = 0;

        public CoffeeMachineView(string machineId)
        {
            InitializeComponent();
            MachineId = machineId;
            DataContext = this;

            // ИСПРАВЛЕНО: Загружаем настройки из config.json
            _config = CoffeeLoader.LoadConfig();

            LoadMenuFromJson();
            UpdateCupStackVisual();
        }

        private void LoadMenuFromJson()
        {
            string fileName = $"menu_{MachineId}.json";
            if (!File.Exists(fileName))
            {
                var defaultMenu = new List<object>
                {
                    new { type = "CoffeeDrink", name = "Капучино", basePrice = 150, icon = "☕", coffeeType = "Arabica" }
                };
                File.WriteAllText(fileName, JsonSerializer.Serialize(defaultMenu, new JsonSerializerOptions { WriteIndented = true }));
            }

            string json = File.ReadAllText(fileName);
            try
            {
                var menu = JsonSerializer.Deserialize<List<CoffeeDrink>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (menu != null)
                {
                    MenuList.Clear();
                    foreach (var item in menu) MenuList.Add(item);
                    CoffeeItemsControl.ItemsSource = MenuList;
                    MenuDataGrid.ItemsSource = MenuList;
                }
            }
            catch { }
        }

        private void SaveJson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string json = JsonSerializer.Serialize(MenuList, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText($"menu_{MachineId}.json", json);
                MessageBox.Show("Меню успешно сохранено!", "Успех");
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
        }

        private void AddMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuList.Add(new CoffeeDrink { Name = "Новый напиток", BasePrice = 100, Icon = "☕", CoffeeType = "Arabica" });
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MenuDataGrid.SelectedItem is Drink drink) MenuList.Remove(drink);
        }

        private void CoffeeCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (_isBrewing || _isCupReady) return;
            if (Inventory.Cups <= 0) { StatusText.Text = "Нет стаканов! Пополните."; return; }

            if (sender is Border border && border.Tag is Drink selectedDrink)
            {
                _selectedDrink = selectedDrink;
                _finalPrice = selectedDrink.BasePrice;
                _selectedSize = null;

                CustomCoffeeName.Text = selectedDrink.Name;
                CustomDrinkType.Text = selectedDrink is CoffeeDrink c ? $"Кофе ({c.CoffeeType})" : "Напиток";

                // Очищаем старые кнопки
                SizesPanel.Children.Clear();

                // ПРОВЕРКА: Если размеров нет, создаем стандартные
                if (_config.CupSizes == null || _config.CupSizes.Count == 0)
                {
                    _config.CupSizes = new List<CupSize>
                    {
                        new CupSize { Name = "Маленький (200 мл)", ExtraPrice = 0 },
                        new CupSize { Name = "Средний (300 мл)", ExtraPrice = 20 },
                        new CupSize { Name = "Большой (400 мл)", ExtraPrice = 40 }
                    };
                }

                bool isFirst = true;
                foreach (var size in _config.CupSizes)
                {
                    var rb = new RadioButton
                    {
                        Content = size.Name,
                        Tag = size,
                        Foreground = Brushes.White,
                        Margin = new Thickness(0, 4, 15, 4),
                        IsChecked = isFirst // Автоматически выбираем первый размер
                    };
                    rb.Checked += Size_Changed;
                    SizesPanel.Children.Add(rb);
                    isFirst = false;
                }

                SyrupHeader.Text = $"Сиропы (+{_config.SyrupPrice} ₽):";
                ToppingHeader.Text = $"Топпинги (+{_config.ToppingPrice} ₽):";

                // Сбрасываем галочки
                foreach (var child in SyrupPanel.Children) if (child is CheckBox cb) cb.IsChecked = false;
                foreach (var child in ToppingPanel.Children) if (child is CheckBox cb) cb.IsChecked = false;
                ShotStandard.IsChecked = true;

                // Пересчитываем цену с учетом выбранного по умолчанию размера
                CalculateCustomPrice();

                // Переключаем экраны
                MenuView.Visibility = Visibility.Collapsed;
                CustomizationView.Visibility = Visibility.Visible;
                PaymentView.Visibility = Visibility.Collapsed;
            }
        }

        private void Size_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is CupSize size) { _selectedSize = size; CalculateCustomPrice(); }
        }

        private void Option_Changed(object sender, RoutedEventArgs e) { CalculateCustomPrice(); }

        private void CalculateCustomPrice()
        {
            if (_selectedDrink == null) return;
            decimal total = _selectedDrink.BasePrice;
            if (_selectedSize != null) total += _selectedSize.ExtraPrice;
            if (Shot1.IsChecked == true) total += _config.ExtraEspressoShotPrice;
            if (Shot2.IsChecked == true) total += _config.ExtraEspressoShotPrice * 2;
            foreach (var child in SyrupPanel.Children) if (child is CheckBox cb && cb.IsChecked == true) total += _config.SyrupPrice;
            foreach (var child in ToppingPanel.Children) if (child is CheckBox cb && cb.IsChecked == true) total += _config.ToppingPrice;

            _finalPrice = total;
            TotalPriceText.Text = $"Итого: {_finalPrice} ₽";
        }

        private void CancelCustomization_Click(object sender, RoutedEventArgs e)
        {
            MenuView.Visibility = Visibility.Visible;
            CustomizationView.Visibility = Visibility.Collapsed;
        }

        private void ConfirmCustomization_Click(object sender, RoutedEventArgs e)
        {
            CustomizationView.Visibility = Visibility.Collapsed;
            PaymentView.Visibility = Visibility.Visible;
            PaymentPriceText.Text = $"{_finalPrice} ₽";
            _insertedCash = 0; _paymentChange = 0;
            InsertedText.Text = "0 ₽"; PaymentChangeText.Text = "0 ₽";
            ConfirmCashBtn.IsEnabled = false; PayByCardBtn.IsEnabled = true; PayByCardBtn.Content = "💳 ОПЛАТИТЬ КАРТОЙ";
        }

        private async void PayByCard_Click(object sender, RoutedEventArgs e)
        {
            PayByCardBtn.IsEnabled = false; PayByCardBtn.Content = "Обработка...";
            await Task.Delay(1500);
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
                else { PaymentChangeText.Text = "0 ₽"; ConfirmCashBtn.IsEnabled = false; }
            }
        }

        private async void ConfirmCash_Click(object sender, RoutedEventArgs e)
        {
            if (_insertedCash >= _finalPrice) await StartBrewingAsync(_paymentChange);
        }

        private void CancelPayment_Click(object sender, RoutedEventArgs e)
        {
            PaymentView.Visibility = Visibility.Collapsed;
            CustomizationView.Visibility = Visibility.Visible;
        }

        private async Task StartBrewingAsync(decimal change)
        {
            _isBrewing = true;
            _currentChange = change;

            // --- 1. СЧИТАЕМ РАСХОД РЕСУРСОВ ---

            // Объем (если размер не выбран, берем 300 мл по умолчанию)
            int targetVolume = _selectedSize?.Volume ?? 300;

            // Условно делим объем пополам: половина вода, половина молоко
            int waterNeeded = targetVolume / 2;
            int milkNeeded = targetVolume / 2;

            // Кофе: 1 стандартный шот (10г) + дополнительные
            int shots = 1;
            if (Shot1.IsChecked == true) shots += 1;
            if (Shot2.IsChecked == true) shots += 2;
            int coffeeNeeded = shots * 10;

            // Сахар: 1 ложка = 5г
            int sugarNeeded = 0;
            if (Sugar1.IsChecked == true) sugarNeeded = 5;
            else if (Sugar2.IsChecked == true) sugarNeeded = 10;
            else if (Sugar3.IsChecked == true) sugarNeeded = 15;

            // Сиропы: 1 сироп = 20мл
            int syrupsCount = 0;
            foreach (var child in SyrupPanel.Children) if (child is CheckBox cb && cb.IsChecked == true) syrupsCount++;
            int syrupNeeded = syrupsCount * 20;

            // Топпинги (пока не добавляли в инвентарь, но можно сделать по аналогии)

            // --- 2. СПИСЫВАЕМ ---

            // Проверка (чтобы не уйти в минус)
            if (Inventory.Water < waterNeeded || Inventory.CoffeeBeans < coffeeNeeded || Inventory.Cups <= 0)
            {
                StatusText.Text = "Ошибка: не хватает ресурсов!";
                _isBrewing = false;
                return;
            }

            Inventory.Cups--;
            Inventory.Water -= waterNeeded;
            Inventory.Milk -= milkNeeded;
            Inventory.CoffeeBeans -= coffeeNeeded;
            Inventory.Sugar -= sugarNeeded;
            Inventory.Syrup -= syrupNeeded;

            UpdateCupStackVisual();

            // --- 3. АНИМАЦИЯ ---
            PaymentView.Visibility = Visibility.Collapsed;
            MenuView.Visibility = Visibility.Visible;
            StatusText.Text = "Готовим...";
            ProgressContainer.Visibility = Visibility.Visible;
            BrewProgress.Value = 0;

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

            if (_currentChange > 0) { ChangeMoneyText.Text = $"{_currentChange} ₽"; StatusText.Text = "Заберите напиток и сдачу!"; }
            else { ChangeMoneyText.Text = "0 ₽"; StatusText.Text = "Заберите напиток!"; }

            _isBrewing = false;
        }

        private void Cup_Click(object sender, MouseButtonEventArgs e)
        {
            if (!_isCupReady) return;
            CoffeeCup.Opacity = 0; TakeCupText.Opacity = 0;
            EmptyCupPlaceholder.Visibility = Visibility.Visible;
            _isCupReady = false;

            if (_currentChange > 0) StatusText.Text = "Заберите сдачу!";
            else StatusText.Text = "Выберите напиток";
        }

        private void Change_Click(object sender, MouseButtonEventArgs e)
        {
            if (_currentChange == 0) return;
            ChangeMoneyText.Text = "0 ₽"; _currentChange = 0;
            StatusText.Text = _isCupReady ? "Заберите напиток!" : "Выберите напиток";
        }

        private void ToggleAdmin_Click(object sender, RoutedEventArgs e)
        {
            AdminPanelView.Visibility = AdminPanelView.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void RefillInventory_Click(object sender, RoutedEventArgs e)
        {
            Inventory.Cups = Inventory.MaxCups;
            Inventory.CoffeeBeans = Inventory.MaxCoffee;
            Inventory.Water = Inventory.MaxWater;
            Inventory.Milk = Inventory.MaxMilk;
            Inventory.Sugar = Inventory.MaxSugar; // ДОБАВЛЕНО
            Inventory.Syrup = Inventory.MaxSyrup; // ДОБАВЛЕНО
            UpdateCupStackVisual();
        }

        private void UpdateCupStackVisual()
        {
            CupStackPanel.Children.Clear();
            int displayCount = Math.Min(Inventory.Cups, 8);
            for (int i = 0; i < displayCount; i++)
            {
                var cup = new System.Windows.Shapes.Path
                {
                    Data = Geometry.Parse("M 5,0 L 25,0 L 22,20 Q 15,22 8,20 Z"),
                    Fill = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                    Stroke = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                    StrokeThickness = 1,
                    Height = 20,
                    Width = 30,
                    Margin = new Thickness(0, -5, 0, 0)
                };
                CupStackPanel.Children.Insert(0, cup);
            }
        }
    }
}