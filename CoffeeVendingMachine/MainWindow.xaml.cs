using System.Windows;
using System.Windows.Input;
using CoffeeVendingMachine.Services;
using CoffeeVendingMachine.Models;
using System.Windows.Controls;


namespace CoffeeVendingMachine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //Гркзим список кофе из coffee.json
            var coffees = CoffeeLoader.Load();

            // DataContext - Сам строит карточки
            DataContext = new
            {
                CoffeeList = coffees
            };
        }

        //Обробатываем клик по карточке
        private void CoffeeCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Coffee selectedCoffee)
            {
                var customizationWindow = new CustomizationWindow(selectedCoffee)
                {
                    Owner = this
                };

                bool? result = customizationWindow.ShowDialog();

                if (result == true)
                {
                    // Пока так
                    MessageBox.Show($"Напиток {selectedCoffee.Name} успешно выбран!\n(Здесь будет логика оплаты)", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
        }


    }
}