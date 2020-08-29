namespace MidiPro.Core.Native
{
    public class Tempo
    {
        public float Value { get; set; }
        public int Position { get; set; } //total position in song @ 960 ticks_per_beat

        public Tempo()
        {
            Value = 120.0f;
            Position = 0;
        }

        public Tempo(float value, int position)
        {
            Value = value;
            Position = position;
        }
    }
}