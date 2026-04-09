using System.Diagnostics;
using System.IO;
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
        private string _gameExePath = string.Empty;
        private string _installPath = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            InitializeLauncher();
        }

        private void InitializeLauncher()
        {
            _config = ConfigService.Load();

            _installPath = _config.InstallPath;

            string? foundExe = GameLocatorService.FindInstalledGameExe(_config.GameExePath);

            if (!string.IsNullOrWhiteSpace(foundExe))
            {
                _gameExePath = foundExe;
                _config.GameExePath = foundExe;
                ConfigService.Save(_config);

                GameStatusText.Text = "Игра: найдена";
                UpdateStatusText.Text = "Обновления: клиент обнаружен";
                InstallStatusText.Text = string.IsNullOrWhiteSpace(_installPath)
                    ? "Установка: путь установки пока не выбран"
                    : "Установка: путь установки сохранен";
                StatusText.Text = "Статус: игра найдена";
                LauncherProgressBar.Value = 100;
            }
            else
            {
                _gameExePath = string.Empty;

                GameStatusText.Text = "Игра: не найдена";
                UpdateStatusText.Text = "Обновления: клиент пока не обнаружен";
                InstallStatusText.Text = string.IsNullOrWhiteSpace(_installPath)
                    ? "Установка: выбери путь установки"
                    : "Установка: путь установки сохранен, игра не установлена";
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
            if (string.IsNullOrWhiteSpace(_gameExePath) || !File.Exists(_gameExePath))
            {
                StatusText.Text = "Статус: запуск невозможен, игра не найдена";
                LauncherProgressBar.Value = 0;

                System.Windows.MessageBox.Show(
                    "Игра не найдена.\n\nУкажи PWClassic.exe или установи игру.",
                    "Игра не найдена",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = _gameExePath,
                    WorkingDirectory = Path.GetDirectoryName(_gameExePath),
                    UseShellExecute = true
                });

                StatusText.Text = "Статус: запуск игры";
                LauncherProgressBar.Value = 100;
            }
            catch
            {
                System.Windows.MessageBox.Show(
                    "Не удалось запустить PWClassic.exe",
                    "Ошибка запуска",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ChooseGameExeButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Выберите PWClassic.exe",
                Filter = "Prime World Classic executable|PWClassic.exe|Executable files (*.exe)|*.exe",
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false
            };

            if (!string.IsNullOrWhiteSpace(_gameExePath) && File.Exists(_gameExePath))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(_gameExePath);
                dialog.FileName = Path.GetFileName(_gameExePath);
            }

            bool? result = dialog.ShowDialog();

            if (result != true || string.IsNullOrWhiteSpace(dialog.FileName))
                return;

            string? resolvedExe = GameLocatorService.ResolveGameExePath(dialog.FileName);

            if (string.IsNullOrWhiteSpace(resolvedExe))
            {
                GameStatusText.Text = "Игра: не найдена";
                UpdateStatusText.Text = "Обновления: клиент не обнаружен";
                InstallStatusText.Text = "Установка: выбранный exe не подходит";
                StatusText.Text = "Статус: выбран неправильный exe";
                LauncherProgressBar.Value = 0;

                System.Windows.MessageBox.Show(
                    "Выбранный файл не является корректным PWClassic.exe клиента Prime World Classic.",
                    "Неверный exe",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            _gameExePath = resolvedExe;
            _config.GameExePath = resolvedExe;

            if (string.IsNullOrWhiteSpace(_config.InstallPath))
            {
                string? parent = Directory.GetParent(Path.GetDirectoryName(resolvedExe) ?? string.Empty)?.FullName;
                if (!string.IsNullOrWhiteSpace(parent))
                {
                    _installPath = parent;
                    _config.InstallPath = parent;
                }
            }

            ConfigService.Save(_config);

            GameStatusText.Text = "Игра: найдена";
            UpdateStatusText.Text = "Обновления: клиент обнаружен";
            InstallStatusText.Text = string.IsNullOrWhiteSpace(_installPath)
                ? "Установка: путь установки пока не выбран"
                : "Установка: путь сохранен";
            StatusText.Text = "Статус: PWClassic.exe успешно определен";
            LauncherProgressBar.Value = 100;

            System.Windows.MessageBox.Show(
                $"Исполняемый файл игры найден и сохранен:\n{resolvedExe}",
                "Путь сохранен",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ChooseInstallPathButton_Click(object sender, RoutedEventArgs e)
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

            if (result != Forms.DialogResult.OK || string.IsNullOrWhiteSpace(dialog.SelectedPath))
                return;

            string selectedPath = InstallPathService.NormalizeInstallPath(dialog.SelectedPath);
            InstallPathCheckResult checkResult = InstallPathService.CheckInstallPath(selectedPath);

            switch (checkResult)
            {
                case InstallPathCheckResult.EmptyFolder:
                    _installPath = selectedPath;
                    _config.InstallPath = selectedPath;
                    ConfigService.Save(_config);

                    InstallStatusText.Text = "Установка: выбрана пустая папка для установки";
                    StatusText.Text = "Статус: путь установки сохранен";
                    LauncherProgressBar.Value = 20;

                    System.Windows.MessageBox.Show(
                        $"Путь установки сохранен:\n{selectedPath}",
                        "Путь установки",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    break;

                case InstallPathCheckResult.ExistingGameFolder:
                    _installPath = selectedPath;
                    _config.InstallPath = selectedPath;

                    string? resolvedExe = GameLocatorService.ResolveGameExePath(selectedPath);
                    if (!string.IsNullOrWhiteSpace(resolvedExe))
                    {
                        _gameExePath = resolvedExe;
                        _config.GameExePath = resolvedExe;
                        GameStatusText.Text = "Игра: найдена";
                        UpdateStatusText.Text = "Обновления: клиент обнаружен";
                    }

                    ConfigService.Save(_config);

                    InstallStatusText.Text = "Установка: выбрана папка с уже существующей игрой";
                    StatusText.Text = "Статус: путь установки сохранен";
                    LauncherProgressBar.Value = 80;

                    System.Windows.MessageBox.Show(
                        $"В выбранной папке уже обнаружена игра.\nПуть установки сохранен:\n{selectedPath}",
                        "Игра уже существует",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    break;

                case InstallPathCheckResult.NonEmptyUnknownFolder:
                    MessageBoxResult choice = System.Windows.MessageBox.Show(
                        "Папка не пустая, и корректная установленная игра в ней не обнаружена.\n\n" +
                        "Использовать эту папку для установки всё равно?",
                        "Папка не пустая",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (choice == MessageBoxResult.Yes)
                    {
                        _installPath = selectedPath;
                        _config.InstallPath = selectedPath;
                        ConfigService.Save(_config);

                        InstallStatusText.Text = "Установка: выбрана непустая папка";
                        StatusText.Text = "Статус: путь установки сохранен с предупреждением";
                        LauncherProgressBar.Value = 15;
                    }
                    break;

                default:
                    System.Windows.MessageBox.Show(
                        "Не удалось использовать выбранную папку.",
                        "Ошибка пути",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    break;
            }
        }

        private async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_installPath))
            {
                StatusText.Text = "Статус: сначала выбери путь установки";
                LauncherProgressBar.Value = 0;

                System.Windows.MessageBox.Show(
                    "Сначала выбери путь установки игры.",
                    "Путь установки не выбран",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (!string.IsNullOrWhiteSpace(_gameExePath) && File.Exists(_gameExePath))
            {
                StatusText.Text = "Статус: игра уже найдена, установка не требуется";
                LauncherProgressBar.Value = 100;
                return;
            }

            try
            {
                IsEnabled = false;
                StatusText.Text = "Статус: загрузка manifest установки";
                LauncherProgressBar.Value = 0;

                BaseInstallManifest manifest =
                    await ManifestService.LoadBaseInstallManifestAsync(_config.BaseInstallManifestPath);

                InstallService installService = new InstallService();

                Progress<string> statusProgress = new Progress<string>(status =>
                {
                    StatusText.Text = $"Статус: {status}";
                });

                Progress<double> overallProgress = new Progress<double>(percent =>
                {
                    LauncherProgressBar.Value = percent;
                });

                string installedExePath = await installService.InstallBaseGameAsync(
                    _installPath,
                    manifest,
                    statusProgress,
                    overallProgress);

                _gameExePath = installedExePath;
                _config.GameExePath = installedExePath;
                _config.InstallPath = _installPath;
                ConfigService.Save(_config);

                GameStatusText.Text = "Игра: установлена";
                UpdateStatusText.Text = "Обновления: базовая версия установлена";
                InstallStatusText.Text = "Установка: завершена";
                StatusText.Text = "Статус: установлено";
                LauncherProgressBar.Value = 100;

                System.Windows.MessageBox.Show(
                    "Базовая установка игры завершена успешно.",
                    "Установка завершена",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                StatusText.Text = "Статус: ошибка установки";
                LauncherProgressBar.Value = 0;

                System.Windows.MessageBox.Show(
                    $"Не удалось установить игру:\n{ex.Message}",
                    "Ошибка установки",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_gameExePath))
            {
                StatusText.Text = "Статус: обновление недоступно, игра не найдена";
                LauncherProgressBar.Value = 0;
                return;
            }

            StatusText.Text = "Статус: игра найдена, модуль обновления будет подключен следующим этапом";
            LauncherProgressBar.Value = 35;
        }

        private void CheckFilesButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_gameExePath))
            {
                StatusText.Text = "Статус: проверка невозможна, игра не найдена";
                LauncherProgressBar.Value = 0;
                return;
            }

            StatusText.Text = "Статус: проверка файлов будет подключена следующим этапом";
            LauncherProgressBar.Value = 50;
        }
    }
}