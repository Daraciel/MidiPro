namespace MidiPro.Core.Native
{
    public class TremoloPoint
    {
        public float value = 0; //0 nothing, 100 one whole tone up
        public int index = 0;

        public TremoloPoint()
        {
        }

        public TremoloPoint(float value, int index)
        {
            this.value = value;
            this.index = index;
        }
    }
}