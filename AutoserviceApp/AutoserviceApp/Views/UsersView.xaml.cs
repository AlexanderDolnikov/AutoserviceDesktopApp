using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using AutoserviceApp.Models;
using AutoserviceApp.ViewModels;
using AutoserviceApp.DataAccess;

namespace AutoserviceApp.Views
{
    public partial class UsersView : UserControl
    {
        public UsersView()
        {
            InitializeComponent();
            //LoadUsers();
        }

        //private void LoadUsers()
        //{
        //    List<User> users = new List<User>
        //    {
        //        new User { Id = 1, Login = "admin", Role = "Администратор" },
        //        new User { Id = 2, Login = "employee1", Role = "Сотрудник" }
        //    };

        //    UsersList.ItemsSource = users;
        //}

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel)
                return;

            if (viewModel.CurrentUser == null || viewModel.CurrentUser.Role != "Администратор")
            {
                MessageBox.Show("Вы не можете добавлять пользователей!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string login = UserLoginInput.Text;
            string password = UserPasswordInput.Password;
            string role = (UserRoleInput.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password) || role == null)
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Хеширование пароля перед сохранением
                string salt = PasswordHelper.GenerateSalt();
                string passwordHash = PasswordHelper.HashPassword(password, salt);

                viewModel.Users.Add(new User
                {
                    Login = login,
                    PasswordHash = passwordHash,  // Используем хеш вместо обычного пароля
                    Salt = salt,
                    Role = role
                });

                MessageBox.Show("Пользователь успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
