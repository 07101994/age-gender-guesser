using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Windows.Media.Capture;
using Windows.UI.Core;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Media.MediaProperties;
using Windows.Graphics.Imaging;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Win2D_Face
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MediaCapture mediaCapture;
        private IRandomAccessStream capturedPhoto;
        private TaskCompletionSource<object> hasLoaded = new TaskCompletionSource<object>();

        // Win2D stuff
        CanvasBitmap photoCanvasBitmap;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            var action = mediaCapture.StopPreviewAsync();
            mediaCapture.Failed -= mediaCapture_Failed;
        }

        private void mediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            var action = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                progressText.Text = "MediaCapture failed: " + errorEventArgs.Message;
                progressText.Visibility = Visibility.Visible;
            });
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.captureElement.Visibility = Visibility.Collapsed;

            await CreateMediaCapture();

            this.captureElement.Visibility = Visibility.Visible;

            hasLoaded.SetResult(null);
        }

        private async Task CreateMediaCapture()
        {
            mediaCapture = new MediaCapture();
            mediaCapture.Failed += mediaCapture_Failed;

            var settings = new MediaCaptureInitializationSettings()
            {
                StreamingCaptureMode = StreamingCaptureMode.Video
            };

            try
            {
                await mediaCapture.InitializeAsync(settings);
            }
            catch (Exception)
            {
                this.progressText.Text = "No camera is available.";

                return;
            }

            captureElement.Source = mediaCapture;
            await mediaCapture.StartPreviewAsync();

            var photoStreamProperties = mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.Photo);
            IMediaEncodingProperties mediaEncodingProperties = null;


            foreach (var photoStreamProperty in photoStreamProperties)
            {
                var videoEncodingProperties = (photoStreamProperty as VideoEncodingProperties);
                if (videoEncodingProperties != null)
                {
                    if (videoEncodingProperties.Width * videoEncodingProperties.Height * 4 <= 4 * 640 * 480)
                    {
                        mediaEncodingProperties = photoStreamProperty;
                    }
                }
            }

            await mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.Photo, mediaEncodingProperties);
        }

        private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            // Make the 'Processing...' label visible
            canvasControl.Visibility = Visibility.Visible;
            canvasControl.Invalidate();

            capturedPhoto = new InMemoryRandomAccessStream();
            var tempPhoto = new InMemoryRandomAccessStream();
            await mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), tempPhoto);
            await tempPhoto.FlushAsync();

            captureElement.Visibility = Visibility.Collapsed;

            // Project Oxford doesn't like the JPEG that CapturePhotoToStreamAsync creates
            // It returns an "Image size is too small" error
            // To workaround this, we have to decode the JPEG, and reencode it again using Windows.Graphics.Imaging
            // Project Oxford successfully detects faces in the reencoded image

            // Decode the image created by CapturePhotoToStreamAsync
            BitmapPixelFormat pixelFormat = BitmapPixelFormat.Rgba8;
            BitmapAlphaMode alphaMode = BitmapAlphaMode.Ignore;
            var decoder = await BitmapDecoder.CreateAsync(tempPhoto);
            var decodedPixelData = await decoder.GetPixelDataAsync(pixelFormat, alphaMode, new BitmapTransform(), ExifOrientationMode.RespectExifOrientation, ColorManagementMode.DoNotColorManage);

            // Reencode the image
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, capturedPhoto);
            encoder.SetPixelData(pixelFormat, alphaMode, decoder.PixelWidth, decoder.PixelHeight, decoder.DpiX, decoder.DpiY, decodedPixelData.DetachPixelData());
            await encoder.FlushAsync();

            // Store the captured photo as a Win2D type for later use
            // Note that this has to come before calls to Project Oxford
            // Project Oxford appears to close the stream when it's finished with it, meaning Win2D can't access it
            photoCanvasBitmap = await CanvasBitmap.LoadAsync(canvasControl, capturedPhoto);

            // Send the photo to Project Oxford to detect the faces
            var faces = await faceServiceClient.DetectAsync(capturedPhoto.AsStreamForRead());

            // Force the canvasControl to be redrawn now that the photo is available
            canvasControl.Invalidate();
        }

        void canvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (photoCanvasBitmap != null)
            {
                CanvasRenderTarget tempRenderTarget = new CanvasRenderTarget(sender, photoCanvasBitmap.Size);

                // Begin by drawing the captured photo into the temporary render target
                using (CanvasDrawingSession ds = tempRenderTarget.CreateDrawingSession())
                {
                    ds.DrawImage(photoCanvasBitmap, new System.Numerics.Vector2(0, 0));
                }

                // End by drawing the rendertarget into the center of the screen
                double imageScale = Math.Min(sender.RenderSize.Width / photoCanvasBitmap.Size.Width, sender.RenderSize.Height / tempRenderTarget.Size.Height);
                double newWidth = imageScale * tempRenderTarget.Size.Width;
                double newHeight = imageScale * tempRenderTarget.Size.Height;
                Rect targetRect = new Rect((sender.RenderSize.Width - newWidth) / 2, (sender.RenderSize.Height - newHeight) / 2, newWidth, newHeight);

                args.DrawingSession.DrawImage(tempRenderTarget, targetRect);
                
            }
            else
            {
                args.DrawingSession.DrawText("Processing...", (float)(sender.Size.Width / 2), (float)(sender.Size.Height / 2), Colors.White);
            }
        }

        private readonly IFaceServiceClient faceServiceClient =
            new FaceServiceClient("Your subscription key");
    }
}
