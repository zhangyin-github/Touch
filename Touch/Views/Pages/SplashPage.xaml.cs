﻿using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Touch.Data;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Touch.Views.Pages
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class SplashPage : Page
    {
        /// <summary>
        ///     Variable to hold the splash screen object.
        /// </summary>
        private readonly SplashScreen _splash;

        /// <summary>
        ///     Rect to store splash screen image coordinates.
        /// </summary>
        private Rect _splashImageRect;

        public SplashPage(SplashScreen splashscreen)
        {
            InitializeComponent();

            // Listen for window resize events to reposition the extended splash screen image accordingly.
            // This is important to ensure that the extended splash screen is
            // formatted properly in response to snapping, unsnapping, rotation, etc...
            Window.Current.SizeChanged += (sender, args) =>
            {
                // Safely update the extended splash screen image coordinates.
                // This function will be fired in response to snapping, unsnapping, rotation, etc...
                if (_splash == null)
                    return;
                // Update the coordinates of the splash screen image.
                _splashImageRect = _splash.ImageLocation;
                PositionImage();
                PositionRing();
            };

            _splash = splashscreen;

            if (_splash == null)
                return;
            // Retrieve the window coordinates of the splash screen image.
            _splashImageRect = _splash.ImageLocation;
            // Position the extended splash screen image in the same location as the system splash screen image.
            PositionImage();
            // Optional: Add a progress ring to your splash screen to show users that content is loading
            PositionRing();
        }

        private void PositionImage()
        {
            ExtendedSplashImage.SetValue(Canvas.LeftProperty, _splashImageRect.X);
            ExtendedSplashImage.SetValue(Canvas.TopProperty, _splashImageRect.Y);
            ExtendedSplashImage.Height = _splashImageRect.Height;
            ExtendedSplashImage.Width = _splashImageRect.Width;
        }

        private void PositionRing()
        {
            SplashProgressRing.SetValue(Canvas.LeftProperty,
                _splashImageRect.X + _splashImageRect.Width * 0.5 - SplashProgressRing.Width * 0.5);
            SplashProgressRing.SetValue(Canvas.TopProperty,
                _splashImageRect.Y + _splashImageRect.Height + _splashImageRect.Height * 0.1);
        }

        private async void Splash_OnLoaded(object sender, RoutedEventArgs e)
        {
            // 初始化数据库
            DatabaseHelper.GetInstance();

            await Task.Delay(1000);

            var rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
                return;
            rootFrame.Navigate(typeof(MainPage));
            Window.Current.Content = rootFrame;
        }
    }
}