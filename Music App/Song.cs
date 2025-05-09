using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Music_App
{
    public class Song
    {
        public string name { get; set; }
        public string title { get; set; }
        public Artist artist { get; set; }
        public float frequency { get; set; }
        public Image photo { get; set; }
        public int year { get; set; }
        public string genre { get; set; }

        public Song(string name, string title, Artist artist, float frequency, Image photo, int year, string genre)
        {
            this.name = name;
            this.title = title;
            this.artist = artist;
            this.frequency = frequency;
            this.photo = photo;
            this.year = year;
            this.genre = genre;
        }

        public Song() { }
    }
}
