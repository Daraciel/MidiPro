namespace MidiPro.Core.GP.Rse
{
    public class RseInstrument
    {
        public int Instrument { get; set; }
        public int Unknown { get; set; }
        public int SoundBank { get; set; }
        public int EffectNumber { get; set; }
        public string EffectCategory { get; set; }
        public string Effect { get; set; }

        public RseInstrument()
        {
            Instrument = -1;
            Unknown = 1;
            SoundBank = -1;
            EffectNumber = -1;
            EffectCategory = string.Empty;
            Effect = string.Empty;
        }
    }
}