﻿
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using System.Text;
using System.Linq;
using Windows.Storage;
using System;
using Windows.Storage.AccessCache;
using TagLib;
using System.IO;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;

namespace com.aurora.aumusic
{
    public class Song
    {
        private static readonly char[] InvalidFileNameChars = new[] { '"', '<', '>', '|', '\0', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\a', '\b', '\t', '\n', '\v', '\f', '\r', '\u000e', '\u000f', '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c', '\u001d', '\u001e', '\u001f', ':', '*', '?', '\\', '/' };
        private static readonly string[] tempTypeStrings = new[] { ".mp3", ".m4a", ".flac", ".wav" };
        private string _title;
        private string _album;
        private string[] _artists;
        public string[] Artists
        {
            get
            {
                return _artists;
            }
            set
            {
                _artists = value;
            }
        }
        private string[] _albumartists;
        public string[] AlbumArtists
        {
            get
            {
                return _albumartists;
            }
            set
            {
                _albumartists = value;
            }
        }
        private string _artWork;
        private string _artWorkName;
        public Size ArtWorkSize = new Size();
        public string ArtWorkName
        {
            get
            {
                return _artWorkName;
            }
            set
            {
                value += "";
                value = InvalidFileNameChars.Aggregate(value, (current, c) => current.Replace(c + "", "_"));
                _artWorkName = value;
            }
        }
        public string ArtWork
        {
            get
            {
                return _artWork;
            }
            set
            {
                _artWork = (value == null) ? "ms-appx:///Assets/unknown.png" : value;
                ArtWorkSize = new Size(400, 400);
            }
        }
        public uint Rating = 0;
        public string MainKey = null;
        public StorageFile AudioFile = null;
        public uint Disc = 0;
        public uint DiscCount = 0;
        public string[] _genres;
        public string[] Genres
        {
            get
            {
                return _genres;
            }
            set
            {
                _genres = value;
            }
        }
        public uint Track = 0;
        public uint TrackCount = 0;
        public uint Year = 0;

        public string Album
        {
            get
            {
                return _album;
            }
            set
            {
                _album = (value == null) ? "Unknown Album" : value;
            }
        }
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = (value == null) ? AudioFile.DisplayName : value;
            }
        }

