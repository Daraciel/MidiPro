using System.Collections.Generic;
using MidiPro.Core.GP.Enums;

namespace MidiPro.Core.GP
{
    public class Voice
    {
        public Measure Measure { get; set; }
        public List<Beat.Beat> Beats { get; set; }
        public VoiceDirections Direction { get; set; }
        public Duration Duration { get; set; }

        public bool IsEmpty => Beats.Count == 0;

        private Voice()
        {
            Measure = null;
            Beats = new List<Beat.Beat>();
            Direction = VoiceDirections.None;
            Duration = null;
        }

        public Voice(Measure measure = null) : this()
        {
            this.Measure = measure;
        }

        public void AddBeat(Beat.Beat beat)
        {
            beat.Voice = this;
            Beats.Add(beat);
        }
    }
}