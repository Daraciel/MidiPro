namespace MidiPro.Core.GP
{
    public class DirectionSign
    {
        public string Name { get; set; }
        public short Measure { get; set; }

        public DirectionSign(string name = "", short measure = 0)
        {
            Name = name;
            Measure = measure;
        }
    }
}