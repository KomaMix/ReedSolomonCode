using ReedSolomonCode.Services;
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

namespace ReedSolomonCode.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Инициализация видимости подсказок
            InputTextBox.GotFocus += (sender, args) => InputTextHint.Visibility = Visibility.Collapsed;
            InputTextBox.LostFocus += (sender, args) => { if (string.IsNullOrEmpty(InputTextBox.Text)) InputTextHint.Visibility = Visibility.Visible; };

            EncodedInputTextBox.GotFocus += (sender, args) => EncodedTextHint.Visibility = Visibility.Collapsed;
            EncodedInputTextBox.LostFocus += (sender, args) => { if (string.IsNullOrEmpty(EncodedInputTextBox.Text)) EncodedTextHint.Visibility = Visibility.Visible; };
        }

        // Обработчик для конвертации текста в байты
        private void ConvertToBinaryButton_Click(object sender, RoutedEventArgs e)
        {
            string input = InputTextBox.Text;

            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Введите текст для конвертации.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Преобразуем текст в байты
                byte[] byteArray = System.Text.Encoding.ASCII.GetBytes(input);

                BinaryOutputTextBox1.Text = string.Join(" ", byteArray);

                // Используем ReedSolomonCodeService для обработки
                ReedSolomonCodeService rsService = new ReedSolomonCodeService(4);
                byte[] result = rsService.Encode(byteArray);

                // Преобразуем в строку байтов
                string byteString = string.Join(" ", result);
                BinaryOutputTextBox2.Text = byteString;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка конвертации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчик для декодирования строки байтов
        private void DecodeButton_Click(object sender, RoutedEventArgs e)
        {
            string byteString = EncodedInputTextBox.Text;

            if (string.IsNullOrWhiteSpace(byteString))
            {
                MessageBox.Show("Введите строку байт для декодирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Преобразуем строку байт в массив байт
                byte[] byteArray = byteString.Split(' ').Select(byte.Parse).ToArray();

                // Используем ReedSolomonCodeService для декодирования
                ReedSolomonCodeService rsService = new ReedSolomonCodeService(4);
                byte[] decodedResult = rsService.Decode(byteArray);

                // Преобразуем байты обратно в текст
                string result = System.Text.Encoding.ASCII.GetString(decodedResult);
                DecodedOutputTextBox.Text = result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось получить исходное сообщение", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}