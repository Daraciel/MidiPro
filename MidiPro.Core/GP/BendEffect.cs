using System.Collections.Generic;
using MidiPro.Core.BE;
using MidiPro.Core.GP.Enums;

namespace MidiPro.Core.GP
{
    public class BendEffect
    {
        public const int SemitoneLength = 1;
        public const int MaxPosition = 12;

        public int MaxValue { get; set; }
        public BendTypes Type { get; set; }
        public int Value { get; set; }
        public List<BendPoint> Points { get; set; }

        public BendEffect()
        {
            MaxValue = SemitoneLength * 12;
            Type = BendTypes.None;
            Value = 0;
            Points = new List<BendPoint>();
        }
    }
}