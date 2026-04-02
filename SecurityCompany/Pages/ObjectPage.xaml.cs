using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace SecurityCompany.Pages
{
    /// <summary>
    /// Логика взаимодействия для ObjectPage.xaml
    /// </summary>
    public partial class ObjectPage : Page
    {
        private User currentUser;
        private List<Object> allObjects;
        private static bool isEditWindowOpen = false;

        public ObjectPage(User user)
        {
            InitializeComponent();
            this.currentUser = user;

            if (currentUser != null && currentUser.Role != "Управляющий")
            {
                BtnCreateObject.Visibility = Visibility.Collapsed;
            }
            LoadSortOptions();
            LoadData();
        }
        private void LoadSortOptions()
        {
            SortComboBox.Items.Clear();
            SortComboBox.Items.Add("Без сортировки");
            SortComboBox.Items.Add("По названию объекта (А-Я)");
            SortComboBox.Items.Add("По названию объекта (Я-А)");
            SortComboBox.Items.Add("По адресу (А-Я)");
            SortComboBox.Items.Add("По адресу(Я - А)");
            SortComboBox.SelectedIndex = 0;
        }
        private void LoadData()
        {
            try
            {
                allObjects = App.db.Object.ToList();
                RefreshObjectsList();

                var statuses = App.db
                    .Object.Select(p => p.Status)
                    .Where(m => !string.IsNullOrEmpty(m))
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList();

                FilterComboBox.Items.Clear(); // Очищаем перед добавлением
                FilterComboBox.Items.Add("Все статусы");

                foreach (var status in statuses)
                {
                    FilterComboBox.Items.Add(status);
                }
                FilterComboBox.SelectedIndex = 0;
                SortComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных", $"{ex}", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RefreshObjectsList()
        {
            try
            {
                if (allObjects == null) return;

                IEnumerable<Object> filteredObjects = allObjects;

                string searchText = TBSearch.Text?.ToLower() ?? "";
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    filteredObjects = filteredObjects.Where(o =>
                        (o.Name != null && o.Name.ToLower().Contains(searchText)) ||
                        (o.Adress != null && o.Adress.ToLower().Contains(searchText)) ||
                        (o.Security != null && o.Security.FirstName != null && o.Security.FirstName.ToLower().Contains(searchText)) ||
                        (o.Security != null && o.Security.SecondName != null && o.Security.SecondName.ToLower().Contains(searchText))
                    );
                }

                if (SortComboBox.SelectedItem != null && SortComboBox.SelectedItem.ToString() != "Без фильтров")
                {
                    string selectedSort = SortComboBox.SelectedItem.ToString();

                    switch (selectedSort)
                    {
                        case "По названию объекта (А-Я)":
                            filteredObjects = filteredObjects.OrderBy(c => c.Name);
                            break;

                        case "По названию объекта (Я-А)":
                            filteredObjects = filteredObjects.OrderByDescending(c => c.Name);
                            break;

                        case "По адресу (А-Я)":
                            filteredObjects = filteredObjects.OrderBy(c => c.Adress);
                            break;

                        case "По адресу (Я-А)":
                            filteredObjects = filteredObjects.OrderByDescending(c => c.Adress);
                            break;
                        default:
                            break;
                    }
                }

                // Сортировка
                if (SortComboBox.SelectedItem != null)
                {
                    var selectedSort = SortComboBox.SelectedItem as ComboBoxItem;
                    string sortTag = selectedSort?.Tag as string;

                    switch (sortTag)
                    {
                        case "Secured":
                            filteredObjects = filteredObjects.OrderBy(o => o.Status);
                            break;
                        case "Temporarily":
                            filteredObjects = filteredObjects.OrderBy(o => o.Status);
                            break;
                        case "NotSecure":
                            filteredObjects = filteredObjects.OrderBy(o => o.Status);
                            break;
                        default:
                            filteredObjects = filteredObjects.OrderBy(o => o.Id);
                            break;
                    }
                }

                ObjectList.ItemsSource = filteredObjects.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении списка", $"{ex}", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenEditWindow(Object selectedObject)
        {
            try
            {
                // Исправлено: используем правильное имя статического поля IsEditWindowOpen (с большой буквы)
                if (isEditWindowOpen || EditObjectWindow.IsEditWindowOpen)
                {
                    MessageBox.Show("Окно редактирования уже открыто", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                isEditWindowOpen = true;

                var editWindow = new EditObjectWindow(selectedObject);
                editWindow.Owner = Window.GetWindow(this);
                editWindow.Closed += (s, args) =>
                {
                    isEditWindowOpen = false;

                    try
                    {
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при обновлении данных", $"{ex}", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };
                editWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                isEditWindowOpen = false;
                MessageBox.Show("Ошибка при открытии окна редактирования", $"{ex}", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var border = sender as Border;
                var object1 = border?.DataContext as Object;

                if (object1 != null)
                {
                    if (currentUser.Role == "Управляющий")
                    {
                        
                        OpenEditWindow(object1);
                    }
                    else
                    {
                        MessageBox.Show("У вас нет прав для редактирования объектов", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при выборе объекта", $"{ex}", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnReport_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnFullReport_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnCreateObject_Click(object sender, RoutedEventArgs e)
        {
            OpenEditWindow(null);
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshObjectsList();
        }

        private void TBSearch_SelectionChanged(object sender, RoutedEventArgs e)
        {
            RefreshObjectsList();
        }

        // Добавьте этот метод, если его нет в XAML
        private void TBSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshObjectsList();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshObjectsList();
        }
    }
}