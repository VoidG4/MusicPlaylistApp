using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_App
{
    public class Artist
    {
        public string name { get; set; }
        public ArrayList songs = new ArrayList();

        public Artist(string name, ArrayList songs)
        {
            this.name = name;
            this.songs = songs;
        }

        public Artist() { }
    }
}
