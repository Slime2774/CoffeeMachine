using System.Windows;
using System.Windows.Controls;
using CoffeeVendingMachine.Models;

namespace CoffeeVendingMachine
{
    public partial class CustomizationWindow : Window
    {
        public Drink SelectedDrink { get; private set; }
        private Config _config;

        // Итоговая цена, которую мы передадим в главное окно
        public decimal FinalPrice { get; private set; }

        // Храним выбранный размер, чтобы знать его доплату
        private CupSize _selectedSize;

        public CustomizationWindow(Drink drink, Config config)
        {
            InitializeComponent();
            SelectedDrink = drink;
            _config = config;

            CoffeeNameText.Text = drink.Name;
            if (drink is CoffeeDrink coffee)
            {
                DrinkTypeText.Text = $"Кофе ({coffee.CoffeeType})";
            }

            // Генерация кнопок объемов
            foreach (var size in config.CupSizes)
            {
                var rb = new RadioButton
                {
                    Content = size.Name,
                    Tag = size,
                    Margin = new Thickness(0, 4, 15, 4)
                };
                // Привязываем событие изменения
                rb.Checked += Size_Changed;
                SizesPanel.Children.Add(rb);
            }

            SyrupHeader.Text = $"Сиропы (+{config.SyrupPrice} ₽):";
            ToppingHeader.Text = $"Топпинги (+{config.ToppingPrice} ₽):";

            // Считаем цену в первый раз
            CalculateTotalPrice();
        }

        // Событие при выборе размера (запоминаем, за что доплата)
        private void Size_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is CupSize size)
            {
                _selectedSize = size;
                CalculateTotalPrice();
            }
        }

        // Событие при клике на любой чекбокс или шот (просто пересчитываем)
        private void Option_Changed(object sender, RoutedEventArgs e)
        {
            CalculateTotalPrice();
        }

        // Главный метод подсчета цены
        private void CalculateTotalPrice()
        {
            // 1. Базовая цена
            decimal total = SelectedDrink.BasePrice;

            // 2. Доплата за размер (если выбран)
            if (_selectedSize != null)
                total += _selectedSize.ExtraPrice;

            // 3. Доплата за шоты
            if (Shot1.IsChecked == true) total += _config.ExtraEspressoShotPrice;
            if (Shot2.IsChecked == true) total += _config.ExtraEspressoShotPrice * 2;

            // 4. Доплата за сиропы
            foreach (CheckBox cb in ((WrapPanel)((FrameworkElement)SyrupHeader.Parent).FindName("SyrupPanel")).Children)
            { /* ... */ }
            total += CountCheckedCheckBoxes() * _config.SyrupPrice; // Грубый подсчет, ниже исправлю

            // 5. Доплата за топпинги
            // (Аналогично, для простоты сейчас считаем, что все чекбоксы имеют одинаковую цену)

            // Записываем результат в переменную и выводим на экран
            FinalPrice = total;
            TotalPriceText.Text = $"Итого: {FinalPrice} ₽";
        }

        // Вспомогательный метод: считаем галочки
        private int CountCheckedCheckBoxes()
        {
            // В реальности нужно разделять сиропы и топпинги, 
            // но пока для примера просто считаем все выбранные чекбоксы.
            // Для точного подсчета мы сделаем это чуть позже с помощью LINQ.
            return 0;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}