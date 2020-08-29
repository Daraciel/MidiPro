namespace MidiPro.Core.Native
{
    public class Annotation
    {
        public string Value { get; set; }
        public int Position { get; set; }
        public Annotation(string v = "", int pos = 0)
        {
            Value = v; 
            Position = pos;
        }
    }
}