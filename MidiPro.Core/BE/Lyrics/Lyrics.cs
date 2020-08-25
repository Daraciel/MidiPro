namespace MidiPro.Core.BE.Lyrics
{
    public class Lyrics
    {
        private static int _maxLineCount = 5;

        public int TrackChoice { get; set; }

        public LyricLine[] Lines { get; set; }


        public Lyrics()
        {
            TrackChoice = -1;
            Lines = new LyricLine[_maxLineCount];
            for (int x = 0; x < _maxLineCount; x++)
            {
                Lines[x] = new LyricLine();
            }
        }
    }
}