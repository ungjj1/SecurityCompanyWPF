using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SecurityCompany.Pages
{
    /// <summary>
    /// Логика взаимодействия для ClientPage.xaml
    /// </summary>
    public partial class ClientPage : Page
    {
        private User currentUser;
        private List<Client> allClients;
        private static bool IsEditWindowOpen = false;

        public ClientPage(User user)
        {
            InitializeComponent();
            this.currentUser = user;

            if (currentUser != null && currentUser.Role != "Управляющий")
            {
                BtnCreateClient.Visibility = Visibility.Collapsed;
                BtnFullReport.Visibility = Visibility.Collapsed;
            }

            LoadSortOptions();

            LoadData();
        }

        private void LoadSortOptions()
        {
            SortComboBox.Items.Clear();
            SortComboBox.Items.Add("Без фильтров");
            SortComboBox.Items.Add("По названию компании (А-Я)");
            SortComboBox.Items.Add("По названию компании (Я-А)");
            SortComboBox.Items.Add("По номеру договора (возр.)");
            SortComboBox.Items.Add("По номеру договора (убыв.)");
            SortComboBox.Items.Add("По названию объекта (А-Я)");
            SortComboBox.Items.Add("По названию объекта (Я-А)");
            SortComboBox.SelectedIndex = 0;
        }

        private void LoadData()
        {
            try
            {
                allClients = App.db.Client
                    .Include(c => c.Object)
                    .Include(c => c.Object.Security)
                    .ToList();

                RefreshClientList();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshClientList()
        {
            try
            {
                if (allClients == null) return;

                IEnumerable<Client> filteredClients = allClients;

                // Поиск
                string search = TBSearch.Text?.ToLower() ?? "";
                if (!string.IsNullOrEmpty(search))
                {
                    filteredClients = filteredClients.Where(c =>
                        (c.NameCompany != null && c.NameCompany.ToLower().Contains(search)) ||
                        (c.ContractNumber != null && c.ContractNumber.ToLower().Contains(search)) ||
                        (c.Object != null && c.Object.Adress != null && c.Object.Adress.ToLower().Contains(search)) ||
                        (c.Object != null && c.Object.Name != null && c.Object.Name.ToLower().Contains(search))
                    );
                }

                // Сортировка
                if (SortComboBox.SelectedItem != null && SortComboBox.SelectedItem.ToString() != "Без фильтров")
                {
                    string selectedSort = SortComboBox.SelectedItem.ToString();

                    switch (selectedSort)
                    {
                        case "По названию компании (А-Я)":
                            filteredClients = filteredClients.OrderBy(c => c.NameCompany);
                            break;

                        case "По названию компании (Я-А)":
                            filteredClients = filteredClients.OrderByDescending(c => c.NameCompany);
                            break;

                        case "По номеру договора (возр.)":
                            filteredClients = filteredClients.OrderBy(c => c.ContractNumber);
                            break;

                        case "По номеру договора (убыв.)":
                            filteredClients = filteredClients.OrderByDescending(c => c.ContractNumber);
                            break;

                        case "По названию объекта (А-Я)":
                            filteredClients = filteredClients.OrderBy(c => c.Object != null ? c.Object.Name : "");
                            break;

                        case "По названию объекта (Я-А)":
                            filteredClients = filteredClients.OrderByDescending(c => c.Object != null ? c.Object.Name : "");
                            break;
                    }
                }

                ClientsList.ItemsSource = filteredClients.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении списка: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeleteClient_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var client = button?.DataContext as Client;

                if (client == null) return;

                // Подтверждение расторжения договора
                var result = MessageBox.Show(
                    $"Вы действительно хотите расторгнуть договор с клиентом?\n\n" +
                    $"Компания: {client.NameCompany}\n" +
                    $"Договор: {client.ContractNumber}\n" +
                    $"Объект: {client.Object?.Name ?? "Не указан"}\n\n" +
                    $"Это действие нельзя отменить!",
                    "Расторжение договора",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // Удаляем клиента из базы данных
                    App.db.Client.Remove(client);
                    App.db.SaveChanges();

                    MessageBox.Show($"Договор с клиентом {client.NameCompany} успешно расторгнут",
                                  "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadData(); // Обновляем список
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при расторжении договора: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TBSearch_SelectionChanged(object sender, RoutedEventArgs e)
        {
            RefreshClientList();
        }

        private void TBSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshClientList();
        }

        private void BtnCreateClient_Click(object sender, RoutedEventArgs e)
        {
            OpenEditWindow(null);
        }

        private void BtnFullReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GenerateReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании отчета: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateReport()
        {
            var clients = ClientsList.ItemsSource as List<Client>;
            if (clients == null || !clients.Any())
            {
                MessageBox.Show("Нет данных для формирования отчета", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string report = "ОТЧЕТ ПО КЛИЕНТАМ\n";
            report += new string('=', 50) + "\n";
            report += $"Дата: {DateTime.Now:dd.MM.yyyy HH:mm:ss}\n";
            report += $"Всего клиентов: {clients.Count}\n";
            report += new string('=', 50) + "\n\n";

            int counter = 1;
            foreach (var client in clients)
            {
                report += $"{counter}. {client.NameCompany}\n";
                report += $"   Договор: {client.ContractNumber}\n";
                report += $"   Объект: {client.Object?.Name ?? "Не указан"}\n";
                report += $"   Адрес: {client.Object?.Adress ?? "Не указан"}\n\n";
                counter++;
            }

            MessageBox.Show(report, "Отчет по клиентам", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var border = sender as Border;
                var client = border?.DataContext as Client;

                if (client != null)
                {
                    if (currentUser.Role == "Управляющий")
                    {
                        OpenEditWindow(client);
                    }
                    else
                    {
                        MessageBox.Show("У вас нет прав для редактирования клиентов",
                                      "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выборе клиента: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenEditWindow(Client client)
        {
            try
            {
                if (IsEditWindowOpen || EditClientWindow.IsEditWindowOpen)
                {
                    MessageBox.Show("Окно редактирования уже открыто", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                IsEditWindowOpen = true;

                var editWindow = new EditClientWindow(client);
                editWindow.Owner = Window.GetWindow(this);
                editWindow.Closed += (s, args) =>
                {
                    IsEditWindowOpen = false;
                    LoadData(); // Обновляем данные после закрытия окна
                };
                editWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                IsEditWindowOpen = false;
                MessageBox.Show($"Ошибка при открытии окна редактирования: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshClientList();
        }
    }
}