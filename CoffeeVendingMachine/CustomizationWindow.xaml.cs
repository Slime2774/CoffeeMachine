using System.Windows;
using System.Windows.Controls;
using CoffeeVendingMachine.Models;

namespace CoffeeVendingMachine
{
    public partial class CustomizationWindow : Window
    {
        // Ссылка на выбранный напиток
        public Drink? SelectedDrink { get; private set; }

        // Ссылка на настройки автомата
        private Config? _config;

        //Финальная цена
        public decimal FinalPrice { get; private set; }

        // Пока не выбран объём null
        private CupSize? _selectedSize;

        public CustomizationWindow(Drink drink, Config config)
        {
            InitializeComponent();

            if (drink == null || config == null)
            {
                MessageBox.Show("Ошибка: данные о напитке не переданы. Запускайте программу через MainWindow!");
                // Закрываем окно сразу, чтобы оно не пыталось рисоваться без данных
                Application.Current.Shutdown();
                return;
            }

            SelectedDrink = drink;
            _config = config;

            // 1. Выводим имя напитка
            CoffeeNameText.Text = drink.Name;

            // Проверяем, является ли объект наследником CoffeeDrink.
            // Если да — приводим тип и получаем доступ к свойству CoffeeType.
            if (drink is CoffeeDrink coffee)
            {
                DrinkTypeText.Text = $"Кофе ({coffee.CoffeeType})";
            }

            // Берем список объемов из config.json и создаем для каждого кнопку.
            foreach (var size in config.CupSizes)
            {
                var rb = new RadioButton
                {
                    Content = size.Name,
                    Tag = size, // Сохраняем объем в Tag, чтобы потом прочитать
                    Margin = new Thickness(0, 4, 15, 4)
                };
                rb.Checked += Size_Changed;
                SizesPanel.Children.Add(rb);
            }

            //Обновляем заголовки, подставляя цены из конфига
            SyrupHeader.Text = $"Сиропы (+{config.SyrupPrice} ₽):";
            ToppingHeader.Text = $"Топпинги (+{config.ToppingPrice} ₽):";
            //TotalPriceText.Text = $"Итого: {drink.BasePrice} ₽";

            // Первичный расчёт
            CalculateTotalPrice();
        }

        // Событие изменения объёма
        private void Size_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is CupSize size)
            {
                _selectedSize = size; // Запоминаем размер 
                CalculateTotalPrice(); // Пересчитываем сумму
            }
        }

        private void Option_Changed(object sender, RoutedEventArgs e)
        {
            CalculateTotalPrice();
        }

        private void CalculateTotalPrice()
        {
            if (TotalPriceText == null || SelectedDrink == null)
            {
                return;
            }


            decimal total = SelectedDrink.BasePrice;

            //Если выбран размер, прибавляем к сумме
            if (_selectedSize != null)
            {
                total += _selectedSize.ExtraPrice;
            }

            // Доплата за крепость
            if (Shot1.IsChecked == true) total += _config.ExtraEspressoShotPrice;
            if (Shot2.IsChecked == true) total += _config.ExtraEspressoShotPrice * 2;

            // Перебираем сиропы
            foreach (var child in SyrupPanel.Children)
            {
                // Если отмечен, + к цене
                if (child is CheckBox cb && cb.IsChecked == true)
                {
                    total += _config.SyrupPrice;
                }
            }

            // Перебираем топпинги
            foreach (var child in ToppingPanel.Children)
            {
                // Если отмечен, + к цене
                if (child is CheckBox cb && cb.IsChecked == true)
                {
                    total += _config.ToppingPrice;
                }
            }
            FinalPrice = total;
            TotalPriceText.Text = $"Итого: {FinalPrice} ₽";

        }


        // Закрыть без сохранения
        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

        // Закрыть с подтверждением
        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}