using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Uwp001.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        CanvasCommandList cl;
        public HomePage()
        {
            this.InitializeComponent();
        }

        // The Draw event is raised once when the CanvasControl first becomes visible, then again any time its contents need to be redrawn. 
        // This can occur, for example, if the control is resized. You can manually trigger a Draw event yourself by calling Invalidate().
        //
        // Reference: https://microsoft.github.io/Win2D/html/E_Microsoft_Graphics_Canvas_UI_Xaml_CanvasControl_Draw.htm
        private void canvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            //CanvasCommandList cl = new CanvasCommandList(sender);
            //using (CanvasDrawingSession clds = cl.CreateDrawingSession())
            //{
            //    for (int i = 0; i < 100; i++)
            //    {
            //        clds.DrawText("Hello, World!", RndPosition(), Color.FromArgb(255, RndByte(), RndByte(), RndByte()));
            //        clds.DrawCircle(RndPosition(), RndRadius(), Color.FromArgb(255, RndByte(), RndByte(), RndByte()));
            //        clds.DrawLine(RndPosition(), RndPosition(), Color.FromArgb(255, RndByte(), RndByte(), RndByte()));
            //    }
            //}

            //GaussianBlurEffect blur = new GaussianBlurEffect();
            //blur.Source = cl;
            //blur.BlurAmount = 10.0f;
            //args.DrawingSession.DrawImage(blur);
            if (contrast != null)
            {
                args.DrawingSession.DrawImage(contrast);
            }
        }

        GaussianBlurEffect blur;
        HueRotationEffect hueRotation;
        ContrastEffect contrast;
        private void canvas_CreateResources(
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender,
            Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            //CanvasCommandList cl = new CanvasCommandList(sender);
            //using (CanvasDrawingSession clds = cl.CreateDrawingSession())
            //{
            //    for (int i = 0; i < 100; i++)
            //    {
            //        clds.DrawText("Hello, World!", RndPosition(), Color.FromArgb(255, RndByte(), RndByte(), RndByte()));
            //        clds.DrawCircle(RndPosition(), RndRadius(), Color.FromArgb(255, RndByte(), RndByte(), RndByte()));
            //        clds.DrawLine(RndPosition(), RndPosition(), Color.FromArgb(255, RndByte(), RndByte(), RndByte()));
            //    }
            //}

            //blur = new GaussianBlurEffect()
            //{
            //    Source = cl,
            //    BlurAmount = 10.0f
            //};
        }

        private void canvas_DrawAnimated(
    Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender,
    Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
        {
            //float radius = (float)(1 + Math.Sin(args.Timing.TotalTime.TotalSeconds)) * 10f;
            //blur.BlurAmount = radius;
        }

        void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            canvas.RemoveFromVisualTree();
            canvas = null;
        }

        Random rnd = new Random();
        private Vector2 RndPosition()
        {
            double x = rnd.NextDouble() * 1000f;
            double y = rnd.NextDouble() * 1000f;
            return new Vector2((float)x, (float)y);
        }

        private float RndRadius()
        {
            return (float)rnd.NextDouble() * 500f;
        }

        private byte RndByte()
        {
            return (byte)rnd.Next(256);
        }

        private async void btnUpload_Clicked(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            //pick only one file once a time
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

                blur = new GaussianBlurEffect();
                blur.Source = cl;
                blur.BlurAmount = (float)gaussianBlurAmountSlider.Value;

                hueRotation = new HueRotationEffect();
                hueRotation.Source = blur;
                hueRotation.Angle = (float)hueRotationAmountSlider.Value / 100f * 360f;

                contrast = new ContrastEffect();
                contrast.Source = hueRotation;
                contrast.Contrast = (float)(contrastAmountSlider.Value - 50) / 50;

                // CanvasControl.Invalidate Method iIndicates that the contents of the CanvasControl need to be redrawn. 
                // Calling Invalidate results in the Draw event being raised shortly afterward.
                //
                // Reference: https://microsoft.github.io/Win2D/html/M_Microsoft_Graphics_Canvas_UI_Xaml_CanvasControl_Invalidate.htm
                canvas.Invalidate();

                gaussianBlurAmountSlider.IsEnabled = true;
                hueRotationAmountSlider.IsEnabled = true;
                contrastAmountSlider.IsEnabled = true;
                
                var image = new BitmapImage();
                await image.SetSourceAsync(fileStream);
                BlurredBackground.UpdateImage(image);
            }
        }

        private void GaussianBlurAmountSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            blur.BlurAmount = (float)e.NewValue;
            canvas.Invalidate();
        }

        private void HueRotationAmountSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            hueRotation.Angle = (float)e.NewValue / 100f * 360f;
            canvas.Invalidate();
        }

        private void ContrastAmountSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            contrast.Contrast = (float)(e.NewValue - 50) / 50f;
            canvas.Invalidate();
        }
    }
}
