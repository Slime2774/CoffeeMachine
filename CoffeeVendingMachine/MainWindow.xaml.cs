using System.Windows;
using System.Windows.Controls;

namespace CoffeeVendingMachine
{
    public partial class MainWindow : Window
    {
        private int _machineCounter = 0;

        public MainWindow()
        {
            InitializeComponent();
            AddNewMachine(); // Создаем первый автомат по умолчанию
        }

        private void AddMachine_Click(object sender, RoutedEventArgs e)
        {
            AddNewMachine();
        }

        private void AddNewMachine()
        {
            _machineCounter++;
            var machine = new CoffeeMachineView($"machine_{_machineCounter}");
            MachinesContainer.Children.Add(machine);

            // Автопрокрутка к новому автомату
            ((ScrollViewer)MachinesContainer.Parent).ScrollToRightEnd();
        }
    }
}