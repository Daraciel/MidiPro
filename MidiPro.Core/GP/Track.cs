using System.Collections.Generic;
using MidiPro.Core.BE;
using MidiPro.Core.GP.Files;
using MidiPro.Core.GP.Rse;

namespace MidiPro.Core.GP
{
    public class Track
    {
        public GpFile Song;
        public int Number = 0;
        public int Offset = 0; //Capo
        public bool IsSolo = false;
        public bool IsMute = false;
        public bool IsVisible = true;
        public bool IndicateTuning = true;
        public string Name = "";
        public List<Measure> Measures = new List<Measure>();
        public List<GuitarString> Strings = new List<GuitarString>();
        public string TuningName = "";
        public MidiChannel Channel = new MidiChannel();
        public Color Color = new Color(255, 0, 0);
        public TrackSettings Settings = new TrackSettings();
        public int Port = 0;
        public bool IsPercussionTrack = false;
        public bool IsBanjoTrack = false;
        public bool Is12StringedGuitarTrack = false;
        public bool UseRse = false;
        public int FretCount = 0;
        public RseTrack Rse = null;

        public Track(GpFile song, int number, List<GuitarString> strings = null, List<Measure> measures = null)
        {
            this.Song = song;
            this.Number = number;
            if (strings != null) this.Strings = strings;
            if (measures != null) this.Measures = measures;
        }

        public void AddMeasure(Measure measure)
        {
            measure.Track = this;
            Measures.Add(measure);
        }
    }
}