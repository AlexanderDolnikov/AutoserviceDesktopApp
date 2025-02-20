using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.Models;
using AutoserviceApp.DataAccess;

namespace AutoserviceApp.Views
{
    public partial class OrdersView : UserControl
    {
        private readonly OrderRepository _orderRepository;
        private readonly DatabaseContext _context;
        private List<Order> _orders;

        public OrdersView()
        {
            InitializeComponent();
            _context = new DatabaseContext();
            _orderRepository = new OrderRepository(_context);
            LoadOrders();
        }

        private void LoadOrders()
        {
            _orders = _orderRepository.GetAllOrders();
            OrdersListPanel.Children.Clear();

            foreach (var order in _orders)
            {
                // Создаем Border для рамки
                Border orderBorder = new Border
                {
                    BorderBrush = new SolidColorBrush(Colors.Gray),
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(0, 5, 0, 5),
                    Padding = new Thickness(5),
                    Background = new SolidColorBrush(Colors.White)
                };

                // Внутри Border создаем StackPanel
                StackPanel orderPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };

                // Дата начала
                TextBlock startDate = new TextBlock
                {
                    Text = order.ДатаНачала.ToString("dd.MM.yyyy"),
                    Width = 100,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                };

                // Разделитель
                Border separator1 = new Border
                {
                    Width = 2,
                    Background = new SolidColorBrush(Colors.Gray),
                    Margin = new Thickness(5, 0, 5, 0)
                };

                // Дата окончания
                TextBlock endDate = new TextBlock
                {
                    Text = order.ДатаОкончания.ToString("dd.MM.yyyy"),
                    Width = 100,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                };

                // Разделитель
                Border separator2 = new Border
                {
                    Width = 2,
                    Background = new SolidColorBrush(Colors.Gray),
                    Margin = new Thickness(5, 0, 5, 0)
                };

                // Код клиента
                TextBlock clientCode = new TextBlock
                {
                    Text = $"Клиент ID: {order.КодКлиента}",
                    Width = 120,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                };

                // Кнопка "Подробнее"
                Button detailsButton = new Button
                {
                    Content = "Подробнее",
                    Background = new SolidColorBrush(Color.FromRgb(170, 212, 19)),
                    Foreground = new SolidColorBrush(Colors.Black),
                    Padding = new Thickness(5),
                    FontWeight = FontWeights.Bold
                };
                detailsButton.Click += (sender, e) => ShowOrderDetails(order);

                // Добавляем элементы в StackPanel
                orderPanel.Children.Add(startDate);
                orderPanel.Children.Add(separator1);
                orderPanel.Children.Add(endDate);
                orderPanel.Children.Add(separator2);
                orderPanel.Children.Add(clientCode);
                orderPanel.Children.Add(detailsButton);

                // Добавляем StackPanel внутрь Border
                orderBorder.Child = orderPanel;

                // Добавляем Border в главный контейнер
                OrdersListPanel.Children.Add(orderBorder);
            }
        }

        private void ShowOrderDetails(Order order)
        {
            MessageBox.Show($"Детали заказа {order.Код}\nКлиент ID: {order.КодКлиента}\nАвтомобиль ID: {order.КодАвтомобиля}",
                            "Детали заказа", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
