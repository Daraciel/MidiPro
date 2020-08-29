namespace MidiPro.Core.Native
{
    public class BendPoint
    {
        public float Value { get; set; }
        public int Index { get; set; } //also global index of midi
        public int UsedChannel { get; set; } //After being part of BendingPlan


        public BendPoint()
        {
            Value = 0;
            Index = 0;
            UsedChannel = 0;
        }

        public BendPoint(float value, int index) : this()
        {
            this.Value = value;
            this.Index = index;
        }
    }
}