        internal static Song RestoreSongfromStorage(int songsCount, List<IStorageFile> allList, ApplicationDataContainer SubContainer)
        {
            Song tempSong = new Song();
            try
            {
                string key = (string)SubContainer.Values["MainKey"];
                string foldertoken = (string)SubContainer.Values["FolderToken"];
                tempSong.MainKey = key;
                tempSong.FolderToken = foldertoken;
                foreach (var item in allList)
                {
                    if (tempSong.MainKey == ((StorageFile)item).FolderRelativeId)
                    {
                        tempSong.AudioFile = (StorageFile)item;
                        break;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            tempSong.Rating = (uint)SubContainer.Values["Rating"];
            tempSong.Title = (string)SubContainer.Values["Title"];
            tempSong.ArtWork = (string)SubContainer.Values["ArtWork"];
            tempSong.Album = (string)SubContainer.Values["Album"];
            tempSong.Year = (uint)SubContainer.Values["Year"];
            tempSong.Disc = (uint)SubContainer.Values["Disc"];
            tempSong.DiscCount = (uint)SubContainer.Values["DiscCount"];
            tempSong.Track = (uint)SubContainer.Values["Track"];
            tempSong.TrackCount = (uint)SubContainer.Values["TrackCount"];
            tempSong.ArtWorkSize.Width = (double)SubContainer.Values["Width"];
            tempSong.ArtWorkSize.Height = (double)SubContainer.Values["Height"];
            tempSong.AlbumArtists = (((string)SubContainer.Values["AlbumArtists"]) != null ? ((string)SubContainer.Values["AlbumArtists"]).Split(new char[3] { '|', ':', '|' }) : null);
            tempSong.Artists = (((string)SubContainer.Values["Artists"]) != null ? ((string)SubContainer.Values["Artists"]).Split(new char[3] { '|', ':', '|' }) : null);
            tempSong.Genres = (((string)SubContainer.Values["Genres"]) != null ? ((string)SubContainer.Values["Genres"]).Split(new char[3] { '|', ':', '|' }) : null);
            string[] sa = (((string)SubContainer.Values["Duration"]) != null ? ((string)SubContainer.Values["Duration"]).Split(':') : null);
            tempSong.Duration = new TimeSpan(Int32.Parse(sa[0]),Int32.Parse(sa[1]),Int32.Parse(sa[2]));
            return tempSong;
        }

        public string FolderToken { get; private set; }
        public TimeSpan Duration { get; private set; }

        public Song()
        {

        }
        public Song(StorageFile File)
        {
            this.AudioFile = File;
        }

        public async Task<Tag> SetTags()
        {
            this.MainKey = AudioFile.FolderRelativeId;
            MusicProperties p = await AudioFile.Properties.GetMusicPropertiesAsync();
            TimeSpan D = p.Duration;
            if (D.Milliseconds > 500)
            {
                Duration = new TimeSpan(D.Hours, D.Minutes, D.Seconds + 1);
            }
            else
            {
                Duration = new TimeSpan(D.Hours, D.Minutes, D.Seconds);
            }
            return await AttachTags();
        }

        private async Task<Tag> AttachTags()
        {
            if (null != this.AudioFile)
            {
                switch (AudioFile.FileType)
                {
                    case ".mp3": var tags1 = await SetTagMP3(); return tags1;
                    case ".m4a": var tags2 = await SetTagM4A(); return tags2;
                    case ".flac": var tags3 = await SetTagFLAC(); return tags3;
                    case ".wav": SetTagWav(); return null;
                    default:
                        return null;
                }
            }
            return null;
        }

        private void SetTagWav()
        {
            Album = null;
            Title = null;
            ArtWork = null;
        }

        private async Task<Tag> SetTagFLAC()
        {
            var fileStream = await AudioFile.OpenStreamForReadAsync();
            var tagFile = TagLib.File.Create(new StreamFileAbstraction(AudioFile.Name,
                             fileStream, fileStream));
            var tags = tagFile.GetTag(TagTypes.FlacMetadata);
            this.Title = tags.Title;
            this.Album = tags.Album;
            this.AlbumArtists = tags.AlbumArtists;
            this.Artists = tags.Performers;
            this.Year = tags.Year;
            this.Genres = tags.Genres;
            this.Disc = tags.Disc;
            this.DiscCount = tags.DiscCount;
            this.Track = tags.Track;
            this.TrackCount = tags.TrackCount;
            return tags;
        }

        public static Song RestoreSongfromStorage(int count, List<IStorageFile> AllList)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            ApplicationDataContainer MainContainer = localSettings.CreateContainer("song" + count, ApplicationDataCreateDisposition.Always);
            Song tempSong = new Song();
            try
            {
                string key = (string)MainContainer.Values["MainKey"];
                string foldertoken = (string)MainContainer.Values["FolderToken"];
                tempSong.MainKey = key;
                tempSong.FolderToken = foldertoken;
                foreach (var item in AllList)
                {
                    if (tempSong.MainKey == ((StorageFile)item).FolderRelativeId)
                    {
                        tempSong.AudioFile = (StorageFile)item;
                        break;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            tempSong.Rating = (uint)MainContainer.Values["Rating"];
            tempSong.Title = (string)MainContainer.Values["Title"];
            tempSong.ArtWork = (string)MainContainer.Values["ArtWork"];
            tempSong.Album = (string)MainContainer.Values["Album"];
            tempSong.Year = (uint)MainContainer.Values["Year"];
            tempSong.Disc = (uint)MainContainer.Values["Disc"];
            tempSong.DiscCount = (uint)MainContainer.Values["DiscCount"];
            tempSong.Track = (uint)MainContainer.Values["Track"];
            tempSong.TrackCount = (uint)MainContainer.Values["TrackCount"];
            tempSong.ArtWorkSize.Width = (double)MainContainer.Values["Width"];
            tempSong.ArtWorkSize.Height = (double)MainContainer.Values["Height"];
            tempSong.AlbumArtists = (((string)MainContainer.Values["AlbumArtists"]) != null ? ((string)MainContainer.Values["AlbumArtists"]).Split(new char[3] { '|', ':', '|' }) : null);
            tempSong.Artists = (((string)MainContainer.Values["Artists"]) != null ? ((string)MainContainer.Values["Artists"]).Split(new char[3] { '|', ':', '|' }) : null);
            tempSong.Genres = (((string)MainContainer.Values["Genres"]) != null ? ((string)MainContainer.Values["Genres"]).Split(new char[3] { '|', ':', '|' }) : null);
            return tempSong;
        }


        public void SaveSongtoStorage(Song item, uint progress)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            ApplicationDataContainer MainContainer =
    localSettings.CreateContainer("song" + progress, ApplicationDataCreateDisposition.Always);
            //string mainkey = StorageApplicationPermissions.FutureAccessList.Add(item.AudioFile, s);
            //if (localSettings.Containers.ContainsKey(mainkey))
            {
                MainContainer.Values["FolderToken"] = item.FolderToken;
                MainContainer.Values["MainKey"] = item.MainKey;
                MainContainer.Values["Title"] = item.Title;
                MainContainer.Values["ArtWork"] = item.ArtWork;
                MainContainer.Values["Album"] = item.Album;
                MainContainer.Values["Year"] = item.Year;
                MainContainer.Values["Disc"] = item.Disc;
                MainContainer.Values["DiscCount"] = item.DiscCount;
                MainContainer.Values["Track"] = item.Track;
                MainContainer.Values["TrackCount"] = item.TrackCount;
                MainContainer.Values["Width"] = item.ArtWorkSize.Width;
                MainContainer.Values["Height"] = item.ArtWorkSize.Height;
                MainContainer.Values["Rating"] = item.Rating;
                string sb = Artists != null ? string.Join("|:|", Artists) : null;
                MainContainer.Values["Artists"] = sb;
                sb = AlbumArtists != null ? string.Join("|:|", AlbumArtists) : null;
                MainContainer.Values["AlbumArtists"] = sb;
                sb = Genres != null ? string.Join("|:|", Genres) : null;
                MainContainer.Values["Genres"] = sb;
                MainContainer.Values["Duration"] = Duration.ToString();
            }
        }

        private async Task<Tag> SetTagM4A()
        {
            var fileStream = await AudioFile.OpenStreamForReadAsync();
            var tagFile = TagLib.File.Create(new StreamFileAbstraction(AudioFile.Name,
                             fileStream, fileStream));
            var tags = tagFile.GetTag(TagTypes.Apple);
            this.Title = tags.Title;
            this.Album = tags.Album;
            this.AlbumArtists = tags.AlbumArtists;
            this.Artists = tags.Performers;
            this.Year = tags.Year;
            this.Genres = tags.Genres;
            this.Disc = tags.Disc;
            this.DiscCount = tags.DiscCount;
            this.Track = tags.Track;
            this.TrackCount = tags.TrackCount;
            return tags;
        }

        private async Task GetArtWorks(Tag tags)
        {
            ArtWorkName = tags.Album;
            IPicture[] p = tags.Pictures;
            StorageFolder cacheFolder = ApplicationData.Current.LocalFolder;

            if (p.Length > 0)
            {

                if (p[0].MimeType.Contains("png"))
                {
                    StorageFile cacheImg = await cacheFolder.CreateFileAsync(ArtWorkName + ".png", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteBytesAsync(cacheImg, p[0].Data.Data);
                    ArtWork = "ms-appdata:///local/" + ArtWorkName + ".png";
                }
                if (p[0].MimeType.Contains("jpeg") || p[0].MimeType.Contains("jpg"))
                {
                    StorageFile cacheImg = await cacheFolder.CreateFileAsync(ArtWorkName + ".jpg", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteBytesAsync(cacheImg, p[0].Data.Data);
                    ArtWork = "ms-appdata:///local/" + ArtWorkName + ".jpg";
                }
            }
            else
            {
                try
                {
                    StorageFile item = await cacheFolder.GetFileAsync(ArtWorkName);
                }
                catch (Exception)
                {
                    ArtWork = null;
                }
            }
        }

        private async Task<Tag> SetTagMP3()
        {
            var fileStream = await AudioFile.OpenStreamForReadAsync();
            var tagFile = TagLib.File.Create(new StreamFileAbstraction(AudioFile.Name,
                             fileStream, fileStream));
            var tags = tagFile.GetTag(TagTypes.Id3v2);
            this.Title = tags.Title;
            this.Album = tags.Album;
            this.AlbumArtists = tags.AlbumArtists;
            this.Artists = tags.Performers;
            this.Year = tags.Year;
            this.Genres = tags.Genres;
            this.Disc = tags.Disc;
            this.DiscCount = tags.DiscCount;
            this.Track = tags.Track;
            this.TrackCount = tags.TrackCount;
            return tags;
        }


        public static async Task<List<Song>> GetSongListfromPath(string tempPath)
        {
            List<Song> SongList = new List<Song>();
            StorageFolder TempFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(tempPath);
            IReadOnlyList<IStorageFile> tempList = await TempFolder.GetFilesAsync();
            foreach (StorageFile tempFile in tempList)
            {
                if (tempTypeStrings.Contains(tempFile.FileType))
                {
                    try
                    {
                        Song song = new Song(tempFile);
                        var tags = await song.SetTags();
                        if (tags != null)
                        {
                            await song.GetArtWorks(tags);
                        }
                        SongList.Add(song);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message + tempFile.Name);

                    }

                }
            }

            return SongList;
        }

        public static async Task GetSongListWithProgress(SongsEnum Songs, string tempPath)
        {
            List<Song> SongList = new List<Song>();
            StorageFolder tempFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(tempPath);
            IReadOnlyList<IStorageFile> AllList = await SearchAllinFolder(tempFolder);
            uint count = (uint)AllList.Count;
            uint progress = 0;
            uint step = 0;
            foreach (StorageFile tempFile in AllList)
            {
                if (tempTypeStrings.Contains(tempFile.FileType))
                {
                    Song song = new Song(tempFile);
                    song.FolderToken = tempPath;
                    var tags = await song.SetTags();
                    if (tags != null)
                    {
                        try
                        {
                            await song.GetArtWorks(tags);
                        }
                        catch (Exception)
                        {
                            song.ArtWork = (song.ArtWorkName == null ? song.ArtWork = null : song.ArtWork = "ms-appdata:///local/" + song.ArtWorkName);
                        }

                    }
                    Songs.SongList.Add(song);
                    song.SaveSongtoStorage(song, progress);
                    progress++;
                    if (((double)(progress - step)) / ((double)count) > 0.01)
                    {
                        Songs.RefreshSongs(Songs.SongList);
                        step = progress;
                        Songs.Percent = (int)(((double)step / count) * 100);
                    }
                }

            }
        }

        public static async Task<IReadOnlyList<IStorageFile>> SearchAllinFolder(StorageFolder tempFolder)
        {

            IReadOnlyList<IStorageItem> tempList = await tempFolder.GetItemsAsync();
            List<IStorageFile> finalList = new List<IStorageFile>();
            foreach (var item in tempList)
            {
                if (item is StorageFolder)
                {
                    finalList.AddRange(await SearchAllinFolder((StorageFolder)item));
                }
                if (item is StorageFile)
                {
                    if (tempTypeStrings.Contains(((StorageFile)item).FileType))
                    {
                        finalList.Add((StorageFile)item);
                    }
                }
            }
            return finalList;
        }
    }
}