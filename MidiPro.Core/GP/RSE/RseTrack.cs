using System.Collections.Generic;
using MidiPro.Core.GP.Enums;

namespace MidiPro.Core.GP.Rse
{
    public class RseTrack
    {

        public RseInstrument Instrument { get; set; }
        public RseEqualizer Equalizer { get; set; }
        public int Humanize { get; set; }
        public Accentuations AutoAccentuation { get; set; }


        public RseTrack()
        {
            Instrument = null;
            Equalizer = null;
            Humanize = 0;
            AutoAccentuation = Accentuations.None;

            if (Equalizer != null && Equalizer.Knobs == null)
            {
                Equalizer.Knobs = new List<float> { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
            }
        }
    }
}