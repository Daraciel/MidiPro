namespace MidiPro.Core.Native
{
    public class TremoloPoint
    {
        public float Value = 0; //0 nothing, 100 one whole tone up
        public int Index = 0;

        public TremoloPoint()
        {
        }

        public TremoloPoint(float value, int index)
        {
            this.Value = value;
            this.Index = index;
        }
    }
}