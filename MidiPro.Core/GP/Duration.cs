using MidiPro.Core.BE;
using MidiPro.Core.GP.Enums;

namespace MidiPro.Core.GP
{
    public class Duration
    {

        public int Value { get; set; }
        public bool IsDotted { get; set; }
        public bool IsDoubleDotted { get; set; }
        public Tuplet Tuplet { get; set; }

        const int MinTime = (int)((int)((int)Durations.QuarterTime * (4.0f / (int)Durations.SixtyFourth)) * 2.0f / 3.0f);

        public Duration()
        {
            Value = (int)Durations.Quarter;
            IsDotted = false;
            IsDoubleDotted = false;
            Tuplet = new Tuplet();
        }
        public Duration(int time) //Does not recognize tuplets
        { //From GP6 Format -> 30 = 64th, 480 = quarter, 1920 = whole
            int substract = 0;

            if (time >= 15) { Value = (int)Durations.HundredTwentyEigth; substract = 15; }
            if (time >= 30) { Value = (int)Durations.SixtyFourth; substract = 30; }
            if (time >= 60) { Value = (int)Durations.ThirtySecond; substract = 60; }
            if (time >= 120) { Value = (int)Durations.Sixteenth; substract = 120; }
            if (time >= 240) { Value = (int)Durations.Eigth; substract = 240; }
            if (time >= 480) { Value = (int)Durations.Quarter; substract = 480; }
            if (time >= 960) { Value = (int)Durations.Half; substract = 960; }
            if (time >= 1920) { Value = (int)Durations.Whole; substract = 1920; }
            time -= substract;
            if (time >= (float)(Value * 0.5f)) IsDotted = true;
            if (time >= (float)(Value * 0.75f)) { IsDotted = false; IsDoubleDotted = true; }



        }

        public int Time()
        {
            int result = (int)((int)Durations.QuarterTime * (4.0f / Value));
            if (IsDotted) result += (int)(result / 2.0f);
            if (IsDoubleDotted) result += (int)((result / 4.0f) * 3);
            return Tuplet.ConvertTime(result);
        }
    }
}