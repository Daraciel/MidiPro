using System.Collections.Generic;
using MidiPro.Core.Native.Enums;

namespace MidiPro.Core.Native
{
    public class Note
    {
        public float ResizeValue = 1.0f; //Should reflect any later changes made to the note duration, so that bendPoints can be adjusted

        //Values from Note
        public int Str = 0;
        public int Fret = 0;
        public int Velocity = 100;
        public bool IsVibrato = false;
        public bool IsHammer = false;
        public bool IsPalmMuted = false;
        public bool IsMuted = false;
        public HarmonicTypes Harmonic = HarmonicTypes.None;
        public float HarmonicFret = 0.0f;
        public bool SlidesToNext = false;
        public bool SlideInFromBelow = false;
        public bool SlideInFromAbove = false;
        public bool SlideOutUpwards = false;
        public bool SlideOutDownwards = false;
        public List<BendPoint> BendPoints = new List<BendPoint>();
        public bool Connect = false; //= tie

        //Values from Beat
        public List<BendPoint> TremoloBarPoints = new List<BendPoint>();
        public bool IsTremoloBarVibrato = false;
        public bool IsSlapped = false;
        public bool IsPopped = false;
        public int Index = 0;
        public int Duration = 0;
        public Fadings Fading = Fadings.None;
        public bool IsRhTapped = false;

        public Note()
        {
            Str = 0;
            Fret = 0;
            Velocity = 100;
            ResizeValue = 1.0f;
            IsVibrato = false;
            IsHammer = false;
            IsPalmMuted = false;
            IsMuted = false;
            Harmonic = HarmonicTypes.None;
            HarmonicFret = 0.0f;
            SlidesToNext = false;
            SlideInFromBelow = false;
            SlideInFromAbove = false;
            SlideOutUpwards = false;
            SlideOutDownwards = false;
            BendPoints = new List<BendPoint>();
            Connect = false;

            TremoloBarPoints = new List<BendPoint>();
            IsTremoloBarVibrato = false;
            IsSlapped = false;
            IsPopped = false;
            Index = 0;
            Duration = 0;
            Fading = Fadings.None;
            IsRhTapped = false;
        }

        public Note(Note old) : this()
        {
            Str = old.Str; Fret = old.Fret; Velocity = old.Velocity; IsVibrato = old.IsVibrato;
            IsHammer = old.IsHammer; IsPalmMuted = old.IsPalmMuted; IsMuted = old.IsMuted;
            Harmonic = old.Harmonic; HarmonicFret = old.HarmonicFret; SlidesToNext = old.SlidesToNext;
            SlideInFromAbove = old.SlideInFromAbove; SlideInFromBelow = old.SlideInFromBelow;
            SlideOutDownwards = old.SlideOutDownwards; SlideOutUpwards = old.SlideOutUpwards;
            BendPoints.AddRange(old.BendPoints);
            TremoloBarPoints.AddRange(old.TremoloBarPoints);
            IsTremoloBarVibrato = old.IsTremoloBarVibrato; IsSlapped = old.IsSlapped; IsPopped = old.IsPopped;
            Index = old.Index; Duration = old.Duration; Fading = old.Fading; IsRhTapped = old.IsRhTapped;
            ResizeValue = old.ResizeValue;
        }

        public void AddBendPoints(List<BendPoint> bendPoints)
        {
            //Hopefully no calculation involved
            this.BendPoints.AddRange(bendPoints);
        }
    }
}