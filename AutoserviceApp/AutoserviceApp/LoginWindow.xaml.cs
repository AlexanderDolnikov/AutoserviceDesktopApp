using System.Windows;
using System.Windows.Input;
using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.DataAccess;
using AutoserviceApp.Models;
using AutoserviceApp.Helpers;

namespace AutoserviceApp
{
    public partial class LoginWindow : Window
    {
        private readonly UserRepository _userRepository;
        public User CurrentUser { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            var context = new DatabaseContext();
            _userRepository = new UserRepository(context);
        }

        private void SetFocusOnFirstInput(object sender = null, RoutedEventArgs? e = null)
        {
            //ViewFocusHelper.SetFocusAndClearItemsValues(LoginInput, PasswordInput);
        }

        /* - - - - - */

        private void PasswordInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login_Click(sender, e);
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginInput.Text;
            string password = PasswordInput.Password;

            if (LoginInput.Text == "" || PasswordInput.Password == "" )
            {
                MessageBox.Show("Введите Ваш логин и пароль.");
                return; 
            }

            var user = _userRepository.GetUserByLogin(login);

            if (user != null)
            {
                // Хешируем введённый пароль с его солью
                string hashedInputPassword = PasswordHelper.HashPassword(password, user.Salt);

                if (hashedInputPassword == user.PasswordHash)
                {
                    LoginInput.Text = "";
                    PasswordInput.Password = "";

                    CurrentUser = user;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль");
                }
            }
            else
            {
                MessageBox.Show("Пользователь не найден");
            }
        }

        /* - - - - - */

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

    }
}

