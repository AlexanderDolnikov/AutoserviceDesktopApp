using System.Windows;
using System.Windows.Controls;
using AutoserviceApp.DataAccess;
using AutoserviceApp.Models;
using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.Interfaces;
using AutoserviceApp.Helpers;

namespace AutoserviceApp.Views
{
    public partial class ClientsView : UserControl, IRefreshable
    {
        private readonly DatabaseContext _context;
        private readonly ClientRepository _clientRepository;
        private readonly OrderRepository _orderRepository;
        private List<Client> _clients;
        private Client _selectedClient;
        private int _selectedClientIndex;

        public ClientsView()
        {
            InitializeComponent();
            _context = new DatabaseContext();
            _clientRepository = new ClientRepository(_context);
            _orderRepository = new OrderRepository(_context);

            RefreshData();
        }

        public void RefreshData() => LoadClients();

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollHelper.HandleMouseWheel(sender, e);
        }

        private void SetFocusOnFirstInput(object sender = null, RoutedEventArgs? e = null)
        {
            ViewFocusHelper.SetFocusAndClearItemsValues(ClientNameTextBox, ClientSurnameTextBox, ClientPhoneTextBox, ClientAddressTextBox);
        }

        /* - - - - - */

        private void LoadClients()
        {
            _clients = _clientRepository.GetAllClients();
            ClientsListBox.ItemsSource = _clients;
        }

        private void ClientsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClientsListBox.SelectedItem is Client selectedClient)
            {
                _selectedClient = selectedClient;
                _selectedClientIndex = ClientsListBox.SelectedIndex;

                ClientNameTextBox.Text = selectedClient.Имя;
                ClientSurnameTextBox.Text = selectedClient.Фамилия;
                ClientPhoneTextBox.Text = selectedClient.Телефон;
                ClientAddressTextBox.Text = selectedClient.Адрес;
            }
        }

        private void AddClient_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ClientNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(ClientSurnameTextBox.Text) ||
                string.IsNullOrWhiteSpace(ClientPhoneTextBox.Text))
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newClient = new Client
            {
                Имя = ClientNameTextBox.Text.Trim(),
                Фамилия = ClientSurnameTextBox.Text.Trim(),
                Телефон = ClientPhoneTextBox.Text.Trim(),
                Адрес = ClientAddressTextBox.Text.Trim()
            };

            _clientRepository.AddClient(newClient);
            LoadClients();

            SetFocusOnFirstInput();
        }

        private void EditClient_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClient == null) return;

            _selectedClient.Имя = ClientNameTextBox.Text.Trim();
            _selectedClient.Фамилия = ClientSurnameTextBox.Text.Trim();
            _selectedClient.Телефон = ClientPhoneTextBox.Text.Trim();
            _selectedClient.Адрес = ClientAddressTextBox.Text.Trim();

            _clientRepository.UpdateClient(_selectedClient);
            LoadClients();

            ClientsListBox.SelectedIndex = _selectedClientIndex;
        }

        private void DeleteClient_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is Client selectedClient)
            {
                // Проверка на существование связанных заказов
                bool hasOrders = _orderRepository.GetAllOrders().Any(o => o.КодКлиента == selectedClient.Код);

                if (hasOrders)
                {
                    var result = MessageBox.Show(
                        "Этот клиент имеет заказы. Все связанные данные будут удалены.\nВы уверены, что хотите удалить его?",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes) return;
                }

                try
                {
                    _clientRepository.DeleteClient(selectedClient.Код);
                    MessageBox.Show("Клиент успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadClients();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                SetFocusOnFirstInput();
            }
        }
    }
}
