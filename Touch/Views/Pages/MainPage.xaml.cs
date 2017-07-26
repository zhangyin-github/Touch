﻿using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Touch.Views.Pages
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            // 刷新button点击事件
            RefreshButton.Click += async (sender, args) => { await GalleryGridViewControl.RefreshAsync(); };
            // 添加回忆点击事件
            CreateMemoryButton.Click += (sender, args) =>
            {
                var rootFrame = Window.Current.Content as Frame;
                rootFrame?.Navigate(typeof(CreateMemoryPage), MemoryGridViewControl.MemoryListView);
                Window.Current.Content = rootFrame;
            };
            // 设置button点击事件
            SettingButton.Click += (sender, args) =>
            {
                var rootFrame = Window.Current.Content as Frame;
                rootFrame?.Navigate(typeof(SettingPage));
                Window.Current.Content = rootFrame;
            };
        }
    }
}