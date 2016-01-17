using System;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace ActivityTrackerUWP.CustomControls
{
    /// <summary>
    /// EntityImage Control to display Entity Image
    /// </summary>
    public sealed partial class EntityImageControl : UserControl
    {
        public EntityImageControl()
        {
            this.InitializeComponent();
            SetImage(null);
        }

        private byte[] imageBytes;
        public byte[] ImageBytes
        {
            set
            {
                imageBytes = value;
                SetImage(value);
            }
        }

        // Expose ImageBytes as Property
        public static DependencyProperty ImageBytesProperty = DependencyProperty.Register("ImageBytes",
            typeof(byte[]), typeof(EntityImageControl), new PropertyMetadata(null, ImageBytesChangedCallback));

        /// <summary>
        /// Invoked when ImageBytes property is set
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void ImageBytesChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EntityImageControl entityImage = (EntityImageControl)d;
            entityImage.ImageBytes = e.NewValue as byte[];
        }

        /// <summary>
        /// This method set ImageSource to image.
        /// </summary>
        /// <param name="imageBytes"></param>
        private async void SetImage(byte[] imageBytes)
        {
            BitmapImage im;

            // If imageBytes has data in it, then use it.
            if (imageBytes != null)
            {
                im = new BitmapImage();
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    await im.SetSourceAsync(ms.AsRandomAccessStream());
                }
            }
            // Otherwise use default picture.
            else
                im = new BitmapImage(new Uri("ms-appx:///Assets/icon-contact.png"));

            image.Source = im;
        }
    }
}
