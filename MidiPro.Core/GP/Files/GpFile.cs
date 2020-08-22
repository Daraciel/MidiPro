using System.Collections.Generic;
using MidiPro.Core.BE;
using MidiPro.Core.GP.Enums;
using MidiPro.Core.GP.Rse;

namespace MidiPro.Core.GP.Files
{
    public abstract class GpFile
    {
        //Parent class for common type
        public abstract void ReadSong();


        public Clipboard Clipboard = null;
        public RseMasterEffect MasterEffect = null;
        public PageSetup PageSetup = null;
        public int Tempo;
        public string TempoName;
        public bool HideTempo;
        public List<DirectionSign> Directions = new List<DirectionSign>();
        public string Words;
        public string Music;
        public List<Track> Tracks = new List<Track>();
        public GpFile Self;
        public string Title;
        public string Subtitle;
        public string Interpret;
        public string Album;
        public string Author;
        public string Copyright;
        public string TabAuthor;
        public string Instructional;
        public int[] VersionTuple = new int[] { };
        public string Version = "";
        public List<Lyrics.Lyrics> Lyrics = new List<Lyrics.Lyrics>();
        public List<MeasureHeader> MeasureHeaders = new List<MeasureHeader>();
        public TripletFeels TripletFeel;
    }
}