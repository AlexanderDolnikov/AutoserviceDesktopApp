using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AutoserviceApp.Models;
using AutoserviceApp.ViewModels;
using AutoserviceApp.Interfaces;
using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.DataAccess;
using DocumentFormat.OpenXml.InkML;

namespace AutoserviceApp.Views
{
    public partial class UsersView : UserControl, IRefreshable
    {
        private readonly UserRepository _userRepository;
        private readonly DatabaseContext _context;
        private User _selectedUser;

        public UsersView()
        {
            InitializeComponent();
            _context = new DatabaseContext();
            _userRepository = new UserRepository(_context);
        }

        public void RefreshData()
        {
            LoadUsers();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 3);
                e.Handled = true;
            }
        }

        private void LoadUsers()
        {
            UsersListBox.ItemsSource = _userRepository.GetAllUsers();
        }

        private void UsersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersListBox.SelectedItem is User selectedUser)
            {
                _selectedUser = selectedUser;
                UserLoginInput.Text = selectedUser.Login;
                UserRoleInput.SelectedItem = UserRoleInput.Items
                    .Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == selectedUser.Role);
            }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UserLoginInput.Text) ||
                string.IsNullOrWhiteSpace(UserPasswordInput.Password) ||
                UserRoleInput.SelectedItem == null)
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string login = UserLoginInput.Text.Trim();
            string password = UserPasswordInput.Password;
            string role = (UserRoleInput.SelectedItem as ComboBoxItem)?.Content.ToString();

            try
            {
                _userRepository.AddUser(login, password, role);
                LoadUsers();
                MessageBox.Show("Пользователь успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                //UserLoginInput.Clear();
                UserPasswordInput.Clear();
                //UserRoleInput.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Violation of UNIQUE KEY constraint 'UQ__Users__")) {
                    MessageBox.Show($"Ошибка: Пользователь с таким логином уже существует!");
                }
                else
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для редактирования!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string newLogin = UserLoginInput.Text.Trim();
            string newRole = (UserRoleInput.SelectedItem as ComboBoxItem)?.Content.ToString();
            string newPassword = UserPasswordInput.Password;

            try
            {
                _userRepository.UpdateUser(_selectedUser.Id, newLogin, newRole);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        
            _userRepository.UpdateUserPassword(_selectedUser.Id, newPassword);

            LoadUsers();
            MessageBox.Show("Данные пользователя обновлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is not User selectedUser)
                return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить пользователя {selectedUser.Login}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _userRepository.DeleteUser(selectedUser.Id);
                LoadUsers();
                MessageBox.Show("Пользователь удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
