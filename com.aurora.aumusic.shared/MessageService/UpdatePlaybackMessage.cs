﻿using com.aurora.aumusic.shared.Albums;
using com.aurora.aumusic.shared.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace com.aurora.aumusic.shared.MessageService
{
    [DataContract]
    public class UpdatePlaybackMessage
    {
        public UpdatePlaybackMessage(List<SongModel> songs)
        {
            Songs = songs;
        }

        [DataMember]
        public List<SongModel> Songs;
    }
}
