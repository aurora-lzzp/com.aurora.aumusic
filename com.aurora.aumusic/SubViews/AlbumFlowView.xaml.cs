﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace com.aurora.aumusic
{
    public sealed partial class AlbumFlowView : Page
    {
        public AlbumFlowView()
        {
            this.InitializeComponent();
            AlbumsFlowControls.ItemsSource = Enumerable.Range(0, 30).Select(i => new AlbumItem { Text = i.ToString() });
        }
    }
}