using System.Windows;

namespace CoffeeVendingMachine
{
    public partial class PasswordWindow : Window
    {
        public bool IsAuthenticated { get; private set; } = false;

        public PasswordWindow()
        {
            InitializeComponent();
            PasswordInput.Focus();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            // Пароль: admin
            if (PasswordInput.Password == "admin")
            {
                IsAuthenticated = true;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Неверный пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                PasswordInput.Clear();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}