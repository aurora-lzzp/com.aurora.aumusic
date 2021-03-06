﻿//Copyright(C) 2015 Aurora Studio

//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
//to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
//and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
//WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.



/// <summary>
/// Usings
/// </summary>
using com.aurora.aumusic.shared.Albums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace com.aurora.aumusic.shared.Songs
{
    public class ShuffleList
    {
        public const int FAV_LIST_CAPACITY = 20;

        List<Song> AllSongs = new List<Song>();
        public ShuffleList(List<AlbumItem> Albums)
        {
            foreach (var item in Albums)
            {
                AllSongs.AddRange(item.Songs);
            }
        }
        public List<Song> GenerateNewList(int count)
        {
            if (AllSongs == null || AllSongs.Count == 0)
                return null;
            Random r = new Random(Guid.NewGuid().GetHashCode());
            List<Song> shuffleList = new List<Song>();
            for (int i = 0; i < count; i++)
            {
                shuffleList.Add(AllSongs[r.Next(AllSongs.Count)]);
            }
            return shuffleList;

        }
        public List<Song> GenerateFavouriteList()
        {
            AllSongs.Sort((first, second) =>
            {
                return -first.PlayTimes.CompareTo(second.PlayTimes);
            });
            List<Song> favList = new List<Song>();
            if (AllSongs.Count > FAV_LIST_CAPACITY)
                favList.AddRange(AllSongs.GetRange(0, FAV_LIST_CAPACITY));
            else
                favList.AddRange(AllSongs);
            List<Song> list = new List<Song>();
            foreach (var item in favList)
            {
                if (item.PlayTimes == 0)
                {
                    continue;
                }
                list.Add(item);
            }
            return list;
        }

        public static void Save(Song song)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            ApplicationDataContainer MainContainer =
    localSettings.CreateContainer(song.FolderToken, ApplicationDataCreateDisposition.Always);
            ApplicationDataContainer SubContainer =
    MainContainer.CreateContainer("Album" + song.Position, ApplicationDataCreateDisposition.Always);
            ApplicationDataContainer triContainer =
    SubContainer.CreateContainer("Song" + song.SubPosition, ApplicationDataCreateDisposition.Always);
            triContainer.Values["PlayTimes"] = song.PlayTimes;
        }

        public static void Save(SongModel song)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            ApplicationDataContainer MainContainer =
    localSettings.CreateContainer(song.FolderToken, ApplicationDataCreateDisposition.Always);
            ApplicationDataContainer SubContainer =
    MainContainer.CreateContainer("Album" + song.Position, ApplicationDataCreateDisposition.Always);
            ApplicationDataContainer triContainer =
    SubContainer.CreateContainer("Song" + song.SubPosition, ApplicationDataCreateDisposition.Always);
            triContainer.Values["PlayTimes"] = song.PlayTimes;
        }

        public static void Rate(SongModel song, uint Rating)
        {
            song.Rating = Rating;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            ApplicationDataContainer MainContainer =
    localSettings.CreateContainer(song.FolderToken, ApplicationDataCreateDisposition.Always);
            ApplicationDataContainer SubContainer =
    MainContainer.CreateContainer("Album" + song.Position, ApplicationDataCreateDisposition.Always);
            ApplicationDataContainer triContainer =
    SubContainer.CreateContainer("Song" + song.SubPosition, ApplicationDataCreateDisposition.Always);
            triContainer.Values["Rating"] = song.Rating;
        }

        public static void Rate(Song song, uint Rating)
        {
            song.Rating = Rating;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            ApplicationDataContainer MainContainer =
    localSettings.CreateContainer(song.FolderToken, ApplicationDataCreateDisposition.Always);
            ApplicationDataContainer SubContainer =
    MainContainer.CreateContainer("Album" + song.Position, ApplicationDataCreateDisposition.Always);
            ApplicationDataContainer triContainer =
    SubContainer.CreateContainer("Song" + song.SubPosition, ApplicationDataCreateDisposition.Always);
            triContainer.Values["Rating"] = song.Rating;
        }

        public static void SaveFavouriteList(List<Song> favList)
        {
            if (favList == null || favList.Count == 0)
                return;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            ApplicationDataContainer MainContainer =
    localSettings.CreateContainer("FavouriteList", ApplicationDataCreateDisposition.Always);
            int i = 0;
            foreach (var item in favList)
            {
                ApplicationDataContainer SubContainer =
                    MainContainer.CreateContainer("FavSong" + i, ApplicationDataCreateDisposition.Always);
                SubContainer.Values["FolderToken"] = item.FolderToken;
                SubContainer.Values["Position"] = item.Position;
                SubContainer.Values["SubPosition"] = item.SubPosition;
                SubContainer.Values["Key"] = item.MainKey;
                i++;
            }
            MainContainer.Values["FavSongsCount"] = i;
        }
        public static async Task<List<Song>> RestoreFavouriteList()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            ApplicationDataContainer MainContainer =
    localSettings.CreateContainer("FavouriteList", ApplicationDataCreateDisposition.Always);
            int i;
            try
            {
                i = (int)MainContainer.Values["FavSongsCount"];
                List<Song> favList = new List<Song>();
                for (int j = 0; j < i; j++)
                {
                    ApplicationDataContainer SubContainer =
                         MainContainer.CreateContainer("FavSong" + j, ApplicationDataCreateDisposition.Always);
                    ApplicationDataContainer FolderContainer =
                        localSettings.CreateContainer((string)SubContainer.Values["FolderToken"], ApplicationDataCreateDisposition.Always);
                    ApplicationDataContainer AlbumContainer =
                        FolderContainer.CreateContainer("Album" + (int)SubContainer.Values["Position"], ApplicationDataCreateDisposition.Always);
                    string key = (string)SubContainer.Values["key"];
                    //StorageFile file;
                    //try
                    //{
                    //    file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(key);
                    //    StorageApplicationPermissions.FutureAccessList.Remove(key);
                    //}
                    //catch (Exception)
                    //{
                    //    continue;
                    //}
                    Song tempSong = Song.RestoreSongfromStorage(AlbumContainer, (int)SubContainer.Values["SubPosition"]);
                    //tempSong.AudioFile = file;
                    tempSong.Position = (int)SubContainer.Values["Position"];
                    tempSong.SubPosition = (int)SubContainer.Values["SubPosition"];
                    favList.Add(tempSong);
                }
                return favList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void SaveShuffleList(List<Song> songs)
        {
            if(songs == null)
            {
                return;
            }
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            ApplicationDataContainer MainContainer =
    localSettings.CreateContainer("ShuffleList", ApplicationDataCreateDisposition.Always);
            int i = 0;
            foreach (var item in songs)
            {
                ApplicationDataContainer SubContainer =
                    MainContainer.CreateContainer("ShuSong" + i, ApplicationDataCreateDisposition.Always);
                SubContainer.Values["FolderToken"] = item.FolderToken;
                SubContainer.Values["Position"] = item.Position;
                SubContainer.Values["SubPosition"] = item.SubPosition;
                SubContainer.Values["Key"] = item.MainKey;
                i++;
            }
            MainContainer.Values["ShuSongsCount"] = i;
        }

        public static List<Song> RestoreShuffleList()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            try
            {
                ApplicationDataContainer MainContainer =
                   localSettings.CreateContainer("ShuffleList", ApplicationDataCreateDisposition.Always);
                int i = (int)MainContainer.Values["ShuSongsCount"];
                List<Song> songs = new List<Song>();
                for (int j = 0; j < i; j++)
                {
                    ApplicationDataContainer SubContainer =
                        MainContainer.CreateContainer("ShuSong" + j, ApplicationDataCreateDisposition.Always);
                    ApplicationDataContainer FolderContainer =
                       localSettings.CreateContainer((string)SubContainer.Values["FolderToken"], ApplicationDataCreateDisposition.Always);
                    ApplicationDataContainer AlbumContainer =
                        FolderContainer.CreateContainer("Album" + (int)SubContainer.Values["Position"], ApplicationDataCreateDisposition.Always);
                    string key = (string)SubContainer.Values["key"];
                    //StorageFile file;
                    //try
                    //{
                    //    file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(key);
                    //    StorageApplicationPermissions.FutureAccessList.Remove(key);
                    //}
                    //catch (Exception)
                    //{
                    //    continue;
                    //}
                    Song tempSong = Song.RestoreSongfromStorage(AlbumContainer, (int)SubContainer.Values["SubPosition"]);
                    //tempSong.AudioFile = file;
                    tempSong.Position = (int)SubContainer.Values["Position"];
                    tempSong.SubPosition = (int)SubContainer.Values["SubPosition"];
                    songs.Add(tempSong);
                }
                return songs;
            }
            catch (Exception)
            {
                return null;
            }

        }

        public static List<Song> ShuffleIt(List<Song> shuffleList)
        {
            if (shuffleList == null || shuffleList.Count == 0)
                return null;
            if (shuffleList.Count <= FAV_LIST_CAPACITY)
                return shuffleList;
            Random r = new Random(Guid.NewGuid().GetHashCode());
            List<Song> ts = new List<Song>();
            for (int i = 0; i < FAV_LIST_CAPACITY; i++)
            {
                var s = r.Next(shuffleList.Count);
                ts.Add(shuffleList[s]);
                shuffleList.RemoveAt(s);
            }
            return ts;
        }

        public static void Love(SongModel currentSong, bool loved)
        {
            currentSong.Loved = loved;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            ApplicationDataContainer MainContainer =
    localSettings.CreateContainer(currentSong.FolderToken, ApplicationDataCreateDisposition.Always);
            ApplicationDataContainer SubContainer =
    MainContainer.CreateContainer("Album" + currentSong.Position, ApplicationDataCreateDisposition.Always);
            ApplicationDataContainer triContainer =
    SubContainer.CreateContainer("Song" + currentSong.SubPosition, ApplicationDataCreateDisposition.Always);
            triContainer.Values["Loved"] = currentSong.Loved;
        }
    }
}

