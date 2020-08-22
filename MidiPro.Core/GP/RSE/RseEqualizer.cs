using System.Collections.Generic;

namespace MidiPro.Core.GP.Rse
{
    public class RseEqualizer
    {
        public float Gain { get; set; }
        public List<float> Knobs { get; set; }

        private RseEqualizer()
        {
            Gain = 0.0f;
            Knobs = null;
        }
        public RseEqualizer(List<float> knobs = null, float gain = 0.0f) : this()
        {
            Gain = gain;
            Knobs = knobs;
        }
    }
}