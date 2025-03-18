using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.DataAccess;
using AutoserviceApp.Models;
using AutoserviceApp.ViewModels;
using AutoserviceApp.Views;

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
                Environment.Exit(0);
            }
        }
        private void ShowModels_Click(object sender, RoutedEventArgs e) => _viewModel.SwitchView("Модели");
        private void ShowCars_Click(object sender, RoutedEventArgs e) => _viewModel.SwitchView("Автомобили");
        private void ShowClients_Click(object sender, RoutedEventArgs e) => _viewModel.SwitchView("Клиенты");
        private void ShowDetails_Click(object sender, RoutedEventArgs e) => _viewModel.SwitchView("Детали");
        private void ShowMasters_Click(object sender, RoutedEventArgs e) => _viewModel.SwitchView("Мастера");
        private void ShowWorkTypes_Click(object sender, RoutedEventArgs e) => _viewModel.SwitchView("Виды работ");
        private void ShowOrders_Click(object sender, RoutedEventArgs e) => _viewModel.SwitchView("Заказы");
        private void ShowUsers_Click(object sender, RoutedEventArgs e) => _viewModel.SwitchView("Пользователи");

        public void ShowWorks_Switch(object sender, RoutedEventArgs e) => _viewModel.SwitchView("Работы");
        public void ShowWorkDetails_Switch(object sender, RoutedEventArgs e) => _viewModel.SwitchView("ДеталиРабот");
        public void ShowWorkComplaints_Switch(object sender, RoutedEventArgs e) => _viewModel.SwitchView("Жалобы");


        private readonly Dictionary<string, List<string>> roleTabs = new Dictionary<string, List<string>>
        {
            { "Сотрудник", new List<string> { "Модели", "Автомобили", "Клиенты", "Детали", "Мастера", "Виды работ", "Заказы" } },
            { "Администратор", new List<string> { "Модели", "Автомобили", "Клиенты", "Детали", "Мастера", "Виды работ", "Заказы", "Пользователи" } }
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

            ShowOrders_Click(this, null);
            ShowWelcomeScreen();
        }        
        private void ConfigWelcomeScreen()
        {
            string roleDescription = _currentUser.Role switch
            {
                "Сотрудник" => "Вы имеете полный доступ ко всем данным, за исключением Пользователей.",
                "Администратор" => "Вы имеете полный доступ ко всем данным, включая управление пользователями.",
                _ => "Неизвестная роль"
            };

            WelcomeText.Text = $"Добро пожаловать в приложение нашего Автосервиса!\n\n" +
                               $"Вы вошли как: {_currentUser.Role}.\n\n" +
                               $"Вам доступен следующий функционал:\n{roleDescription}";
        }
        private void ShowWelcomeScreen(object sender = null, RoutedEventArgs? e = null)
        {
            ConfigWelcomeScreen();
            MainContent.Visibility = Visibility.Collapsed;
            WelcomeScreen.Visibility = Visibility.Visible;
        }
        private void CloseWelcomeScreen_Click(object sender = null, RoutedEventArgs e = null)
        {
            WelcomeScreen.Visibility = Visibility.Collapsed;
            MainContent.Visibility = Visibility.Visible;
        }

        private void ShowAbout_Click(object sender, RoutedEventArgs e)
        {
            string helpText = "Программа сделана студентом группы ПО-31 - Дольников Александр";

            MessageBox.Show(helpText, "Справка", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void OpenReportsView_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SwitchView("Отчеты");
        }

        private void OpenChartsView_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SwitchView("Диаграммы");
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
                Environment.Exit(0);
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private void CloseApp_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Maximize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseWelcomeScreen_Click();
            }
        }


    }
}
