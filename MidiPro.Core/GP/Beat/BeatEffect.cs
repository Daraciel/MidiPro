using MidiPro.Core.BE;
using MidiPro.Core.GP.Enums;

namespace MidiPro.Core.GP.Beat
{
    public class BeatEffect
    {
        public bool FadeIn { get; set; }
        public bool FadeOut { get; set; }
        public bool VolumeSwell { get; set; }

        public BeatStrokeDirections PickStroke { get; set; }
        public bool HasStrummed { get; set; } // HasRasgueado
        public BeatStroke Stroke { get; set; }
        public SlapEffects SlapEffect { get; set; }
        public bool Vibrato { get; set; }
        public Chord Chord { get; set; }
        public BendEffect TremoloBar { get; set; }
        public MixTableChange MixTableChange { get; set; }

        public bool IsChord => Chord != null;
        public bool IsTremoloBar => TremoloBar != null;
        public bool IsSlapEffect => SlapEffect != SlapEffects.None;
        public bool HasPickStroke => PickStroke != BeatStrokeDirections.None;

        public BeatEffect()
        {
            FadeIn = false;
            FadeOut = false;
            VolumeSwell = false;
            PickStroke = BeatStrokeDirections.None;
            HasStrummed = false;
            Stroke = null;
            SlapEffect = SlapEffects.None;
            Vibrato = false;
            Chord = null;
            TremoloBar = null;
            MixTableChange = null;
        }

        public bool IsDefault()
        {
            BeatEffect def = new BeatEffect();
            return (Stroke == def.Stroke && HasStrummed == def.HasStrummed &&
                    PickStroke == def.PickStroke && FadeIn == def.FadeIn &&
                    Vibrato == def.Vibrato && TremoloBar == def.TremoloBar &&
                    SlapEffect == def.SlapEffect);

        }
    }
}