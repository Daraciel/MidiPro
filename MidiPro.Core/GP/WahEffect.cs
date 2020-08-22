using MidiPro.Core.GP.Enums;

namespace MidiPro.Core.GP
{
    public class WahEffect
    {
        public WahStates State { get; set; }
        public bool Display { get; set; }

        public WahEffect()
        {
            State = WahStates.None;
            Display = false;
        }
    }
}