using System.Collections.Generic;
using MidiPro.Core.Enums;
using MidiPro.Core.GP.Enums;

namespace MidiPro.Core.GP
{
    public class MeasureHeader
    {
        //public GPFile song;
        public RepeatGroup RepeatGroup { get; set; }
        public int Number { get; set; }
        public int Start { get; set; }
        public TimeSignature TimeSignature { get; set; }
        public KeySignatures KeySignature { get; set; }
        public Tempo Tempo { get; set; }
        public TripletFeels TripletFeel { get; set; }
        public bool IsRepeatOpen { get; set; }
        public int RepeatClose { get; set; }
        public List<int> RepeatAlternatives { get; set; }
        public int RealStart { get; set; }
        public bool HasDoubleBar { get; set; }
        public Marker Marker { get; set; }
        public List<string> Direction { get; set; }
        public List<string> FromDirection { get; set; }

        public bool HasMarker => Marker != null;

        public int Length => TimeSignature!=null ? TimeSignature.Numerator * TimeSignature.Denominator.Time() : 0;

        public MeasureHeader()
        {
            RepeatGroup = null;
            Number = 0;
            Start = (int)Durations.QuarterTime;
            TimeSignature = new TimeSignature();
            KeySignature = KeySignatures.CMajor;
            Tempo = new Tempo();
            TripletFeel = TripletFeels.None;
            IsRepeatOpen = false;
            RepeatClose = -1;
            RepeatAlternatives = new List<int>();
            RealStart = -1;
            HasDoubleBar = false;
            Marker = null;
            Direction = new List<string>();
            FromDirection = new List<string>();
        }
    }
}