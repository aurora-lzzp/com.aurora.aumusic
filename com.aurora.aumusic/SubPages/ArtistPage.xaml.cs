﻿using System;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;
using com.aurora.aumusic.shared.Songs;
using com.aurora.aumusic.shared.MessageService;
using com.aurora.aumusic.shared.Albums;
using System.Collections.ObjectModel;
using com.aurora.aumusic.shared.Helpers;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using com.aurora.aumusic.shared;

namespace com.aurora.aumusic
{
    public sealed partial class ArtistPage : Page
    {
        public static List<AlbumItem> AllSongs;
        ObservableCollection<ArtistsKeyGroup<AlbumItem>> ArtistsGroupViewModel = new ObservableCollection<ArtistsKeyGroup<AlbumItem>>();

        public ArtistPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            App.ResetTitleBar();
            ArtistsSource.Source = ArtistsGroupViewModel;
            ArtistDetailsSource.Source = null;
        }



        private async void LoadingRing_Loaded(object sender, RoutedEventArgs e)
        {
            await ThreadPool.RunAsync((work) =>
            {
                if (AllSongs != null)
                {
                    var query = ArtistsKeyGroup<AlbumItem>.CreateGroups(AllSongs, album => album.AlbumArtists, true);
                    this.Dispatcher.RunAsync(CoreDispatcherPriority.High, new DispatchedHandler(() =>
                     {
                         ArtistsGroupViewModel.Clear();
                         foreach (var g in query)
                         {
                             List<string> artworks = new List<string>();
                             foreach (var item in g)
                             {
                                 artworks.Add(item.AlbumArtWork);
                             }
                             g.SetArtworks(artworks.ToArray());
                             ArtistsGroupViewModel.Add(g);
                         }
                     }));

                }
            });
            LoadingRing.IsActive = false;
            LoadingRing.Visibility = Visibility.Collapsed;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var album = ((sender as Button).DataContext as Song).Parent as AlbumItem;
            MessageService.SendMessageToBackground(new ForePlaybackChangedMessage(PlaybackState.Playing, album.ToSongModelList(), new SongModel((sender as Button).DataContext as Song)));
        }

        private void SemanticZoom_ViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (SemanticZoom.IsZoomedInViewActive)
            {
                SystemNavigationManager.GetForCurrentView().BackRequested += Zoom_BackRequested;
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                var list = AlbumSongsGroup.CreateGroup(e.SourceItem.Item as ArtistsKeyGroup<AlbumItem>, true);
                if (list == null)
                    SemanticZoom.IsZoomedInViewActive = false;
                ArtistDetailsSource.Source = list;
                ArtistDetailedView.ItemsSource = ArtistDetailsSource.View;
                SemanticZoom.CanChangeViews = false;
                var ArtistArtworkGroups = ArtistDetailedView.FindChildControl<RelativePanel>("ArtistArtworkGroups") as RelativePanel;
                var ArtistArtworkGroup0 = ArtistDetailedView.FindChildControl<RelativePanel>("ArtistArtworkGroup0") as RelativePanel;
                var ArtistArtworkGroup1 = ArtistDetailedView.FindChildControl<RelativePanel>("ArtistArtworkGroup1") as RelativePanel;

                var ArtistName = ArtistDetailedView.FindChildControl<TextBlock>("ArtistName") as TextBlock;
                var ArtistDetails = ArtistDetailedView.FindChildControl<TextBlock>("ArtistDetails") as TextBlock;

                var artistsconverter = new ArtistsConverter();
                var artistdetailsconverter = new ArtistDetailsConverter();
                var artistdetails = artistdetailsconverter.Convert(list, null, null, null);
                var artists = artistsconverter.Convert(list[0].AlbumArtists, null, true, null);
                ArtistDetails.Text = (string)artistdetails;
                ArtistName.Text = (string)artists;
                var imagelist = ArtistArtworkGroups.GetImages();
                foreach (var image in imagelist)
                {
                    image.Source = null;
                }
                int i = 0;
                if (list.Count < 5)
                    ArtistArtworkGroup0.Height = 420;
                else if (list.Count < 9)
                {
                    ArtistArtworkGroup0.Height = 240;
                    ArtistArtworkGroup1.Height = 240;
                }
                foreach (var album in list)
                {
                    if (imagelist.Count == i)
                        break;
                    imagelist[i].Source = new BitmapImage(new Uri(album.AlbumArtWork));
                    i++;
                }
            }
        }

        private void Zoom_BackRequested(object sender, BackRequestedEventArgs e)
        {
            SemanticZoom.CanChangeViews = true;
            SemanticZoom.IsZoomedInViewActive = false;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= Zoom_BackRequested;
        }

        private void GroupPlayButton_Click(object sender, RoutedEventArgs e)
        {
            var list = new List<SongModel>();
            foreach (var item in ((sender as Button).DataContext as ArtistsKeyGroup<AlbumItem>))
            {
                list.AddRange(item.ToSongModelList());
            }
            MessageService.SendMessageToBackground(new ForePlaybackChangedMessage(PlaybackState.Playing, list, list[0]));
        }

        private void RelativePanel_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            //TODO:
        }

        private void ArtistPlayButton_Click(object sender, RoutedEventArgs e)
        {
            var list = new List<SongModel>();
            foreach (var item in (ArtistDetailsSource.Source as List<AlbumSongsGroup>))
            {
                var album = item[0].Parent;
                list.AddRange(album.ToSongModelList());
            }
            MessageService.SendMessageToBackground(new ForePlaybackChangedMessage(PlaybackState.Playing, list, list[0]));
        }
    }
}
