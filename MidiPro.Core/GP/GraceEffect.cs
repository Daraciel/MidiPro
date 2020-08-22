using MidiPro.Core.BE;
using MidiPro.Core.GP.Enums;

namespace MidiPro.Core.GP
{
    public class GraceEffect
    {
        public int Fret { get; set; }
        public int Duration { get; set; }
        public int Velocity { get; set; }
        public GraceEffectTransitions Transition { get; set; }
        public bool IsOnBeat { get; set; }
        public bool IsDead { get; set; }

        public GraceEffect()
        {
            Fret = 0;
            Duration = -1;
            Velocity = Velocities.Def;
            Transition = GraceEffectTransitions.None;
            IsOnBeat = false;
            IsDead = false;
        }

        public int DurationTime()
        {
            return (int)((int)Durations.QuarterTime / 16.0f * Duration);

        }
    }
}