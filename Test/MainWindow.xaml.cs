using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Test {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        /// <summary>Имя папки с новыми версиями.</summary>
        private string _updateFolderName = "new";

        /// <summary>Сообщение об отсутствии обновлений.</summary>
        private string _notUpdatesMessage = "Нет доступных обновлений.";

        /// <summary>Подтверждение отката на предыдущую версию.</summary>
        /// <param name="currentVersion">Текущая версия</param>
        /// <param name="oldVersion">Старая версия</param>
        /// <returns>Запрос подтвержения.</returns>
        private string ToOldConfirmMessage(string currentVersion, string oldVersion) {
            return "Вы точно хотите изменить текущую версию " + currentVersion + " на предыдущую версию " + oldVersion + "?";
        }

        /// <summary>Путь к файлу выполняемой сборки.</summary>
        private string _executingPath = Assembly.GetExecutingAssembly().Location;

        /// <summary>Имя исполняемого файла.</summary>
        private string _mainFileName { get; set; }

        /// <summary>Возвращает строковое представление версии указанного файла.</summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <returns>Строка версии укаанного файла.</returns>
        private string GetFileVersion(string filePath) {
            return FileVersionInfo.GetVersionInfo(filePath).FileVersion;
        }

        public MainWindow() {
            InitializeComponent();
            _mainFileName = Path.GetFileName(_executingPath);
            string fileVersion = GetFileVersion(_executingPath);
            Title = _mainFileName + " " + fileVersion;
            MessageText.Text = "Текущая версия файла: " + fileVersion;
            Activate();
        }

        /// <summary>Обработка события нажатия кнопки UpdateButton.</summary>
        private void UpdateButton_Click(object sender, RoutedEventArgs e) {
            if(!Directory.Exists(_updateFolderName)) {
                Directory.CreateDirectory(_updateFolderName);
            }

            var srcFilePath = Path.Combine(_updateFolderName, _mainFileName);
            if(!File.Exists(srcFilePath)) {
                MessageText.Text = _notUpdatesMessage;
                return;
            }

            string curVersion = GetFileVersion(_executingPath);
            var vCurent = new Version(curVersion);
            string srcVersion = GetFileVersion(srcFilePath);
            var vSource = new Version(srcVersion);

            if(vSource == vCurent) {
                MessageText.Text = _notUpdatesMessage;
                return;
            }
            if(vSource < vCurent) {
                string confirmOld = ToOldConfirmMessage(curVersion, srcVersion);
                MessageBoxResult result = MessageBox.Show(confirmOld, "Внимание! Старая версия!", MessageBoxButton.YesNo);
                if(result == MessageBoxResult.No) return;
            }

            File.Copy(srcFilePath, srcFilePath + ".new");
            File.Replace(srcFilePath, _executingPath, _mainFileName + ".old");
            var oldProcess = Process.GetCurrentProcess();
            oldProcess.MainModule.Dispose();
            Process.Start(_mainFileName);
            oldProcess.Kill();
        }

    }
}
