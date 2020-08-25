namespace MidiPro.Core.Native
{
    public class BendPoint
    {
        public float value = 0;
        public int index = 0; //also global index of midi
        public int usedChannel = 0; //After being part of BendingPlan
        public BendPoint(float value, int index)
        {
            this.value = value;
            this.index = index;
        }
        public BendPoint() { }
    }
}