using System.Collections.Generic;
using MidiPro.Core.BE;
using MidiPro.Core.GP.Enums;

namespace MidiPro.Core.GP.Beat
{
    public class Beat
    {
        public Voice Voice { get; set; }
        public List<Voice> Voices { get; set; }
        public Measure Measure { get; set; }
        public Duration Duration { get; set; }
        public int Start { get; set; }
        public BeatEffect Effect { get; set; }
        public Octaves Octave { get; set; }
        public BeatDisplay Display { get; set; }
        public List<Note> Notes { get; set; }
        public BeatStatuses Status { get; set; }
        public BeatText Text { get; set; }


        public int RealStart => Measure != null ? Measure.Header.RealStart + Start + Measure.Start : 0;

        public bool HasVibrato => Notes?.Exists(p => p.Effect.Vibrato) ?? false;

        public bool HasHarmonic => Notes?.Exists(p => p.Effect.IsHarmonic) ?? false;

        private Beat()
        {
            Voice = null;
            Voices = new List<Voice>();
            Measure = null;
            Duration = new Duration();
            Start = (int)Durations.QuarterTime;
            Effect = new BeatEffect();
            Octave = Octaves.None;
            Display = new BeatDisplay();
            Notes = new List<Note>();
            Status = BeatStatuses.Normal;
            Text = new BeatText();
        }

        public Beat(Voice voice = null) :this()
        {
            this.Voice = voice;
        }


        public void AddNote(Note note)
        {
            note.Beat = this;
            Notes.Add(note);
        }
    }
}