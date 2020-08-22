namespace MidiPro.Core.GP.Enums
{
    public enum BendTypes
    {
        //: No Preset.
        None = 0,
        // Bends
        // =====
        //: A simple bend.
        Bend = 1,
        //: A bend and release afterwards.
        BendRelease = 2,
        //: A bend, then release and rebend.
        BendReleaseBend = 3,
        //: Prebend.
        Prebend = 4,
        //: Prebend and then release.
        PrebendRelease = 5,

        // Tremolobar
        // ==========
        //: Dip the bar down and then back up.
        Dip = 6,
        //: Dive the bar.
        Dive = 7,
        //: Release the bar up.
        ReleaseUp = 8,
        //: Dip the bar up and then back down.
        InvertedDip = 9,
        //: Return the bar.
        Return = 10,
        //: Release the bar down.
        ReleaseDown = 11
    }
}