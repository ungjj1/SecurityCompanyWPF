using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SecurityCompany.Pages
{
    public class AvatarConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Проверяем, есть ли изображение в базе
            if (value is byte[] bytes && bytes.Length > 0)
            {
                try
                {
                    using (MemoryStream stream = new MemoryStream(bytes))
                    {
                        BitmapImage image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = stream;
                        image.EndInit();
                        image.Freeze();
                        return image;
                    }
                }
                catch
                {
                    // Если не удалось загрузить изображение из БД, возвращаем заглушку
                    return GetPlaceholderImage();
                }
            }

            // Если изображения нет в БД, возвращаем заглушку
            return GetPlaceholderImage();
        }

        private BitmapImage GetPlaceholderImage()
        {
            try
            {
                // Путь к заглушке (Avatar.png должна быть в папке Resources)
                string imagePath = "pack://application:,,,/Resources/Avatar.png";

                BitmapImage placeholder = new BitmapImage();
                placeholder.BeginInit();
                placeholder.UriSource = new Uri(imagePath, UriKind.Absolute);
                placeholder.CacheOption = BitmapCacheOption.OnLoad;
                placeholder.EndInit();
                placeholder.Freeze();

                return placeholder;
            }
            catch
            {
                // Если не удалось загрузить заглушку, возвращаем null
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}