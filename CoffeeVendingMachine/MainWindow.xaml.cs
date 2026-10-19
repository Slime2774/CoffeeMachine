using CoffeeVendingMachine.Models;
using CoffeeVendingMachine.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CoffeeVendingMachine
{
    public partial class MainWindow : Window
    {
        private Config _config;

        public MainWindow()
        {
            InitializeComponent();

            // 1. Загружаем меню из menu.json (список наследников Drink)
            var menu = CoffeeLoader.LoadMenu();

            // 2. Загружаем настройки из config.json (цены, объемы)
            _config = CoffeeLoader.LoadConfig();

            // 3. Устанавливаем контекст данных. 
            // ItemsControl в XAML сам отрисует карточки на основе этого списка.
            DataContext = new { CoffeeList = menu };
        }

        // Обработчик клика по карточке
        private void CoffeeCard_Click(object sender, MouseButtonEventArgs e)
        {
            // Получаем объект напитка из Tag карточки
            if (sender is Border border && border.Tag is Drink selectedDrink)
            {
                // Открываем окно настройки, передавая туда напиток и конфиг
                var win = new CustomizationWindow(selectedDrink, _config) { Owner = this };

                // ShowDialog блокирует главное окно, пока не закроется окно настройки
                if (win.ShowDialog() == true)
                {
                    // Сюда мы добавим логику оплаты и приготовления
                    MessageBox.Show($"{selectedDrink.Name} готовится!", "Успех");
                }
            }
        }
    }
}