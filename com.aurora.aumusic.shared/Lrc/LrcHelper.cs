﻿using com.aurora.aumusic.shared.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.aurora.aumusic.shared.Lrc
{
    public class LrcHelper
    {
        private static char[] NipponChar = new char[]{'あ','か','さ','た','な','は','ま',
            'や','ら','わ','い','き','し','ち','に','ひ','み','り','う','く','す',
            'つ','ぬ','ふ','む','ゆ','る','ん','え','け','せ','て','ね','へ','め',
            'れ','お','こ','そ','と','の','ほ','も','よ','ろ','を','が','ざ','だ',
            'ば','ぱ','ぎ','じ','ぢ','び','ぴ','ぐ','ず','づ','ぶ','げ','ぜ','で','べ','ぺ','ご','ぞ','ど','ぼ','ぽ'};
        public static async Task<LrcRequestModel> isLrcExist(Song song)
        {
            var url = genreqest(song);
            var result =  await WebHelper.WebGETAsync(url, 0, new LrcRequestModel());
            return result;
        }

        private static string genreqest(Song song)
        {
            bool isNippon = false;
            foreach (var item in NipponChar)
            {
                if (song.Title.Contains(item))
                {
                    isNippon = true;
                    break;
                }
            }
            string title;
            if (!isNippon)
                title = ChineseConverter.ToSimplified(song.Title);
            else
                title = song.Title;

            string artist = null;
            if (song.Artists[0] != "Unknown Artists")
                if (!isNippon)
                    artist = ChineseConverter.ToSimplified(song.Artists[0]);
                else
                    artist = song.Artists[0];
            return artist != null ? "http://geci.me/api/lyric/" + title + '/' + artist : "http://geci.me/api/lyric/" + title;
        }
    }
}