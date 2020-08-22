namespace MidiPro.Core.GP
{
    public class Tempo
    {
        public int Value { get; set; }
        public Tempo(int value = 120)
        {
            this.Value = value;
        }
    }
}