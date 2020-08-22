namespace MidiPro.Core.Native
{
    public class Note
    {
        public float resizeValue = 1.0f; //Should reflect any later changes made to the note duration, so that bendPoints can be adjusted

        //Values from Note
        public int str = 0;
        public int fret = 0;
        public int velocity = 100;
        public bool isVibrato = false;
        public bool isHammer = false;
        public bool isPalmMuted = false;
        public bool isMuted = false;
        public HarmonicType harmonic = HarmonicType.none;
        public float harmonicFret = 0.0f;
        public bool slidesToNext = false;
        public bool slideInFromBelow = false;
        public bool slideInFromAbove = false;
        public bool slideOutUpwards = false;
        public bool slideOutDownwards = false;
        public List<BendPoint> bendPoints = new List<BendPoint>();
        public bool connect = false; //= tie

        //Values from Beat
        public List<BendPoint> tremBarPoints = new List<BendPoint>();
        public bool isTremBarVibrato = false;
        public bool isSlapped = false;
        public bool isPopped = false;
        public int index = 0;
        public int duration = 0;
        public Fading fading = Fading.none;
        public bool isRHTapped = false;

        public Note(Note old)
        {
            str = old.str; fret = old.fret; velocity = old.velocity; isVibrato = old.isVibrato;
            isHammer = old.isHammer; isPalmMuted = old.isPalmMuted; isMuted = old.isMuted;
            harmonic = old.harmonic; harmonicFret = old.harmonicFret; slidesToNext = old.slidesToNext;
            slideInFromAbove = old.slideInFromAbove; slideInFromBelow = old.slideInFromBelow;
            slideOutDownwards = old.slideOutDownwards; slideOutUpwards = old.slideOutUpwards;
            bendPoints.AddRange(old.bendPoints);
            tremBarPoints.AddRange(old.tremBarPoints);
            isTremBarVibrato = old.isTremBarVibrato; isSlapped = old.isSlapped; isPopped = old.isPopped;
            index = old.index; duration = old.duration; fading = old.fading; isRHTapped = old.isRHTapped;
            resizeValue = old.resizeValue;
        }
        public Note() { }

        public void addBendPoints(List<BendPoint> bendPoints)
        {
            //Hopefully no calculation involved
            this.bendPoints.AddRange(bendPoints);
        }
    }
}