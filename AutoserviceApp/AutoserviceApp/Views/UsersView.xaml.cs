using System.Windows;
using System.Windows.Controls;
using AutoserviceApp.Models;
using AutoserviceApp.Interfaces;
using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.DataAccess;
using AutoserviceApp.Helpers;

namespace AutoserviceApp.Views
{
    public partial class UsersView : UserControl, IRefreshable
    {
        private readonly UserRepository _userRepository;
        private readonly DatabaseContext _context;
        private User _selectedUser;
        private int _selectedUserIndex;

        public UsersView()
        {
            InitializeComponent();
            _context = new DatabaseContext();
            _userRepository = new UserRepository(_context);

            RefreshData();
        }

        public void RefreshData()
        {
            LoadUsers();
        }
        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollHelper.HandleMouseWheel(sender, e);
        }

        private void SetFocusOnFirstInput(object sender = null, RoutedEventArgs? e = null)
        {
            ViewFocusHelper.SetFocusAndClearItemsValues(UserLoginInput, UserPasswordInput, UserRoleDropdown);
        }

        /* - - - - - */
        private void LoadUsers()
        {
            UsersListBox.ItemsSource = _userRepository.GetAllUsers();
        }

        private void UsersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersListBox.SelectedItem is User selectedUser)
            {
                _selectedUser = selectedUser;
                _selectedUserIndex = UsersListBox.SelectedIndex;

                UserLoginInput.Text = selectedUser.Login;
                UserRoleDropdown.Text = selectedUser.Role;
            }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UserLoginInput.Text) ||
                string.IsNullOrWhiteSpace(UserPasswordInput.Password) ||
                UserRoleDropdown.SelectedItem == null)
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string login = UserLoginInput.Text.Trim();
            string password = UserPasswordInput.Password;
            string role = (UserRoleDropdown.SelectedItem as ComboBoxItem)?.Content.ToString();

            try
            {
                _userRepository.AddUser(login, password, role);
                LoadUsers();
                MessageBox.Show("Пользователь успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                SetFocusOnFirstInput();
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
            string newRole = (UserRoleDropdown.SelectedItem as ComboBoxItem)?.Content.ToString();
            string newPassword = UserPasswordInput.Password;

            if (newLogin is null || newRole is null || newPassword is null)
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _userRepository.UpdateUser(_selectedUser.Id, newLogin, newRole);
                _userRepository.UpdateUserPassword(_selectedUser.Id, newPassword);

                MessageBox.Show("Данные пользователя обновлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        
            LoadUsers();
            UsersListBox.SelectedIndex = _selectedUserIndex;
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
                _userRepository.Delete(selectedUser.Id);
                LoadUsers();
                MessageBox.Show("Пользователь удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            SetFocusOnFirstInput();
        }
    }
}
