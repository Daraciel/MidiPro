using MidiPro.Core.BE;
using MidiPro.Core.GP.Rse;

namespace MidiPro.Core.GP
{
    public class MixTableChange
    {
        public string TempoName { get; set; }
        public bool HideTempo { get; set; }
        public bool UseRse { get; set; }
        public MixTableItem Instrument { get; set; }
        public MixTableItem Volume { get; set; }
        public MixTableItem Balance { get; set; }
        public MixTableItem Chorus { get; set; }
        public MixTableItem Reverb { get; set; }
        public MixTableItem Phaser { get; set; }
        public MixTableItem Tremolo { get; set; }
        public MixTableItem Tempo { get; set; }
        public WahEffect Wah { get; set; }
        public RseInstrument Rse { get; set; }

        private MixTableChange()
        {
            TempoName = string.Empty;
            HideTempo = false;
            UseRse = false;
            Instrument = null;
            Volume = null;
            Balance = null;
            Chorus = null;
            Reverb = null;
            Phaser = null;
            Tremolo = null;
            Tempo = null;
            Wah = null;
            Rse = null;
        }

        public MixTableChange(string tempoName = "", bool hideTempo = true, bool useRse = true) : this()
        {
            TempoName = tempoName;
            HideTempo = hideTempo;
            UseRse = useRse;
        }

        public bool IsJustWah()
        {
            return (Instrument == null && Volume == null && Balance == null && Chorus == null && Reverb == null &&
                    Phaser == null && Tremolo == null && Wah != null);
        }
    }
}