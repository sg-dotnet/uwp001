using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Uwp001.Controls
{
    public sealed partial class BlurredBackground : UserControl
    {
        public BlurredBackground()
        {
            InitializeComponent();
        }

        internal void UpdateImage(ImageSource imageSource) =>
           BackgroundImage.Source = imageSource;
    }
}
