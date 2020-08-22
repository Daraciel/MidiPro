using MidiPro.Core.GP.Enums;

namespace MidiPro.Core.GP.Beat
{
    public class BeatDisplay
    {
        public bool BreakBeam { get; set; }
        public bool ForceBeam { get; set; }
        public VoiceDirections BeamDirection { get; set; }
        public TupletBrackets TupletBracket { get; set; }
        public int BreakSecondary { get; set; }
        public bool BreakSecondaryTuplet { get; set; }
        public bool ForceBracket { get; set; }

        public BeatDisplay()
        {
            BreakBeam = false;
            ForceBeam = false;
            BeamDirection = VoiceDirections.None;
            TupletBracket = TupletBrackets.None;
            BreakSecondary = 0;
            BreakSecondaryTuplet = false;
            ForceBracket = false;
        }
    }
}