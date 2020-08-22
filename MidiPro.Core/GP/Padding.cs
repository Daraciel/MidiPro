namespace MidiPro.Core.GP
{
    public class Padding
    {
        public int Right { get; set; }
        public int Top { get; set; }
        public int Left { get; set; }
        public int Bottom { get; set; }

        public Padding(int right = 0, int top = 0, int left = 0, int bottom = 0)
        {
            Right = right; 
            Top = top; 
            Left = left; 
            Bottom = bottom;
        }
    }
}