using System.Windows;
using System.Windows.Input;
using PWlaunchertest.Models;
using PWlaunchertest.Services;
using Forms = System.Windows.Forms;

namespace PWlaunchertest
{
    public partial class MainWindow : Window
    {
        private LauncherConfig _config = new LauncherConfig();
        private string _resolvedGamePath = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            InitializeLauncher();
        }

        private void InitializeLauncher()
        {
            _config = ConfigService.Load();

            string? foundPath = GameLocatorService.FindInstalledGame(_config.InstallPath);

            if (!string.IsNullOrWhiteSpace(foundPath))
            {
                _resolvedGamePath = foundPath;
                _config.InstallPath = foundPath;
                ConfigService.Save(_config);

                GameStatusText.Text = "Игра: найдена";
                UpdateStatusText.Text = "Обновления: клиент обнаружен";
                InstallStatusText.Text = "Установка: повторная установка не требуется";
                StatusText.Text = "Статус: игра найдена";
                LauncherProgressBar.Value = 100;
            }
            else
            {
                _resolvedGamePath = string.Empty;

                GameStatusText.Text = "Игра: не найдена";
                UpdateStatusText.Text = "Обновления: клиент пока не обнаружен";
                InstallStatusText.Text = "Установка: укажи папку вручную или установи игру";
                StatusText.Text = "Статус: игра не найдена";
                LauncherProgressBar.Value = 0;
            }
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_resolvedGamePath))
            {
                StatusText.Text = "Статус: нельзя запустить, игра не найдена";
                LauncherProgressBar.Value = 0;
                return;
            }

            StatusText.Text = "Статус: игра найдена, запуск подключим следующим этапом";
            LauncherProgressBar.Value = 100;
        }

        private void ChooseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using Forms.FolderBrowserDialog dialog = new Forms.FolderBrowserDialog();

            dialog.Description = "Выберите папку, где расположена игра Prime World Classic";
            dialog.UseDescriptionForTitle = true;
            dialog.ShowNewFolderButton = false;

            if (!string.IsNullOrWhiteSpace(_resolvedGamePath))
            {
                dialog.InitialDirectory = _resolvedGamePath;
            }

            Forms.DialogResult result = dialog.ShowDialog();

            if (result != Forms.DialogResult.OK || string.IsNullOrWhiteSpace(dialog.SelectedPath))
                return;

            string? resolvedPath = GameLocatorService.ResolveGamePath(dialog.SelectedPath);

            if (string.IsNullOrWhiteSpace(resolvedPath))
            {
                GameStatusText.Text = "Игра: не найдена";
                UpdateStatusText.Text = "Обновления: клиент не обнаружен";
                InstallStatusText.Text = "Установка: выбранная папка не подходит";
                StatusText.Text = "Статус: в выбранной папке игра не найдена";
                LauncherProgressBar.Value = 0;

                System.Windows.MessageBox.Show(
                    "В выбранной папке не найден корректный клиент Prime World Classic.\n\n" +
                    "Можно указывать как саму папку игры, так и корневую Steam-папку игры, если внутри есть Game или Launcher.",
                    "Игра не найдена",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            _resolvedGamePath = resolvedPath;
            _config.InstallPath = resolvedPath;
            ConfigService.Save(_config);

            GameStatusText.Text = "Игра: найдена";
            UpdateStatusText.Text = "Обновления: клиент обнаружен";
            InstallStatusText.Text = "Установка: путь сохранен";
            StatusText.Text = "Статус: путь к игре успешно сохранен";
            LauncherProgressBar.Value = 100;

            System.Windows.MessageBox.Show(
                $"Папка игры успешно определена:\n{resolvedPath}",
                "Путь сохранен",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_resolvedGamePath))
            {
                StatusText.Text = "Статус: клиент не найден, установка будет доступна следующим этапом";
                LauncherProgressBar.Value = 10;
                return;
            }

            StatusText.Text = "Статус: игра уже найдена, установка не требуется";
            LauncherProgressBar.Value = 100;
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_resolvedGamePath))
            {
                StatusText.Text = "Статус: обновление недоступно, игра не найдена";
                LauncherProgressBar.Value = 0;
                return;
            }

            StatusText.Text = "Статус: игра найдена, модуль обновления будет добавлен следующим этапом";
            LauncherProgressBar.Value = 30;
        }

        private void CheckFilesButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_resolvedGamePath))
            {
                StatusText.Text = "Статус: проверка невозможна, игра не найдена";
                LauncherProgressBar.Value = 0;
                return;
            }

            StatusText.Text = "Статус: проверка файлов будет подключена следующим этапом";
            LauncherProgressBar.Value = 45;
        }
    }
}