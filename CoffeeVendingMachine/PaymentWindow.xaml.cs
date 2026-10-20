using System;
using System.Windows;
using System.Windows.Controls;

namespace CoffeeVendingMachine
{
    public partial class PaymentWindow : Window
    {
        private decimal _finalPrice;
        private decimal _insertedCash = 0;

        public bool IsPaymentSuccessful { get; private set; } = false;
        public decimal Change { get; private set; } = 0;

        public PaymentWindow(decimal finalPrice)
        {
            InitializeComponent();
            _finalPrice = finalPrice;
            TotalPriceText.Text = $"{finalPrice} ₽";
        }

        // Оплата картой (SystemPay)
        private async void PayByCard_Click(object sender, RoutedEventArgs e)
        {
            PayByCardBtn.IsEnabled = false;
            PayByCardBtn.Content = "Обработка...";

            // Имитация задержки терминала
            await System.Threading.Tasks.Task.Delay(1500);

            MessageBox.Show("Оплата картой прошла успешно!", "SystemPay");

            IsPaymentSuccessful = true;
            Change = 0; // При оплате картой сдача не нужна
            DialogResult = true;
            Close();
        }

        // Внесение наличных
        private void InsertCash_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && decimal.TryParse(btn.Tag?.ToString(), out decimal amount))
            {
                _insertedCash += amount;
                InsertedText.Text = $"{_insertedCash} ₽";

                if (_insertedCash >= _finalPrice)
                {
                    Change = _insertedCash - _finalPrice;
                    ChangeText.Text = $"{Change} ₽";
                    ConfirmCashBtn.IsEnabled = true;
                }
                else
                {
                    ChangeText.Text = "0 ₽";
                    ConfirmCashBtn.IsEnabled = false;
                }
            }
        }

        // Подтверждение наличных
        private void ConfirmCash_Click(object sender, RoutedEventArgs e)
        {
            if (_insertedCash >= _finalPrice)
            {
                IsPaymentSuccessful = true;
                DialogResult = true;
                Close();
            }
        }

        // Отмена
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsPaymentSuccessful = false;
            DialogResult = false;
            Close();
        }
    }
}