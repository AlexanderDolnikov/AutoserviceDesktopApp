using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Windows;
using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.DataAccess;
using AutoserviceApp.Models;
using AutoserviceApp.Interfaces;
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

            var user = _userRepository.GetUserByLogin(login);

            if (user != null)
            {
                // Хешируем введённый пароль с его солью
                string hashedInputPassword = PasswordHelper.HashPassword(password, user.Salt);

                if (hashedInputPassword == user.PasswordHash)
                {
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
    }
}

