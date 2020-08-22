using MidiPro.Core.BE;
using MidiPro.Core.GP.Enums;

namespace MidiPro.Core.GP
{
    public class Note
    {
        public Beat.Beat Beat { get; set; }
        public int Value { get; set; }
        public int Velocity { get; set; }
        public int Str { get; set; }
        public bool SwapAccidentals { get; set; }
        public NoteEffect Effect { get; set; }
        public double DurationPercent { get; set; }
        public NoteTypes Type { get; set; }
        public int Duration { get; set; }
        public int Tuplet { get; set; }

        private Note()
        {
            Beat = null;
            Value = 0;
            Velocity = Velocities.Def;
            Str = 0;
            SwapAccidentals = false;
            Effect = new NoteEffect();
            DurationPercent = 1.0;
            DurationPercent = 1.0;
            Type = NoteTypes.Rest;
            Duration = 0;
            Tuplet = 0;
        }

        public Note(Beat.Beat beat = null) : this()
        {
            this.Beat = beat;
        }

        public int RealValue()
        {
            return (Value + Beat.Voice.Measure.Track.Strings[Str - 1].Value);
        }
    }
}