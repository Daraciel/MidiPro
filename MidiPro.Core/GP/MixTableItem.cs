namespace MidiPro.Core.GP
{
    public class MixTableItem
    {
        public int Value;
        public int Duration;
        public bool AllTracks;

        public MixTableItem()
        {
            Value = 0;
            Duration = 0;
            AllTracks = false;
        }

        public MixTableItem(int value = 0, int duration = 0, bool allTracks = false) : this()
        {
            this.Value = value;
            this.Duration = duration;
            this.AllTracks = allTracks;
        }
    }
}