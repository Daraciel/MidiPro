namespace MidiPro.Core.GP
{
    public class GuitarString
    {
        public int Number { get; set; }

        public int Value { get; set; }


        public GuitarString(int number, int value)
        {
            this.Number = number; 
            this.Value = value;
        }
    }
}