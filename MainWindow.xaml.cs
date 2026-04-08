using System.IO;
using System.Windows;
using Forms = System.Windows.Forms;

namespace PWlaunchertest
{
    public partial class MainWindow : Window
    {
        private string _installPath = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Min_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Steam_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show(
                "Steam пока не подключен.",
                "Steam",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_installPath))
            {
                System.Windows.MessageBox.Show(
                    "Сначала выбери папку установки.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            System.Windows.MessageBox.Show(
                $"Позже здесь будет запуск игры из папки:\n{_installPath}",
                "Играть",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            using Forms.FolderBrowserDialog dialog = new Forms.FolderBrowserDialog();

            dialog.Description = "Выберите папку для установки Prime World Classic";
            dialog.UseDescriptionForTitle = true;
            dialog.ShowNewFolderButton = true;

            if (!string.IsNullOrWhiteSpace(_installPath) && Directory.Exists(_installPath))
            {
                dialog.InitialDirectory = _installPath;
            }

            Forms.DialogResult result = dialog.ShowDialog();

            if (result == Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                _installPath = dialog.SelectedPath;
                InstallPathText.Text = _installPath;
                StatusText.Text = "Статус: папка установки выбрана";
            }
        }

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_installPath))
            {
                System.Windows.MessageBox.Show(
                    "Сначала выбери папку установки.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            System.Windows.MessageBox.Show(
                $"Позже сюда добавим установку игры в папку:\n{_installPath}",
                "Установка",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_installPath))
            {
                System.Windows.MessageBox.Show(
                    "Сначала выбери папку установки.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            System.Windows.MessageBox.Show(
                $"Позже сюда добавим обновление клиента в папке:\n{_installPath}",
                "Обновление",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Check_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_installPath))
            {
                System.Windows.MessageBox.Show(
                    "Сначала выбери папку установки.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            System.Windows.MessageBox.Show(
                $"Позже сюда добавим проверку файлов в папке:\n{_installPath}",
                "Проверка файлов",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}