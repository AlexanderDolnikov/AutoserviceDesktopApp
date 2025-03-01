using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using AutoserviceApp.Models;
using System.Windows.Markup;
using AutoserviceApp.Interfaces;

namespace AutoserviceApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private UserControl _currentView;
        public UserControl CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
            }
        }
        public ObservableCollection<User> Users { get; set; }


        public Dictionary<string, Func<UserControl>> ViewMappings { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            ViewMappings = new Dictionary<string, Func<UserControl>>
            {
                { "Заказы", () => new Views.OrdersView() },
                { "Работы", () => new Views.WorksView() },
                { "Жалобы", () => new Views.ComplaintsView() },
                { "Мастера", () => new Views.MastersView() },
                { "Детали", () => new Views.DetailsView() },
                { "Пользователи", () => new Views.UsersView() }
            };

            // Открываем первую вкладку по умолчанию
            if (ViewMappings.Any())
            {
                CurrentView = ViewMappings.First().Value();
            }
        }

        public void SwitchView(string viewName)
        {
            if (ViewMappings.ContainsKey(viewName))
            {
                CurrentView = ViewMappings[viewName]();

                // Принудительная загрузка данных при переключении вкладки
                if (CurrentView is IRefreshable refreshable)
                {
                    refreshable.RefreshData();
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}

