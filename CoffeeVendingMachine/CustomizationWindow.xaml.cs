using System.Windows;
using CoffeeVendingMachine.Models;

namespace CoffeeVendingMachine
{
    /// <summary>
    /// Логика взаимодействия для CustomizationWindow.xaml
    /// </summary>
    public partial class CustomizationWindow : Window
    {
        //Передаёим кофе из главного меню
        public Coffee SelectedCoffee { get; private set; }

        public CustomizationWindow(Coffee coffee)
        {
            InitializeComponent();

            SelectedCoffee = coffee;

            CoffeeNameText.Text = coffee.Name; // Название кофе в заголовок 
        }
        
        //Кнопка "Отмена" - закрыть окно
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // Кнопка "Добавить в заказ"
        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            // Нужно потом прописать логику и объединить с главной

            this.DialogResult = true;
            this.Close();

        }



    }
}
