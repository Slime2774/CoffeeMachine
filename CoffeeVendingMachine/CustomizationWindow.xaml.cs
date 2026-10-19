using System.Windows;
using System.Windows.Controls;
using CoffeeVendingMachine.Models;

namespace CoffeeVendingMachine
{
    public partial class CustomizationWindow : Window
    {
        // Ссылка на выбранный напиток
        public Drink SelectedDrink { get; private set; }

        // Ссылка на настройки автомата
        private Config _config;

        public CustomizationWindow(Drink drink, Config config)
        {
            InitializeComponent();
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
                    Tag = size.Volume, // Сохраняем объем в Tag, чтобы потом прочитать
                    Margin = new Thickness(0, 4, 15, 4)
                };
                SizesPanel.Children.Add(rb);
            }

            //Обновляем заголовки, подставляя цены из конфига
            SyrupHeader.Text = $"Сиропы (+{config.SyrupPrice} ₽):";
            ToppingHeader.Text = $"Топпинги (+{config.ToppingPrice} ₽):";
            TotalPriceText.Text = $"Итого: {drink.BasePrice} ₽";
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