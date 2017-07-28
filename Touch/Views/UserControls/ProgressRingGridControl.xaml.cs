﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Touch.Views.UserControls
{
    public sealed partial class ProgressRingGridControl : UserControl
    {
        public ProgressRingGridControl()
        {
            InitializeComponent();
            ToggleAnimation(false);
        }

        /// <summary>
        /// 显示
        /// </summary>
        public void Show()
        {
            ToggleAnimation(true);
        }

        /// <summary>
        /// 隐藏
        /// </summary>
        public void Hide()
        {
            ToggleAnimation(false);
        }

        // TODO 复用
        private void ToggleAnimation(bool show)
        {
            var _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            var _detailGridVisual = ElementCompositionPreview.GetElementVisual(RootGrid);

            var fadeAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeAnimation.InsertKeyFrame(1f, show ? 1f : 0f);
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(700);
            
            _detailGridVisual.StartAnimation("Opacity", fadeAnimation);
        }
    }
}
