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

using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.DataAccess;
using AutoserviceApp.Models;
using System.Data.SqlClient;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using AutoserviceApp.ViewModels;

namespace AutoserviceApp
{
    public partial class MainWindow : Window
    {
        private readonly DetailRepository _detailRepository;
        private readonly WorkDetailRepository _workDetailRepository;
        private readonly WorkRepository _workRepository;
        private readonly ComplaintRepository _complaintRepository;
        private readonly UserRepository _userRepository;

        private User _currentUser;

        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            var loginWindow = new LoginWindow();

            if (loginWindow.ShowDialog() == true)
            {
                InitializeComponent();

                _viewModel = new MainViewModel();
                _viewModel.CurrentUser = loginWindow.CurrentUser;
                DataContext = _viewModel;

                _currentUser = loginWindow.CurrentUser;
                var context = new DatabaseContext();

                _detailRepository = new DetailRepository(context);
                _workDetailRepository = new WorkDetailRepository(context);
                _workRepository = new WorkRepository(context);
                _complaintRepository = new ComplaintRepository(context);
                _userRepository = new UserRepository(context);

                ApplyRoleRestrictions();
            }
            else
            {
                Application.Current.Shutdown();
            } 
        }

        private void ShowOrders_Click(object sender, RoutedEventArgs e) => _viewModel.SwitchView("Заказы");
        private void ShowWorks_Click(object sender, RoutedEventArgs e) => _viewModel.SwitchView("Работы");
        private void ShowComplaints_Click(object sender, RoutedEventArgs e) => _viewModel.SwitchView("Жалобы");
        private void ShowMasters_Click(object sender, RoutedEventArgs e) => _viewModel.SwitchView("Мастера");
        private void ShowDetails_Click(object sender, RoutedEventArgs e) => _viewModel.SwitchView("Детали");
        private void ShowUsers_Click(object sender, RoutedEventArgs e) => _viewModel.SwitchView("Пользователи");



        private readonly Dictionary<string, List<string>> roleTabs = new Dictionary<string, List<string>>
        {
            { "Сотрудник", new List<string> { "Клиенты", "Автомобили", "Модели", "Мастера", "Виды работ", "Детали", "Заказы" } },
            { "Администратор", new List<string> { "Клиенты", "Автомобили", "Модели", "Мастера", "Виды работ", "Детали", "Заказы", "Пользователи" } }
        };

        private void ApplyRoleRestrictions()
        {
            if (!roleTabs.ContainsKey(_currentUser.Role))
                return;

            List<string> allowedTabs = roleTabs[_currentUser.Role];

            // Очищаем меню (или скрываем кнопки, если они заранее созданы)
            foreach (var button in MenuPanel.Children.OfType<Button>())
            {
                button.Visibility = allowedTabs.Contains(button.Content.ToString()) ? Visibility.Visible : Visibility.Collapsed;
            }

            // Дополнительно скрываем кнопки редактирования мастеров для сотрудников
            //if (_currentUser.Role == "Сотрудник")
            //{
            //    ShowUsersButton.Visibility = Visibility.Collapsed; // Прячем кнопку "Пользователи"
            //}

            ShowWelcomeScreen(this, null);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private void ConfigWelcomeScreen()
        {
            string roleDescription = _currentUser.Role switch
            {
                //"Гость" => "Вы можете просматривать список мастеров и последние выполненные работы.",
                "Сотрудник" => "Вы имеете полный доступ ко всем данным, за исключением Мастеров и Пользователей.",
                "Администратор" => "Вы имеете полный доступ ко всем данным, включая управление пользователями.",
                _ => "Неизвестная роль"
            };

            WelcomeText.Text = $"Добро пожаловать в приложение нашего Автосервиса!\n\n" +
                               $"Вы вошли как: {_currentUser.Role}.\n\n" +
                               $"Вам доступен следующий функционал:\n{roleDescription}";
        }

        private void ShowWelcomeScreen(object sender, RoutedEventArgs? e)
        {
            ConfigWelcomeScreen();
            MainContent.Visibility = Visibility.Collapsed;
            WelcomeScreen.Visibility = Visibility.Visible;
        }

        private void CloseWelcomeScreen_Click(object sender, RoutedEventArgs e)
        {
            WelcomeScreen.Visibility = Visibility.Collapsed;
            MainContent.Visibility = Visibility.Visible;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Visibility = Visibility.Collapsed;
            WelcomeScreen.Visibility = Visibility.Hidden;

            var loginWindow = new LoginWindow();

            if (loginWindow.ShowDialog() == true)
            {
                _currentUser = loginWindow.CurrentUser;

                ApplyRoleRestrictions();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private void ShowAbout_Click(object sender, RoutedEventArgs e)
        {
            string helpText = "Программа сделана студентом группы ПО-31 - Дольников Александр";

            MessageBox.Show(helpText, "Справка", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void GenerateReport1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Отчет 1...", "Отчет");
        }
        private void GenerateReport2_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Отчет 2...", "Отчет");
        }
        private void GenerateReport3_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Отчет 3...", "Отчет");
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Maximize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }
}
