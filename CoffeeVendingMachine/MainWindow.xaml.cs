using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CoffeeVendingMachine.Services;


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
    }
}