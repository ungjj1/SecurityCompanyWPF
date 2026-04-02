using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
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
using System.Xml.Linq;

namespace SecurityCompany.Pages
{
    /// <summary>
    /// Логика взаимодействия для EditClientWindow.xaml
    /// </summary>
    public partial class EditClientWindow : Window
    {
        private Client currentClient;
        private bool isNew;

        public static bool IsEditWindowOpen { get; private set; }

        public EditClientWindow(Client client)
        {
            InitializeComponent();
            this.currentClient = client;
            IsEditWindowOpen = true;
            isNew = currentClient == null;

            this.Title = isNew ? "Добавление клиента" : "Редактирование клиента";
            TitleWindow.Content = this.Title;

            LoadObjectsList();

            if (!isNew && currentClient != null)
            {
                try
                {
                    TBCompanyName.Text = currentClient.NameCompany;
                    TBDocNum.Text = currentClient.ContractNumber;
                    ObjectsComboBox.SelectedItem = currentClient.Object.Name;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            this.Closed += (s, o) => IsEditWindowOpen = false;
        }

        private void LoadObjectsList()
        {
            try
            {
                var objectsList = App.db.Object.ToList();
                ObjectsComboBox.ItemsSource = objectsList;
                ObjectsComboBox.DisplayMemberPath = "Name";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке списка объектов: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool ValidateInput()
        {
            if(string.IsNullOrWhiteSpace(TBCompanyName.Text))
            {
                MessageBox.Show("Название компании не может быть пустым", "Введите данные", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                TBCompanyName.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(TBDocNum.Text))
            {
                MessageBox.Show("Поле с документом не может быть пустым", "Введите данные", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                TBDocNum.Focus();
                return false;
            }
            if (ObjectsComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите объект из списка", "Введите данные", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                TBCompanyName.Focus();
                return false;
            }

            return true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())

                    return;

                currentClient.NameCompany = TBCompanyName.Text.Trim();
                currentClient.ContractNumber = TBDocNum.Text.Trim();
                currentClient.Object = ObjectsComboBox.SelectedItem as Object;

                if (isNew)
                {
                    App.db.Entry(currentClient).State = System.Data.Entity.EntityState.Modified;
                }

                App.db.SaveChanges();

                MessageBox.Show("Данные успешно сохранены!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                this.Close();
            }
            catch (DbEntityValidationException ex)
            {
                // Собираем все ошибки валидации
                var errorMessages = new List<string>();
                foreach (var entityValidationError in ex.EntityValidationErrors)
                {
                    var entityType = entityValidationError.Entry.Entity.GetType().Name;
                    var entityState = entityValidationError.Entry.State;

                    foreach (var validationError in entityValidationError.ValidationErrors)
                    {
                        var errorMessage = $"Сущность: {entityType}, " +
                                           $"Состояние: {entityState}, " +
                                           $"Свойство: {validationError.PropertyName}, " +
                                           $"Ошибка: {validationError.ErrorMessage}";
                        errorMessages.Add(errorMessage);
                        Debug.WriteLine(errorMessage);
                    }
                }

                MessageBox.Show($"Ошибка валидации данных:\n{string.Join("\n", errorMessages)}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}