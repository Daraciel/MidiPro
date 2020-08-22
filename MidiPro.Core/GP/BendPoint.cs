namespace MidiPro.Core.GP
{
    public class BendPoint
    {
        public int Position { get; set; }
        public int Value { get; set; }

        public float Gp6Position { get; set; }
        public float Gp6Value { get; set; }

        private bool _vibrato;

        private BendPoint()
        {
            Position = 0;
            Value = 0;
            Gp6Position = 0;
            Gp6Value = 0;
            _vibrato = false;
        }

        public BendPoint(int position = 0, int value = 0, bool vibrato = false) : this()
        {
            Position = position;
            Value = value;
            _vibrato = vibrato;
            Gp6Position = position * 100.0f / BendEffect.MaxPosition;
            Gp6Value = value * 25.0f / BendEffect.SemitoneLength;
        }

        public BendPoint(float position, float value, bool isGp6Format = true) : this()
        {
            if (isGp6Format)
            {
                //GP6 Format: position: 0-100, value: 100 = 1 whole tone up
                Position = (int)(position * BendEffect.MaxPosition / 100);
                Value = (int)(value * 2 * BendEffect.SemitoneLength / 100);
                Gp6Position = position;
                Gp6Value = value;
            }
            else
            {
                Position = (int)position;
                Value = (int)value;
                Gp6Position = position * 100.0f / BendEffect.MaxPosition;
                Gp6Value = value * 50.0f / BendEffect.SemitoneLength;
            }
        }

        public int GetTime(int duration)
        {
            return (int)(duration * (float)Position / (float)BendEffect.MaxPosition);
        }
    }
}