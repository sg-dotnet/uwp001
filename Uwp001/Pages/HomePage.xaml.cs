using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Controls.Primitives;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Uwp001.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        CanvasCommandList cl;
        GaussianBlurEffect blur;
        HueRotationEffect hueRotation;
        ContrastEffect contrast;
        SaturationEffect saturation;
        TemperatureAndTintEffect temperatureAndTint;
        GrayscaleEffect grayscale;
        ICanvasEffect canvasEffect;

        public HomePage()
        {
            InitializeComponent();
        }

        // The Draw event is raised once when the CanvasControl first becomes visible, then again any time its contents need to be redrawn. 
        // This can occur, for example, if the control is resized. You can manually trigger a Draw event yourself by calling Invalidate().
        //
        // Reference: https://microsoft.github.io/Win2D/html/E_Microsoft_Graphics_Canvas_UI_Xaml_CanvasControl_Draw.htm
        private void canvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            if (canvasEffect != null)
            {
                args.DrawingSession.DrawImage(canvasEffect);
            }
        }

        void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            canvas.RemoveFromVisualTree();
            canvas = null;
        }

        private async void btnUpload_Clicked(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            // Pick only one file once a time
            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file == null)
            {
                return;
            }

            using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
            {
                cl = new CanvasCommandList(canvas);

                using (CanvasDrawingSession clds = cl.CreateDrawingSession())
                {
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
                    clds.DrawImage(
                        CanvasBitmap.CreateFromSoftwareBitmap(canvas, 
                        await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Rgba16, BitmapAlphaMode.Premultiplied)));
                }

                blur = new GaussianBlurEffect
                {
                    Source = cl,
                    BlurAmount = (float)gaussianBlurAmountSlider.Value
                };

                hueRotation = new HueRotationEffect
                {
                    Source = blur,
                    Angle = (float)hueRotationAmountSlider.Value / 100f * 360f
                };

                contrast = new ContrastEffect
                {
                    Source = hueRotation,
                    Contrast = (float)(contrastAmountSlider.Value - 50f) / 50f
                };

                saturation = new SaturationEffect
                {
                    Source = contrast,
                    Saturation = (float)saturationAmountSlider.Value / 100f
                };

                temperatureAndTint = new TemperatureAndTintEffect
                {
                    Source = saturation,
                    Temperature = (float)(temperatureAmountSlider.Value - 50f) / 50f,
                    Tint = (float)(tintAmountSlider.Value - 50f) / 50f
                };

                grayscale = new GrayscaleEffect
                {
                    Source = temperatureAndTint
                };

                canvasEffect = saturation;

                // CanvasControl.Invalidate Method iIndicates that the contents of the CanvasControl need to be redrawn. 
                // Calling Invalidate results in the Draw event being raised shortly afterward.
                //
                // Reference: https://microsoft.github.io/Win2D/html/M_Microsoft_Graphics_Canvas_UI_Xaml_CanvasControl_Invalidate.htm
                canvas.Invalidate();

                gaussianBlurAmountSlider.IsEnabled = true;
                hueRotationAmountSlider.IsEnabled = true;
                contrastAmountSlider.IsEnabled = true;
                saturationAmountSlider.IsEnabled = true;
                temperatureAmountSlider.IsEnabled = true;
                tintAmountSlider.IsEnabled = true;
                grayscaleBufferPrevision.IsEnabled = true;
                
                var image = new BitmapImage();
                await image.SetSourceAsync(fileStream);
            }
        }

        private void GaussianBlurAmountSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            blur.BlurAmount = (float)e.NewValue;
            canvas.Invalidate();
        }

        private void HueRotationAmountSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            hueRotation.Angle = (float)e.NewValue / 100f * 360f;
            canvas.Invalidate();
        }

        private void ContrastAmountSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            contrast.Contrast = (float)(e.NewValue - 50) / 50f;
            canvas.Invalidate();
        }

        private void SaturationAmountSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            saturation.Saturation = (float)e.NewValue / 100f;
            canvas.Invalidate();
        }

        private void TemperatureAmountSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            temperatureAndTint.Temperature = (float)(e.NewValue - 50) / 50f;
            canvas.Invalidate();
        }

        private void TintAmountSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            temperatureAndTint.Tint = (float)(e.NewValue - 50) / 50f;
            canvas.Invalidate();
        }

        private void GrayscaleBufferPrevision0_Click(object sender, RoutedEventArgs e)
        {
            canvasEffect = temperatureAndTint;
            canvas.Invalidate();
        }

        private void GrayscaleBufferPrevision1_Click(object sender, RoutedEventArgs e)
        {
            grayscale.BufferPrecision = CanvasBufferPrecision.Precision16Float;
            canvasEffect = grayscale;
            canvas.Invalidate();
        }

        private void GrayscaleBufferPrevision2_Click(object sender, RoutedEventArgs e)
        {
            grayscale.BufferPrecision = CanvasBufferPrecision.Precision32Float;
            canvasEffect = grayscale;
            canvas.Invalidate();
        }

        private void GrayscaleBufferPrevision3_Click(object sender, RoutedEventArgs e)
        {
            grayscale.BufferPrecision = CanvasBufferPrecision.Precision8UIntNormalized;
            canvasEffect = grayscale;
            canvas.Invalidate();
        }
    }
}
