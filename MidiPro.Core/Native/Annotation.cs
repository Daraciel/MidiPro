namespace MidiPro.Core.Native
{
    public class Annotation
    {
        public string value = "";
        public int position = 0;
        public Annotation(string v = "", int pos = 0)
        {
            value = v; position = pos;
        }
    }
}