using System.Collections.Generic;
using MidiPro.Core.BE;
using MidiPro.Core.GP.Enums;

namespace MidiPro.Core.GP
{
    public class Measure
    {

        public const int MaxVoices = 2;

        public Track Track { get; set; }
        public MeasureHeader Header { get; set; }
        public MeasureClefs Clef { get; set; }
        public List<Voice> Voices { get; set; }
        public LineBreaks LineBreak { get; set; }
        public List<Beat.Beat> Beats { get; set; }
        public SimileMarks SimileMark { get; set; }

        public bool IsEmpty => Beats.Count == 0 && Voices.TrueForAll(p => p.IsEmpty);

        /*
        public bool IsEmpty()
        {
            foreach (Voice v in Voices)
            {
                if (!v.IsEmpty()) return false;
            }
            if (Beats.Count != 0) return false;
            return true;
        }
        */

        public int End => Start + Length;

        public int Number => Header.Number;
        public KeySignatures KeySignature => Header.KeySignature;

        public int RepeatClose => Header.RepeatClose;

        public int Start => Header.Start;
        public int Length => Header.Length;
        public Tempo Tempo => Header.Tempo;

        public TimeSignature TimeSignature => Header.TimeSignature;
        public bool IsRepeatOpen => Header.IsRepeatOpen;
        public TripletFeels TripletFeel => Header.TripletFeel;
        public bool HasMarker => Header.HasMarker;
        public Marker Marker => Header.Marker;

        private Measure()
        {
            Track = null;
            Header = null;
            Clef = MeasureClefs.Treble;
            Voices = new List<Voice>();
            LineBreak = LineBreaks.None;
            Beats = new List<Beat.Beat>();
            SimileMark = SimileMarks.None;
        }

        public Measure(Track track = null, MeasureHeader header = null) : this()
        {
            if (Voices.Count == 0)
            {
                for (int x = 0; x < MaxVoices; x++)
                {
                    Voices.Add(new Voice(this));
                }
            }
            this.Header = header;
            this.Track = track;
        }

        public void AddVoice(Voice voice)
        {
            voice.Measure = this;
            Voices.Add(voice);
        }
    }
}