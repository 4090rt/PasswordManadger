using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PasswordMenedger.BusinesLogic;
using PasswordMenedger.BusinesLogic.LoggerFac;
using PasswordMenedger.Controllers_UI___BL;
using PasswordMenedger.Controllers_UI___BL.CreateDB;
using PasswordMenedger.Controllers_UI___BL.UpdatesControllers;
using PasswordMenedger.DataBase;
using PasswordMenedger.DataBase.PoolSQLiteConnection;
using PasswordMenedger.DataModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace PasswordMenedger
{
    public partial class MainWindow : Window
    {
        private bool _isPasswordsLoaded = false;

        private GeneratePassword _generatePassword;
        private GenerateRandomPassword _generateRandomPassword;
        private CreateTable _createtable;
        public ExportClass _exportclass;
        private DeleteAllPasswordsController _DeleteAllPasswordsController;
        private LoadedAllPasswordsController _LoadedAllPasswordsController;
        private DeleteConcrectPasswordController _DeleteConcrectPasswordController;
        private ItemsControl leftColumn;
        private ItemsControl rightColumn;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _memorycache;
        private readonly HttppwnedController _httppwnedController;
        private readonly ILogger<MainWindow> _logger = LogFac.LoggerCreate<MainWindow>();
        private ObservableCollection<string> LeftPasswords;
        private ObservableCollection<string> RightPasswords;
        private readonly UpdateUserIconController  _updateUserIconController;
        private readonly UpdateServiceName _updateServiceName;
        private readonly UpdateServiceUrl _updateServiceUrl;
        public MainWindow()
        {
            InitializeComponent();
            var serviceProvider = App.ServiceProvider;
            _httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            _memorycache = serviceProvider.GetRequiredService<IMemoryCache>();
            AutoCopyCheckBox.IsChecked = Properties.Settings.Default.CopyPassword;
            SaveHistoryCheckBox.IsChecked = Properties.Settings.Default.SaceHistory;

            _generateRandomPassword = new GenerateRandomPassword();
            _generatePassword = new GeneratePassword(_generateRandomPassword);
            _createtable = new CreateTable();
            _createtable.CreateTableMethod();
            _exportclass = new ExportClass();
            _DeleteAllPasswordsController = new DeleteAllPasswordsController();
            _LoadedAllPasswordsController = new LoadedAllPasswordsController();
            _DeleteConcrectPasswordController = new DeleteConcrectPasswordController();
            _httppwnedController = new HttppwnedController(_httpClientFactory, _memorycache);
            _updateUserIconController = new UpdateUserIconController();
            _updateServiceName = new UpdateServiceName();   
            _updateServiceUrl = new UpdateServiceUrl();
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            Properties.Settings.Default.CopyPassword = AutoCopyCheckBox.IsChecked ?? false;
            Properties.Settings.Default.SaceHistory = SaveHistoryCheckBox.IsChecked ?? false;
            Properties.Settings.Default.Save();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            string text = PasswordTextBox.Text;
            if (text.Length >= 8)
            {
                Clipboard.SetData(DataFormats.Text, text);
                MessageBox.Show("Пароль скопирован в буфер обмена!");
            }
            else
            {
                MessageBox.Show("Пароль пуст - такое мы не копируем даже в буфер");
            }
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            PasswordTextBox.Clear();
            double Lenght = LengthSlider.Value;
            bool? RegisterUp = UppercaseCheckBox?.IsChecked;
            bool? RegisterDown = LowercaseCheckBox?.IsChecked;
            bool? Number = NumbersCheckBox?.IsChecked;
            bool? SpecSymbols = SymbolsCheckBox?.IsChecked;
            bool? ExceptionSymbols = ExcludeSimilarCheckBox?.IsChecked;

            int length = (int)LengthSlider.Value;
            bool hasUpper = UppercaseCheckBox.IsChecked == true;
            bool hasLower = LowercaseCheckBox.IsChecked == true;
            bool hasNumbers = NumbersCheckBox.IsChecked == true;
            bool hasSymbols = SymbolsCheckBox.IsChecked == true;

            if (length >= 15 && hasUpper && hasLower && hasNumbers && hasSymbols)
            {
                StrengthText.Text = "Сильный";
                StrengthBars.Text = "●●●●●";
                CrackTimeText.Text = "Время взлома: ~100 лет";
            }
            else if (length >= 12 && ((hasUpper && hasLower && hasNumbers) || (hasUpper && hasLower && hasSymbols)))
            {
                StrengthText.Text = "Нормальный";
                StrengthBars.Text = "●●●";
                CrackTimeText.Text = "Время взлома: ~1 год";
            }
            else if (length >= 8 && (hasUpper || hasLower) && (hasNumbers || hasSymbols))
            {
                StrengthText.Text = "Слабый";
                StrengthBars.Text = "●";
                CrackTimeText.Text = "Время взлома: ~2 часа";
            }
            else
            {
                StrengthText.Text = "Очень слабый";
                StrengthBars.Text = "○";
                CrackTimeText.Text = "Время взлома: ~ 5 минут";
            }

            string sb = _generatePassword.CallGenereate(Lenght, RegisterUp, RegisterDown, Number, SpecSymbols, ExceptionSymbols);

            PasswordTextBox.Text = sb;

            if (AutoCopyCheckBox.IsChecked == true)
            { 
                Clipboard.SetText(sb);
                MessageBox.Show("Пароль скопирован!");
            }
        }

        private async void ClearAllPasswords_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _DeleteAllPasswordsController.DeletePasswordsController();
                PasswordsPanel.Children.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Возникло исключение при попытке очистить пароли" + ex.Message);
                return;
            }
        }

        private async void ExportPasswords_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _exportclass.ExportFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Возникло исключение при экпорта паролей" + ex.Message);
                return;
            }
        }


        private async void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTab = MainTabControl.SelectedItem as TabItem;

            if (selectedTab == mypasswords) 
            {
                if (!_isPasswordsLoaded)
                {
                    await LoadedPasswordList(); 
                    _isPasswordsLoaded = true;
                }
            }
        }

        public async Task LoadedPasswordList()
        {
            try
            {
                List<SavePasswordModel> list = await _LoadedAllPasswordsController.RequestAllPasswords().ConfigureAwait(false);
                Dispatcher.Invoke(() =>
                {
                    if (list != null && list.Count > 0)
                    {
                        PasswordsPanel.Children.Clear();
                        foreach (var item in list)
                        {
                                var card = CreatePasswordCard(item);
                                PasswordsPanel.Children.Add(card);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Пароли не найдены" + list.Count);
                    }
                });
            }
            catch(Exception ex)
            {
                MessageBox.Show("Возникло исключение при получении списка паролей" + ex.Message);
            }
        }
        private byte[] GenerateDefaultIcon(string serviceName)
        {
            var firstLetter = serviceName.FirstOrDefault().ToString().ToUpper();

            var drawingVisual = new DrawingVisual();
            using (var dc = drawingVisual.RenderOpen())
            {
                dc.DrawEllipse(Brushes.Gray, null, new Point(16, 16), 16, 16);
                var text = new FormattedText(firstLetter, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    new Typeface("Arial"), 16, Brushes.White, 1);
                dc.DrawText(text, new Point(16 - text.Width / 2, 16 - text.Height / 2));
            }
            var bitmap = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawingVisual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using var stream = new MemoryStream();
            encoder.Save(stream);
            return stream.ToArray();
        }

        private Border CreatePasswordCard(SavePasswordModel list)
        {
            // Карточка
            var card = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8F9FA")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DEE2E6")),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 8),
                Padding = new Thickness(15, 10, 15, 10),
                CornerRadius = new CornerRadius(8)
            };

            // Сетка для размещения элементов в одной строке
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Для кнопки "+"
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Для иконки
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Для левой части
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Для правой части

            // Кнопка "+"
            var addButton = new Button
            {
                Content = "+",
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28A745")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Width = 30,
                Height = 30,
                Margin = new Thickness(0, 0, 8, 0),
                Cursor = Cursors.Hand,
                Tag = list,
                FontSize = 18,
                FontWeight = FontWeights.Bold
            };
            addButton.Click += async (s, e) =>
            {
                var button = s as Button;
                var entry = button?.Tag as SavePasswordModel;
                await _updateUserIconController.UpdateController(list.Id);
                MessageBox.Show($"Добавить для '{entry?.Name}'");
            };

            // Иконка
            var icon = new System.Windows.Controls.Image
            {
                Width = 32,
                Height = 32,
                Margin = new Thickness(0, 0, 10, 0),
                Stretch = Stretch.Uniform
            };

            if (list.IconUser != null && list.IconUser.Length > 0)
            {
                var image = new BitmapImage();
                using var stream = new MemoryStream(list.IconUser);
                image.BeginInit();
                image.StreamSource = stream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                icon.Source = image;
            }
            else if (list.IconUser == null && list.Icon != null && list.Icon.Length > 0)
            {
                var image = new BitmapImage();
                using var stream = new MemoryStream(list.Icon);
                image.BeginInit();
                image.StreamSource = stream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                icon.Source = image;
            }
            else
            {
                byte[] bytes = GenerateDefaultIcon(list.Name);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                using var stream = new MemoryStream(bytes);
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                icon.Source = bitmap;
            }

            // Левая часть (Имя, Ссылка, Пароль, Дата)
            var leftPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Контейнер для имени с кнопкой редактирования
            var nameContainer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 15, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            // 1. Имя
            var nameText = new TextBlock
            {
                Text = list.Name,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center
            };
            nameContainer.Children.Add(nameText);

            // Кнопка редактирования имени (маленькая)
            var editNameButton = new Button
            {
                Content = "✏️",
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Width = 20,
                Height = 20,
                Margin = new Thickness(5, 0, 0, 0),
                Cursor = Cursors.Hand,
                FontSize = 12,
                Tag = list,
                ToolTip = "Редактировать имя"
            };
            editNameButton.Click += async (s, e) =>
            {
                var button = s as Button;
                var entry = button?.Tag as SavePasswordModel;
                if (entry != null)
                {
                    var inputDialog = new InputDialog("Редактирование имени", "Введите новое имя:", entry.Name);
                    if (inputDialog.ShowDialog() == true)
                    {
                        string newname = entry.Name = inputDialog.InputText;
                        nameText.Text = newname;
                        await _updateServiceName.UpdateName(entry.Id, newname);
                    }
                }
            };
            nameContainer.Children.Add(editNameButton);

            leftPanel.Children.Add(nameContainer);

            // Контейнер для ссылки с кнопкой редактирования
            var urlContainer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 15, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            // 2. Ссылка (обрезанная)
            var displayUrl = GetShortenedUrl(list.URL);
            var hyperlink = new Hyperlink
            {
                NavigateUri = new Uri(list.URL),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007BFF"))
            };
            hyperlink.Inlines.Add(displayUrl);
            hyperlink.RequestNavigate += (s, e) =>
                System.Diagnostics.Process.Start(e.Uri.ToString());

            var linkTextBlock = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            linkTextBlock.Inlines.Add(hyperlink);
            urlContainer.Children.Add(linkTextBlock);

            // Кнопка редактирования URL (маленькая)
            var editUrlButton = new Button
            {
                Content = "✏️",
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Width = 20,
                Height = 20,
                Margin = new Thickness(5, 0, 0, 0),
                Cursor = Cursors.Hand,
                FontSize = 12,
                Tag = list,
                ToolTip = "Редактировать URL"
            };
            editUrlButton.Click += async (s, e) =>
            {
                var button = s as Button;
                var entry = button?.Tag as SavePasswordModel;
                if (entry != null)
                {
                    var inputDialog = new InputDialog("Редактирование URL", "Введите новый URL:", entry.URL);
                    if (inputDialog.ShowDialog() == true)
                    {
                        string url = entry.URL = inputDialog.InputText;
                        // Обновляем отображение ссылки
                        var newDisplayUrl = GetShortenedUrl(url);
                        linkTextBlock.Inlines.Clear();
                        var newHyperlink = new Hyperlink
                        {
                            NavigateUri = new Uri(url),
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007BFF"))
                        };
                        newHyperlink.Inlines.Add(newDisplayUrl);
                        newHyperlink.RequestNavigate += (s2, e2) =>
                            System.Diagnostics.Process.Start(e2.Uri.ToString());
                        linkTextBlock.Inlines.Add(newHyperlink);
                        await _updateServiceUrl.UpdateUrl(entry.Id, url);
                    }
                }
            };
            urlContainer.Children.Add(editUrlButton);

            leftPanel.Children.Add(urlContainer);

            // 3. Пароль
            var passwordText = new TextBlock
            {
                Text = list.Password,
                FontSize = 13,
                Margin = new Thickness(0, 0, 15, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#495057"))
            };
            leftPanel.Children.Add(passwordText);

            // 4. Дата
            var dateText = new TextBlock
            {
                Text = list.Date,
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6C757D"))
            };
            leftPanel.Children.Add(dateText);

            // Правая часть (Кнопки)
            var rightPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Кнопка "Копировать"
            var copyButton = new Button
            {
                Content = "📋 Копировать",
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28A745")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 0, 8, 0),
                Cursor = Cursors.Hand,
                Tag = list
            };

            copyButton.Click += (s, e) =>
            {
                var button = s as Button;
                var entry = button?.Tag as SavePasswordModel;

                if (entry != null)
                {
                    Clipboard.SetText(entry.Password);
                    MessageBox.Show($"Пароль для '{entry.Name}' скопирован!",
                        "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            };
            rightPanel.Children.Add(copyButton);

            // Кнопка "Удалить"
            var deleteButton = new Button
            {
                Content = "🗑 Удалить",
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC3545")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(12, 6, 12, 6),
                Cursor = Cursors.Hand,
                Tag = list
            };

            deleteButton.Click += async (s, e) =>
            {
                var button = s as Button;
                var entry = button?.Tag as SavePasswordModel;

                if (entry != null)
                {
                    var result = MessageBox.Show($"Удалить пароль для '{entry.Name}'?",
                        "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        await _DeleteConcrectPasswordController.PasswordDeleteMethod(list.Id);
                    }
                }
            };
            rightPanel.Children.Add(deleteButton);

            // Добавляем элементы в сетку
            grid.Children.Add(addButton);
            Grid.SetColumn(addButton, 0);

            grid.Children.Add(icon);
            Grid.SetColumn(icon, 1);

            grid.Children.Add(leftPanel);
            Grid.SetColumn(leftPanel, 2);

            grid.Children.Add(rightPanel);
            Grid.SetColumn(rightPanel, 3);

            card.Child = grid;
            return card;
        }

        // Вспомогательный класс для диалога ввода
        public class InputDialog : Window
        {
            private TextBox textBox;
            public string InputText { get; private set; }

            public InputDialog(string title, string prompt, string defaultValue = "")
            {
                Title = title;
                Width = 400;
                Height = 150;
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                ResizeMode = ResizeMode.NoResize;

                var stackPanel = new StackPanel { Margin = new Thickness(20) };

                var label = new TextBlock
                {
                    Text = prompt,
                    Margin = new Thickness(0, 0, 0, 10),
                    FontSize = 14
                };
                stackPanel.Children.Add(label);

                textBox = new TextBox
                {
                    Text = defaultValue,
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 0, 15)
                };
                stackPanel.Children.Add(textBox);

                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                var okButton = new Button
                {
                    Content = "OK",
                    Width = 80,
                    Height = 30,
                    Margin = new Thickness(0, 0, 10, 0),
                    IsDefault = true
                };
                okButton.Click += (s, e) =>
                {
                    InputText = textBox.Text;
                    DialogResult = true;
                    Close();
                };

                var cancelButton = new Button
                {
                    Content = "Отмена",
                    Width = 80,
                    Height = 30,
                    IsCancel = true
                };
                cancelButton.Click += (s, e) =>
                {
                    DialogResult = false;
                    Close();
                };

                buttonPanel.Children.Add(okButton);
                buttonPanel.Children.Add(cancelButton);
                stackPanel.Children.Add(buttonPanel);

                Content = stackPanel;
            }
        }
        // Вспомогательный метод для обрезания URL
        private string GetShortenedUrl(string url, int maxLength = 30)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            // Убираем протокол для отображения
            string displayUrl = url;
            if (displayUrl.StartsWith("https://"))
                displayUrl = displayUrl.Substring(8);
            else if (displayUrl.StartsWith("http://"))
                displayUrl = displayUrl.Substring(7);
            else if (displayUrl.StartsWith("www."))
                displayUrl = displayUrl.Substring(4);

            // Обрезаем до максимальной длины
            if (displayUrl.Length > maxLength)
            {
                displayUrl = displayUrl.Substring(0, maxLength) + "...";
            }

            return displayUrl;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveHistoryCheckBox.IsChecked == true)
            {
                if (PasswordTextBox.Text.Length >= 8)
                {
                    _isPasswordsLoaded = false;
                    MainFrame.Navigate(new SavePassword(PasswordTextBox.Text));
                }
                else
                {
                    MessageBox.Show("Нечего сохранять");

                }
            }
            else
            {
                MessageBox.Show("Чтобы сохранить пароль, включите функцию сохранения в натсройках!");
            }
        }

        private async void ImportPasswords_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               await _exportclass.ImportFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Возникло исключение при импорте" + ex.Message);
                return;
            }
        }

        private void AutoCopyCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private async void CheckPWNED_Click(object sender, RoutedEventArgs e)
        {
            LeftPasswords = new ObservableCollection<string>();
            RightPasswords = new ObservableCollection<string>();
            List<CheckPassword> check = new List<CheckPassword>();
            bool cheked = false;
            try
            {
                string Pass = PasswordTextBoxCheck.Text;

                using var Sha_1 = SHA1.Create();
                byte[] bytepass = Encoding.UTF8.GetBytes(Pass);
                var hashe = Sha_1.ComputeHash(bytepass);
                var fullhash = BitConverter.ToString(hashe).Replace("-", "").ToUpperInvariant();
                PasswordHashTextBlock.Text = fullhash;

                List<CacheTestPassword> list = await _httppwnedController.RequestpwnedAndReserve(Pass).ConfigureAwait(false);
                if (list != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        foreach (var item1 in list)
                        {
                            check = item1.checkPasswordsList;
                            cheked = item1.checkPasswordEnabled;
                        }
                        if (cheked == true)
                        {
                            PasswordHashTextBlockEnbled.Text = "Пароль найден в слитых базах.\n Перегенерируйте его";
                        }
                        else
                        {
                            PasswordHashTextBlockEnbled.Text = "Пароль не найден в слитых базах,\n можете его использовать";
                        }
                        foreach (var item in check)
                        {
                            LeftPasswords.Add(item.PasswordHash);
                            RightPasswords.Add($"{item.Count}");
                        }
                        if (LeftPasswords.Count > 0 && LeftPasswords != null && RightPasswords.Count > 0 && RightPasswords != null)
                        {
                            leftColumn = FindName("LeftPasswordsColumn") as ItemsControl;
                            rightColumn = FindName("RightPasswordsColumn") as ItemsControl;

                            if (leftColumn != null && rightColumn != null)
                            {
                                leftColumn.ItemsSource = LeftPasswords;
                                rightColumn.ItemsSource = RightPasswords;
                            }
                        }
                        else
                        {
                            _logger.LogError("Не удалось заполнить дочерние листый!");
                            MessageBox.Show("Не удалось найти данный пароль в базах");
                            return;
                        }
                    });
                }
                else
                {
                    _logger.LogWarning("Список паролей от PWNED пуст!");
                    MessageBox.Show("Список паролей от PWNED пуст!");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возикло исключение при получинеии паролей от PWNED" + ex.Message);
                MessageBox.Show($"Error: {ex.Message}");
                return;
            }
        }

        private void CheckPWNEDV_Click(object sender, RoutedEventArgs e)
        {
            string password = Clipboard.GetData(DataFormats.Text) as string;
            if (password != null)
            {
                PasswordTextBoxCheck.Text = password;
            }
        }
    }
}