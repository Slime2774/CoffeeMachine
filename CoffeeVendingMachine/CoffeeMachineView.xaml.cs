using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Media; // Для звуков
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using CoffeeVendingMachine.Models;
using CoffeeVendingMachine.Services;
using System.Windows.Threading;

namespace CoffeeVendingMachine
{
    public partial class CoffeeMachineView : UserControl
    {
        public Inventory Inventory { get; set; } = new Inventory();
        public string MachineId { get; set; }
        public ObservableCollection<Drink> MenuList { get; set; } = new ObservableCollection<Drink>();

        private Config _config = new Config();
        private bool _isBrewing = false;
        private bool _isCupReady = false;
        private decimal _currentChange = 0;
        private Drink? _selectedDrink;
        private decimal _finalPrice = 0;
        private CupSize? _selectedSize;
        private decimal _insertedCash = 0;
        private decimal _paymentChange = 0;

        private DispatcherTimer _idleTimer;
        private int _idleSeconds = 0;
        private const int IdleTimeoutSeconds = 15; // Через сколько секунд включать заставку (30 сек)

        // Плеер для звуков
        private SoundPlayer? _soundPlayer;

        public CoffeeMachineView(string machineId)
        {
            InitializeComponent();
            MachineId = machineId;
            DataContext = this;
            _config = CoffeeLoader.LoadConfig();

            LoadMenuFromJson();
            UpdateCupStackVisual();

            // ДОБАВЛЕНО: Инициализация таймера
            _idleTimer = new DispatcherTimer();
            _idleTimer.Interval = TimeSpan.FromSeconds(1);
            _idleTimer.Tick += IdleTimer_Tick;
            _idleTimer.Start();

            // ДОБАВЛЕНО: Сбрасываем таймер при клике на любую часть автомата
            this.PreviewMouseDown += (s, e) => ResetIdleTimer();
        }


        // --- ВОСПРОИЗВЕДЕНИЕ ЗВУКОВ ---
        private void PlaySound(string fileName)
        {
            // Сначала останавливаем предыдущий звук, если он играет
            _soundPlayer?.Stop();
            _soundPlayer = null;

            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds", fileName);

                // Проверка: если файла нет, просто выходим
                if (File.Exists(path))
                {
                    _soundPlayer = new SoundPlayer(path);
                    _soundPlayer.Play();
                }
            }
            catch
            {
                // Если произошла ошибка воспроизведения (битый файл, нет кодеков), 
                // просто игнорируем её, чтобы не ломать автомат.
                _soundPlayer = null;
            }
        }

        // --- ЗАГРУЗКА И СОХРАНЕНИЕ JSON ---
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

        // --- ЛОГИКА АВТОМАТА ---
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

                SizesPanel.Children.Clear();
                if (_config.CupSizes == null || _config.CupSizes.Count == 0)
                {
                    _config.CupSizes = new List<CupSize>
                    {
                        new CupSize { Name = "Маленький", ExtraPrice = 0 },
                        new CupSize { Name = "Средний", ExtraPrice = 20 },
                        new CupSize { Name = "Большой", ExtraPrice = 40 }
                    };
                }

                bool isFirst = true;
                foreach (var size in _config.CupSizes)
                {
                    var rb = new RadioButton { Content = size.Name, Tag = size, Foreground = Brushes.White, Margin = new Thickness(0, 4, 15, 4), IsChecked = isFirst };
                    rb.Checked += Size_Changed;
                    SizesPanel.Children.Add(rb);
                    isFirst = false;
                }

                SyrupHeader.Text = $"Сиропы (+{_config.SyrupPrice} ₽):";
                ToppingHeader.Text = $"Топпинги (+{_config.ToppingPrice} ₽):";

                foreach (var child in SyrupPanel.Children) if (child is CheckBox cb) cb.IsChecked = false;
                foreach (var child in ToppingPanel.Children) if (child is CheckBox cb) cb.IsChecked = false;
                ShotStandard.IsChecked = true;
                Sugar0.IsChecked = true;

                CalculateCustomPrice();

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

