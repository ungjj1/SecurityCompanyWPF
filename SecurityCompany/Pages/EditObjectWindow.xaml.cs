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
    /// Логика взаимодействия для EditObjectWindow.xaml
    /// </summary>
    public partial class EditObjectWindow : Window
    {
        private Object currentObject;
        private string currentImagePath;
        private bool isNew = false;

        public static bool IsEditWindowOpen { get; private set; }

        public EditObjectWindow(Object currentObject)
        {
            InitializeComponent();
            IsEditWindowOpen = true;
            this.currentObject = currentObject;
            isNew = this.currentObject == null;

            // Создаем новый объект, если его нет
            if (isNew)
            {
                this.currentObject = new Object();
            }

            this.Title = isNew ? "Добавление объекта" : "Редактирование объекта";
            TitleWindow.Content = this.Title;

            // Загружаем статусы
            LoadStatuses();

            // Загружаем список охранников
            LoadSecurityList();

            if (!isNew && currentObject != null)
            {
                try
                {
                    // Заполняем поля формы
                    TBName.Text = currentObject.Name;
                    TBAdress.Text = currentObject.Adress;
                    StatusComboBox.SelectedItem = currentObject.Status;

                    // Выбираем охранника в ComboBox
                    if (currentObject.Security != null)
                    {
                        SecurityComboBox.SelectedItem = currentObject.Security;
                    }

                    PhotoPreview.Source = ConvertByteArrayToImage(currentObject.ObjectPhoto);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}",
                             "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            this.Closed += (s, o) => IsEditWindowOpen = false;
        }

        private void LoadSecurityList()
        {
            try
            {
                // Получаем список всех охранников
                var securityList = App.db.Security.ToList();
                SecurityComboBox.ItemsSource = securityList;
                SecurityComboBox.DisplayMemberPath = "SecondName"; // Или можно создать FullName
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке списка охранников: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void LoadStatuses()
        {
            try
            {
                var statuses = App.db.Object
                    .Select(s => s.Status)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList();

                if (statuses.Count == 0)
                {
                    statuses.Add("Под охраной");
                    statuses.Add("Временная охрана");
                    statuses.Add("Без охраны");
                }

                StatusComboBox.Items.Clear();
                foreach (var status in statuses)
                {
                    StatusComboBox.Items.Add(status);
                }

                if (isNew && StatusComboBox.SelectedItem == null)
                {
                    StatusComboBox.SelectedItem = "Без охраны";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке статусов: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                // Добавляем стандартные статусы в случае ошибки
                StatusComboBox.Items.Clear();
                StatusComboBox.Items.Add("Под охраной");
                StatusComboBox.Items.Add("Временная охрана");
                StatusComboBox.Items.Add("Без охраны");

                if (isNew)
                {
                    StatusComboBox.SelectedItem = "Без охраны";
                }
            }
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

                    // Показываем превью
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(currentImagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    PhotoPreview.Source = bitmap;

                    // Отображаем путь в TextBox
                    PhotoTextBox.Text = currentImagePath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                // Заполняем данные объекта
                currentObject.Name = TBName.Text.Trim();
                currentObject.Adress = TBAdress.Text.Trim();
                currentObject.Status = StatusComboBox.SelectedItem?.ToString();

                // Получаем выбранного охранника
                currentObject.Security = SecurityComboBox.SelectedItem as Security;

                // Обработка изображения
                bool hasNewImage = !string.IsNullOrWhiteSpace(currentImagePath);

                if (hasNewImage)
                {
                    // Если выбрано новое изображение
                    currentObject.ObjectPhoto = File.ReadAllBytes(currentImagePath);
                }
                else if (isNew && !hasNewImage)
                {
                    // Если новый объект и изображение не выбрано, ставим заглушку
                    try
                    {
                        var resourceStream = Application.GetResourceStream(new Uri("Resources/picture.png", UriKind.Relative));
                        if (resourceStream != null)
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                resourceStream.Stream.CopyTo(memoryStream);
                                currentObject.ObjectPhoto = memoryStream.ToArray();
                            }
                        }
                    }
                    catch
                    {
                        // Если заглушка не найдена, оставляем null
                        currentObject.ObjectPhoto = null;
                    }
                }

                // Сохраняем в базу данных
                if (isNew)
                {
                    App.db.Object.Add(currentObject);
                }
                else
                {
                    App.db.Entry(currentObject).State = EntityState.Modified;
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

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(TBName.Text))
            {
                MessageBox.Show("Название объекта не может быть пустым", "Введите данные", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                TBName.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(TBAdress.Text))
            {
                MessageBox.Show("Адрес объекта не может быть пустым", "Введите данные", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                TBAdress.Focus();
                return false;
            }

            if (StatusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите статус объекта", "Введите данные", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                StatusComboBox.Focus();
                return false;
            }

            if (SecurityComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите охранника объекта", "Введите данные", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                SecurityComboBox.Focus();
                return false;
            }

            return true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}