using System.Collections.Generic;
using MidiPro.Core.GP.Enums;
using MidiPro.Core.GP.Harmonic;

namespace MidiPro.Core.GP
{
    public class NoteEffect
    {
        public bool Vibrato { get; set; }
        public List<SlideTypes> Slides { get; set; }
        public bool Hammer { get; set; }
        public bool GhostNote { get; set; }
        public bool AccentuatedNote { get; set; }
        public bool HeavyAccentuatedNote { get; set; }
        public bool PalmMute { get; set; }
        public bool Staccato { get; set; }
        public bool LetRing { get; set; }
        public Fingerings LeftHandFinger { get; set; }
        public Fingerings RightHandFinger { get; set; }
        public Note Note { get; set; }
        public BendEffect Bend { get; set; }
        public HarmonicEffect Harmonic { get; set; }
        public GraceEffect Grace { get; set; }
        public TrillEffect Trill { get; set; }
        public TremoloPickingEffect TremoloPicking { get; set; }

        public bool IsBend => (Bend != null && Bend.Points.Count > 0);
        public bool IsHarmonic => Harmonic != null;
        public bool IsGrace => Grace != null;
        public bool IsTrill => Trill != null;
        public bool IsTremoloPicking => TremoloPicking != null;

        public bool IsFingering => ((int)LeftHandFinger > -1 || (int)RightHandFinger > -1);

        public NoteEffect()
        {
            Vibrato = false;
            Slides = new List<SlideTypes>();
            Hammer = false;
            GhostNote = false;
            AccentuatedNote = false;
            HeavyAccentuatedNote = false;
            PalmMute = false;
            Staccato = false;
            LetRing = false;
            LeftHandFinger = Fingerings.Open;
            RightHandFinger = Fingerings.Open;
            Note = null;
            Bend = null;
            Harmonic = null;
            Grace = null;
            Trill = null;
            TremoloPicking = null;
        }
    }
}