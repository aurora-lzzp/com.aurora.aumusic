﻿using System;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using System.Collections.Generic;


//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace com.aurora.aumusic
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        SplitListView splitlistview;
        private static int BUTTON_CLICKED = 0;
        PlaybackPack playbackPack = new PlaybackPack();
        PlayBack playBack = new PlayBack();
        TextBlock TimeElapsedBlock;
        TextBlock TimeTotalBlock;
        Slider ProgressSlider;
        Slider VolumeSlider;

        public MainPage()
        {
            this.InitializeComponent();
            //设置汉堡按钮控制
            //默认打开MymusicPage
            splitlistview = new SplitListView();
            SplitViewSources.Source = splitlistview;
            //MainFrame.Navigate(typeof(SettingsPage));
        }

        private bool TimeTask(TimeSpan delay, bool completed)
        {
            ThreadPoolTimer DelayTimer = ThreadPoolTimer.CreateTimer(
async (source) =>
{
    await

            Dispatcher.RunAsync(
                                CoreDispatcherPriority.High,
                                () =>
                                {
                                    TimeSpan ts = PlaybackControl.Position;
                                    if (ts.Seconds >= 10)
                                    {
                                        string s = ((ts.Days * 24) + ts.Hours) * 60 + ts.Minutes + ":" + ts.Seconds;
                                        TimeElapsedBlock.Text = s;
                                        return;
                                    }
                                    else
                                    {
                                        string s = ((ts.Days * 24) + ts.Hours) * 60 + ts.Minutes + ":0" + ts.Seconds;
                                        TimeElapsedBlock.Text = s;
                                        return;
                                    }
                                });

    completed = true;
},
                delay,
async (source) =>
{
    await

            Dispatcher.RunAsync(
                            CoreDispatcherPriority.High,
                            () =>
                            {

                                if (completed)
                                {
                                    completed = TimeTask(delay, completed);
                                }
                                else
                                {
                                }

                            });
});
            return completed;
        }

        private void Menubtn_Click(object sender, RoutedEventArgs e)
        {
            //菜单按钮点击定义
            Menudrawer.IsPaneOpen = !Menudrawer.IsPaneOpen;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (BUTTON_CLICKED == 0)
            {
                BUTTON_CLICKED = 1;
                this.MainFrame.Navigate(typeof(SettingsPage));
                SettingsButton.IsEnabled = false;
                MenuList.SelectedItem = null;
            }
        }



        private async void HorizontalThumb_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (playBack.NowPlaying() != null)
            {
                if ((PlaybackControl.Position + TimeSpan.FromMilliseconds(500)) >= playBack.NowPlaying().Duration)
                {
                    await playBack.PlayNext(PlaybackControl);
                }
                else
                {
                    PlaybackControl.Play();
                }
            }
            PlaybackControl.MediaEnded += SetMediaEnd;
        }
        private async void SetMediaEnd(object sender, RoutedEventArgs e)
        {
            AlbumFlowPage a = MainFrame.Content as AlbumFlowPage;
            await playBack.PlayNext(this.PlaybackControl);
        }

        private void ellipse_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            PlaybackControl.MediaEnded -= SetMediaEnd;
        }

        private void PlaybackControl_MediaOpened(object sender, RoutedEventArgs e)
        {
            Song s = playBack.NowPlaying();
            TimeSpan ts = s.Duration;
            if (ts.Seconds >= 10)
            {
                string p = ts.Hours * 60 + ts.Minutes + ":" + ts.Seconds;
                TimeTotalBlock.Text = p;
            }
            else
            {
                string p = ts.Hours * 60 + ts.Minutes + ":0" + ts.Seconds;
                TimeTotalBlock.Text = p;
            }
            BitmapImage b = s != null ? new BitmapImage(new Uri(s.ArtWork)) : new BitmapImage(new Uri("ms-appx:///Assets/unknown.png"));
            PlayBackImage.Source = b;
            bool completed = false;
            TimeSpan delay = TimeSpan.FromMilliseconds(100);
            completed = TimeTask(delay, completed);
            ThumbToolTipConveter thumbConverter = (ThumbToolTipConveter)ProgressSlider.ThumbToolTipValueConverter;
            thumbConverter.sParmeter = ts.TotalSeconds;
        }

        private void MenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView l = sender as ListView;
            foreach (var item in splitlistview)
            {
                item.visibility = Visibility.Collapsed;
            }
            if (l.SelectedIndex != -1)
            {
                BUTTON_CLICKED = 0;
                SettingsButton.IsEnabled = true;
                Splitlist s = l.SelectedItem as Splitlist;
                s.visibility = Visibility.Visible;
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                switch (s.Title)
                {
                    case "AlbumFlow":
                        if (localSettings.Values.ContainsKey("FolderSettings"))
                        {
                            ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)localSettings.Values["FolderSettings"];
                            if (composite != null)
                            {
                                playbackPack.Media = PlaybackControl;
                                playbackPack.PlaybackControl = playBack;
                                playbackPack.States = PLAYBACK_STATES.Null;
                                MainFrame.Navigate(typeof(AlbumFlowPage), playbackPack); break;
                            }
                        }
                        MainFrame.Navigate(typeof(SettingsPage)); l.SelectedIndex = -1; break;
                    case "Artists": MainFrame.Navigate(typeof(ArtistPage)); break;
                    case "Songs": MainFrame.Navigate(typeof(SongsPage)); break;
                    case "Song Lists": MainFrame.Navigate(typeof(ListPage)); break;
                }
            }
        }

        private void TimeElapsedBlock_Loaded(object sender, RoutedEventArgs e)
        {
            TimeElapsedBlock = sender as TextBlock;
        }

        private void TimeRemainingBlock_Loaded(object sender, RoutedEventArgs e)
        {
            TimeTotalBlock = sender as TextBlock;
        }

        private void ProgressSlider_Loaded(object sender, RoutedEventArgs e)
        {
            ProgressSlider = sender as Slider;
        }

        private async void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (playBack.NowPlaying() != null)
            {
                if (PlaybackControl.Position <= TimeSpan.FromSeconds((playBack.NowPlaying().Duration.TotalSeconds / 99)))
                {
                    await playBack.PlayPrevious(PlaybackControl);
                }
                else
                {
                    PlaybackControl.Position = TimeSpan.FromSeconds(0);
                    PlaybackControl.Play();
                }
            }
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (playBack.NowPlaying() != null)
            {
                await playBack.PlayNext(PlaybackControl);
            }
        }



        private void VolumeSlider_Loaded(object sender, RoutedEventArgs e)
        {
            VolumeSlider = sender as Slider;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("volume"))
            {
                VolumeSlider.Value = (double)localSettings.Values["Volume"];
            }
        }

        private void VolumeFlyout_Closed(object sender, object e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["Volume"] = VolumeSlider.Value;
        }
    }
}
