using System.Collections.Generic;

namespace MidiPro.Core.GP.Rse
{
    public class RseMasterEffect
    {
        public int Volume { get; set; }
        public int Reverb { get; set; }
        public RseEqualizer Equalizer { get; set; }

        private RseMasterEffect()
        {
            Volume = 0;
            Reverb = 0;
            Equalizer = null;
        }

        public RseMasterEffect(int volume = 0, int reverb = 0, RseEqualizer equalizer = null) : this()
        {
            this.Volume = volume;
            this.Reverb = reverb;
            this.Equalizer = equalizer;
            if (equalizer != null && equalizer.Knobs == null)
            {
                equalizer.Knobs = new List<float> { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
            }
        }
    }
}