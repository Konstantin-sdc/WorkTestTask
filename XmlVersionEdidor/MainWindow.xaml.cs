using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using Microsoft.Win32;

namespace XmlVersionEdidor {

    // UNDONE: Правила имени файла.
    //Имя файла имеет формат «XX_YY_ZZ.xml», где:
    //XX – набор русских букв.Количество символов - не более 100;
    //YY – набор цифр.Количество символов – либо 1, либо 10, либо от 14 до 20;
    //ZZ – любые символы.Количество символов – не более 7.


    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        /// <summary>Регулярное выражение для проверки соответствия формату.</summary>
        private readonly Regex _regex = new Regex("^([А-Яа-я]{2,5})_(\\d{1}|\\d{10}|\\d{14,20})_(.{1,7})$");

        /// <summary>Проверяет, соответствует ли значение текстового поля регулярному выражению. 
        /// Добавляет соответствующую всплывающую подсказку.
        /// </summary>
        /// <param name="textBox">Текстовое поле.</param>
        /// <returns></returns>
        private bool IsMatchRegular(TextBox textBox) {
            Match isMath = _regex.Match(textBox.Text);
            textBox.ToolTip = new ToolTip {
                Content = (isMath.Success) ? "Соответствует формату" : "Не соответствует формату."
            };
            return isMath.Success;
        }

        /// <summary>Каталог открытия и сохранения по умолчанию.</summary>
        private string _loadedDirectory { get; set; }

        /// <summary>Полное имя файла в обработке.</summary>
        private string _fileLocation { get; set; }

        /// <summary>XML-документ в обработке.</summary>
        private XmlVersionsDoc _document { get; set; }

        /// <summary>XML-документ с версиями.</summary>
        private class XmlVersionsDoc : XmlDocument {

            /// <summary>Узел "File".</summary>
            private XmlElement _fileNode {
                get {
                    XmlNodeList NodeList = GetElementsByTagName("File");
                    return (XmlElement)NodeList.Item(0);
                }
            }

            /// <summary>Значение "FileVersion".</summary>
            public string VersionValue {
                get {
                    return _fileNode.Attributes.GetNamedItem("FileVersion").Value;
                }
                set {
                    _fileNode.Attributes.GetNamedItem("FileVersion").Value = value;
                }
            }

            /// <summary>Значение "Name".</summary>
            public string XmlFileNameValue {
                get {
                    return _fileNode.GetElementsByTagName("Name").Item(0).InnerText;
                }
                set {
                    _fileNode.GetElementsByTagName("Name").Item(0).InnerText = value;
                }
            }

            /// <summary>Значение "DateTime".</summary>
            public string DateTimeValue {
                get {
                    return _fileNode.GetElementsByTagName("DateTime").Item(0).InnerText;
                }
                set {
                    _fileNode.GetElementsByTagName("DateTime").Item(0).InnerText = value;
                }
            }

        }

        /// <summary>Загрузка данных xml в текущий экземпляр.</summary>
        /// <param name="filePath">Полный путь к файлу xml — источнику данных</param>
        private void LoadXmlData(string filePath) {
            _loadedDirectory = Path.GetDirectoryName(filePath);
            FileNameNew.Text = FileNameCurrent.Text = Path.GetFileNameWithoutExtension(_fileLocation);
            var unused = IsMatchRegular(FileNameCurrent);
            var unused1 = IsMatchRegular(FileNameNew);
            _document = new XmlVersionsDoc();
            _document.Load(filePath);
            FileVersionNew.Text = FileVersionCurrent.Text = _document.VersionValue;
            XmlFileNameNew.Text = XmlFileNameCurrent.Text = _document.XmlFileNameValue;
            ChangeDateNew.Text = ChangeDateCurrent.Text = _document.DateTimeValue;
            Title = filePath;
        }

        public MainWindow() {
            InitializeComponent();
            Title = "Редактор атрибутов";
            SaveButton.Visibility = Visibility.Hidden;
        }

        /// <summary>Реакция на нажатие <see cref="OpenButton"/></summary>
        private void OpenButton_Click(object sender, RoutedEventArgs e) {
            var openDialog = new OpenFileDialog() {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "xml",
                Filter = "XML файлы(*.XML)|*.XML",
                Multiselect = false,
                ValidateNames = true,
            };
            bool? result = openDialog.ShowDialog(this);
            if(result == true) {
                _fileLocation = openDialog.FileName;
                LoadXmlData(_fileLocation);
                SaveButton.Visibility = Visibility.Visible;
            }
        }

        /// <summary>Реакция на нажатие <see cref="SaveButton"/></summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            _document.XmlFileNameValue = XmlFileNameNew.Text;
            _document.VersionValue = FileVersionNew.Text;
            var isSameNames = FileNameNew.Text.Equals(FileNameCurrent.Text, StringComparison.OrdinalIgnoreCase);

            if(!isSameNames) {

                var invalidChars = Path.GetInvalidFileNameChars();
                int indexOfInvalid = FileNameNew.Text.IndexOfAny(invalidChars);
                if(indexOfInvalid >= 0) {
                    string messageText = "В имени файла не должно быть символов: " + new string(invalidChars);
                    MessageBox.Show(this, messageText, "Недопустимое имя файла");
                    return;
                }

                string newLocation = Path.Combine(_loadedDirectory, FileNameNew.Text.Trim() + ".xml");
                if(File.Exists(newLocation)) {
                    MessageBoxResult result = MessageBox.Show(
                        this,
                        "Файл с таким именем уже существует. Перезаписать?",
                        "Файл уже существует",
                        MessageBoxButton.YesNo
                        );
                    if(result == MessageBoxResult.No) return;
                }
                _fileLocation = newLocation;

            }

            _document.Save(_fileLocation);
            LoadXmlData(_fileLocation);
        }

        /// <summary>Реакция на изменение <see cref="FileNameNew"/></summary>
        private void FileNameNew_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            var unused = IsMatchRegular(FileNameNew);
        }

    }
}
