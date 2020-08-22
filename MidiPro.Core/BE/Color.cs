namespace MidiPro.Core.BE
{
    public class Color
    {
        public float R { get; set; }

        public float G { get; set; }

        public float B { get; set; }

        public float A { get; set; }

        public Color()
        {
            R = 0;
            G = 0;
            B = 0;
            A = 1.0f;
        }

        public Color(int r, int g, int b, int a = 255)
        {
            R = (float)r / 255.0f;
            G = (float)g / 255.0f;
            B = (float)b / 255.0f;
            A = (float)a / 255.0f;
        }
    }
}