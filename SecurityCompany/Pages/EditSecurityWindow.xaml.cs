using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
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

namespace SecurityCompany.Pages
{
    /// <summary>
    /// Логика взаимодействия для EditSecurityWindow.xaml
    /// </summary>
    public partial class EditSecurityWindow : Window
    {
        public Security currentSecurity;
        private string currentImagePath;
        private bool isNew;
        public static bool isEditWindowOpen { get; private set; }

        public EditSecurityWindow(Security security)
        {
            InitializeComponent();
            this.currentSecurity = security;
            isEditWindowOpen = true;
            isNew = currentSecurity == null;

            this.Title = isNew ? "Добавление охранника" : "Редактирование охранника";
            TitleWindow.Content = this.Title;

            // Заполняем ComboBox статусами
            LoadStatuses();

            if (currentSecurity != null && !isNew)
            {
                try
                {
                    // Заполняем поля формы
                    TBFirstName.Text = security.FirstName;
                    TBSecondName.Text = security.SecondName;
                    TBPostName.Text = security.Post;
                    PhotoPreview.Source = ConvertByteArrayToImage(security.Avatar);

                    // Выбираем статус в ComboBox
                    StatusComboBox.SelectedItem = security.Status;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}",
                             "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            this.Closed += (s, e) => isEditWindowOpen = false;
        }

        private void LoadStatuses()
        {
            try
            {
                // Получаем список уникальных статусов из базы данных
                var statuses = App.db.Security
                    .Select(s => s.Status)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList();

                // Если статусов нет, добавляем стандартные
                if (statuses.Count == 0)
                {
                    statuses.Add("Активен");
                    statuses.Add("Уволен");
                    statuses.Add("В отпуске");
                    statuses.Add("На больничном");
                }

                // Очищаем ComboBox и добавляем статусы
                StatusComboBox.Items.Clear();
                foreach (var status in statuses)
                {
                    StatusComboBox.Items.Add(status);
                }

                // Если статус еще не выбран, выбираем "Активен" по умолчанию для нового сотрудника
                if (isNew && StatusComboBox.SelectedItem == null)
                {
                    StatusComboBox.SelectedItem = "Активен";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке статусов: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                // Добавляем стандартные статусы в случае ошибки
                StatusComboBox.Items.Clear();
                StatusComboBox.Items.Add("Активен");
                StatusComboBox.Items.Add("Уволен");
                StatusComboBox.Items.Add("В отпуске");
                StatusComboBox.Items.Add("На больничном");

                if (isNew)
                {
                    StatusComboBox.SelectedItem = "Активен";
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrEmpty(TBFirstName.Text))
            {
                MessageBox.Show("Введите фамилию", "Заполните все поля", MessageBoxButton.OK, MessageBoxImage.Error);
                TBFirstName.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(TBSecondName.Text))
            {
                MessageBox.Show("Введите имя", "Заполните все поля", MessageBoxButton.OK, MessageBoxImage.Error);
                TBSecondName.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(TBPostName.Text))
            {
                MessageBox.Show("Введите должность", "Заполните все поля", MessageBoxButton.OK, MessageBoxImage.Error);
                TBPostName.Focus();
                return false;
            }
            if (StatusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите статус сотрудника", "Заполните все поля", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusComboBox.Focus();
                return false;
            }
            return true;
        }

        private BitmapImage ConvertByteArrayToImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            try
            {
                using (var ms = new MemoryStream(imageData))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    image.Freeze();
                    return image;
                }
            }
            catch
            {
                return null;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                // Заполняем данные сотрудника
                currentSecurity.FirstName = TBFirstName.Text.Trim();
                currentSecurity.SecondName = TBSecondName.Text.Trim();
                currentSecurity.Post = TBPostName.Text.Trim();
                currentSecurity.Status = StatusComboBox.SelectedItem.ToString(); // Получаем выбранный статус

                // Обработка изображения
                bool hasNewImage = !string.IsNullOrWhiteSpace(currentImagePath);

                if (hasNewImage)
                {
                    // Если выбрано новое изображение
                    currentSecurity.Avatar = File.ReadAllBytes(currentImagePath);
                }
                else if (isNew && !hasNewImage)
                {
                    // Если новый сотрудник и изображение не выбрано, ставим заглушку
                    try
                    {
                        var resourceStream = Application.GetResourceStream(new Uri("Resources/Avatar.png", UriKind.Relative));
                        if (resourceStream != null)
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                resourceStream.Stream.CopyTo(memoryStream);
                                currentSecurity.Avatar = memoryStream.ToArray();
                            }
                        }
                    }
                    catch
                    {
                        // Если заглушка не найдена, оставляем null
                        currentSecurity.Avatar = null;
                    }
                }

                // Сохраняем в базу данных
                if (isNew)
                {
                    App.db.Security.Add(currentSecurity);
                }
                else
                {
                    App.db.Entry(currentSecurity).State = EntityState.Modified;
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
            this.Close();
        }

        private void BrowsePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Выберите изображение";
                openFileDialog.Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                bool? result = openFileDialog.ShowDialog();

                if (result == true)
                {
                    currentImagePath = openFileDialog.FileName;

                    // Показываем превью выбранного изображения
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(currentImagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    PhotoPreview.Source = bitmap;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}