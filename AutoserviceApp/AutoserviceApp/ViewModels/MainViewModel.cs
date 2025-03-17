using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using AutoserviceApp.Models;
using AutoserviceApp.Interfaces;
using AutoserviceApp.DataAccess.Models;

namespace AutoserviceApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        /* Current view: */
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

        /* Current user: */
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

        /* Current order: */
        private OrderWithInfo? _selectedOrder;
        public OrderWithInfo? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged(nameof(SelectedOrder));
            }
        }

        /* Current work: */
        private WorkWithInfo? _selectedWork;
        public WorkWithInfo? SelectedWork
        {
            get => _selectedWork;
            set
            {
                _selectedWork = value;
                OnPropertyChanged(nameof(SelectedWork));
            }
        }

        /* - - - - - */

        public ObservableCollection<User> Users { get; set; }
        public Dictionary<string, Func<UserControl>> ViewMappings { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            ViewMappings = new Dictionary<string, Func<UserControl>>
            {                
                { "Заказы", () => new Views.OrdersView() },
                { "Работы", () => new Views.WorksView() },
                { "ДеталиРаботы", () => new Views.WorkDetailsView() },
                { "Жалобы", () => new Views.ComplaintsView() },
                { "Модели", () => new Views.ModelsView() },
                { "Автомобили", () => new Views.CarsView() },
                { "Клиенты", () => new Views.ClientsView() },
                { "Детали", () => new Views.DetailsView() },
                { "Мастера", () => new Views.MastersView() },
                { "Виды работ", () => new Views.WorkTypesView() },
                { "Пользователи", () => new Views.UsersView() },
                { "Отчеты", () => new Views.ReportsView() }
            };
        }

        public void SwitchView(string viewName)
        {
            if (ViewMappings.ContainsKey(viewName))
            {
                var newView = ViewMappings[viewName]();

                // Принудительно передаем DataContext из текущей ViewModel в новую View
                if (newView is UserControl view)
                {
                    view.DataContext = this;
                }

                CurrentView = newView;

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

