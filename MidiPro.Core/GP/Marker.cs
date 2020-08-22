using MidiPro.Core.BE;

namespace MidiPro.Core.GP
{
    public class Marker
    {
        public string Title { get; set; }
        public Color Color { get; set; }
        public MeasureHeader MeasureHeader { get; set; }

        public Marker()
        {
            Title = "Section";
            Color = new Color(255, 0, 0);
            MeasureHeader = null;
        }
    }
}