        // --- ОПЛАТА ---
        private async void PayByCard_Click(object sender, RoutedEventArgs e)
        {
            PayByCardBtn.IsEnabled = false; PayByCardBtn.Content = "Обработка...";
            PlaySound("beep.wav"); // ЗВУК ТЕРМИНАЛА
            await Task.Delay(1500);
            await StartBrewingAsync(0);
        }

        private void InsertCash_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && decimal.TryParse(btn.Tag?.ToString(), out decimal amount))
            {
                PlaySound("coin.wav"); // ЗВУК МОНЕТЫ
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

        // --- ПРИГОТОВЛЕНИЕ ---
        private async Task StartBrewingAsync(decimal change)
        {
            _isBrewing = true;
            _currentChange = change;

            // Расход ресурсов
            int targetVolume = _selectedSize?.Volume ?? 300;
            int waterNeeded = targetVolume / 2;
            int milkNeeded = targetVolume / 2;
            int shots = 1 + (Shot1.IsChecked == true ? 1 : 0) + (Shot2.IsChecked == true ? 2 : 0);
            int coffeeNeeded = shots * 10;
            int sugarNeeded = Sugar1.IsChecked == true ? 5 : (Sugar2.IsChecked == true ? 10 : (Sugar3.IsChecked == true ? 15 : 0));
            int syrupsCount = 0;
            foreach (var child in SyrupPanel.Children) if (child is CheckBox cb && cb.IsChecked == true) syrupsCount++;
            int syrupNeeded = syrupsCount * 20;

            if (Inventory.Cups <= 0 || Inventory.Water < waterNeeded ||
                Inventory.Milk < milkNeeded || Inventory.CoffeeBeans < coffeeNeeded ||
                Inventory.Sugar < sugarNeeded || Inventory.Syrup < syrupNeeded)
            {
                StatusText.Text = "Ошибка: не хватает ресурсов!";
                _isBrewing = false;
                return; // Прерываем метод, кофе не готовится, ресурсы не списываются
            }

            Inventory.Cups--;
            Inventory.Water -= waterNeeded;
            Inventory.Milk -= milkNeeded;
            Inventory.CoffeeBeans -= coffeeNeeded;
            Inventory.Sugar -= sugarNeeded;
            Inventory.Syrup -= syrupNeeded;
            UpdateCupStackVisual();

            PaymentView.Visibility = Visibility.Collapsed;
            MenuView.Visibility = Visibility.Visible;
            StatusText.Text = "Готовим...";
            ProgressContainer.Visibility = Visibility.Visible;
            BrewProgress.Value = 0;

            EmptyCupPlaceholder.Visibility = Visibility.Collapsed;
            CoffeeCup.Opacity = 1;
            CoffeeLiquid.Height = 0;

            PlaySound("brew.wav"); // ЗВУК ПРИГОТОВЛЕНИЯ

            for (int i = 0; i <= 30; i++)
            {
                BrewProgress.Value = i * (100.0 / 30.0);
                CoffeeLiquid.Height = (i / 30.0) * 60.0;
                await Task.Delay(100);
            }

            // ИСПРАВЛЕНО: Останавливаем звук, чтобы он не играл дольше, чем готовится кофе
            _soundPlayer?.Stop();

            ProgressContainer.Visibility = Visibility.Collapsed;
            TakeCupText.Opacity = 1;
            _isCupReady = true;

            if (_currentChange > 0)
            {
                ShowCoins(_currentChange); // РИСУЕМ МОНЕТЫ
                StatusText.Text = "Заберите напиток и сдачу!";
            }
            else
            {
                ChangeMoneyText.Text = "0 ₽";
                ChangeCoinsPanel.Children.Clear();
                StatusText.Text = "Заберите напиток!";
            }

            _isBrewing = false;
        }

        // --- ОТРИСОВКА 5 МОНЕТ ---
        private void ShowCoins(decimal amount)
        {
            ChangeCoinsPanel.Children.Clear();
            ChangeMoneyText.Visibility = Visibility.Collapsed;

            if (amount <= 0) return;

            // Разбиваем сумму ровно на 5 частей (или меньше, если сумма < 5)
            int coinsCount = 5;
            decimal coinValue = Math.Floor(amount / coinsCount);
            decimal remainder = amount - (coinValue * coinsCount);

            // Если сумма слишком маленькая (например, 3 рубля), рисуем 3 монеты по 1 рублю
            if (coinValue == 0)
            {
                coinsCount = (int)amount;
                coinValue = 1;
                remainder = 0;
            }

            for (int i = 0; i < coinsCount; i++)
            {
                decimal currentCoin = coinValue;
                if (i == coinsCount - 1) currentCoin += remainder; // Остаток кидаем в последнюю монету

                var coin = new Border
                {
                    Width = 28,
                    Height = 28,
                    CornerRadius = new CornerRadius(14),
                    Background = new LinearGradientBrush(Color.FromRgb(255, 215, 0), Color.FromRgb(218, 165, 32), 45),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(184, 134, 11)),
                    BorderThickness = new Thickness(1.5),
                    Margin = new Thickness(2),
                    Child = new TextBlock
                    {
                        Text = currentCoin.ToString("0"),
                        Foreground = new SolidColorBrush(Color.FromRgb(60, 40, 20)),
                        FontWeight = FontWeights.Bold,
                        FontSize = 10,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                ChangeCoinsPanel.Children.Add(coin);
            }

            PlaySound("coin.wav"); // ЗВУК ПАДАЮЩИХ МОНЕТ
        }

        // --- ВЫДАЧА ---
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
            ChangeMoneyText.Text = "0 ₽";
            ChangeCoinsPanel.Children.Clear();
            _currentChange = 0;
            PlaySound("coin.wav"); // Звук забора сдачи
            StatusText.Text = _isCupReady ? "Заберите напиток!" : "Выберите напиток";
        }

        // --- АДМИНКА ---
        private void ToggleAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (AdminPanelView.Visibility == Visibility.Collapsed)
            {
                // ТРЕБУЕМ ПАРОЛЬ
                var passWin = new PasswordWindow { Owner = Window.GetWindow(this) };
                if (passWin.ShowDialog() == true && passWin.IsAuthenticated)
                {
                    AdminPanelView.Visibility = Visibility.Visible;
                }
            }
            else
            {
                AdminPanelView.Visibility = Visibility.Collapsed;
            }
        }

        private void RefillInventory_Click(object sender, RoutedEventArgs e)
        {
            Inventory.Cups = Inventory.MaxCups;
            Inventory.CoffeeBeans = Inventory.MaxCoffee;
            Inventory.Water = Inventory.MaxWater;
            Inventory.Milk = Inventory.MaxMilk;
            Inventory.Sugar = Inventory.MaxSugar;
            Inventory.Syrup = Inventory.MaxSyrup;
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

        // ДОБАВЛЕНО: Логика таймера
        private void IdleTimer_Tick(object? sender, EventArgs e)
        {
            // Если автомат занят (готовит или ждет стакан) или открыта админка — не показываем заставку
            if (_isBrewing || _isCupReady || AdminPanelView.Visibility == Visibility.Visible)
            {
                _idleSeconds = 0;
                return;
            }

            // Считаем время только если открыто главное меню
            if (MenuView.Visibility == Visibility.Visible)
            {
                _idleSeconds++;
                if (_idleSeconds >= IdleTimeoutSeconds)
                {
                    ShowScreenSaver();
                }
            }
        }

        private void ResetIdleTimer()
        {
            _idleSeconds = 0;

            // Если заставка была включена, убираем её
            if (ScreenSaverView.Visibility == Visibility.Visible)
            {
                ScreenSaverView.Visibility = Visibility.Collapsed;
                MenuView.Visibility = Visibility.Visible;
            }
        }

        private void ShowScreenSaver()
        {
            ScreenSaverView.Visibility = Visibility.Visible;
        }

        private void ScreenSaver_Click(object sender, MouseButtonEventArgs e)
        {
            ResetIdleTimer();
        }

    }
}