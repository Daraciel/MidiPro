namespace MidiPro.Core.GP
{
    public class Clipboard
    {
        public int StartMeasure { get; set; }
        public int StopMeasure { get; set; }
        public int StartTrack { get; set; }
        public int StopTrack { get; set; }
        public int StartBeat { get; set; }
        public int StopBeat { get; set; }
        public bool SubBarCopy { get; set; }

        public Clipboard()
        {
            StartMeasure = 1;
            StopMeasure = 1;
            StartTrack = 1;
            StopTrack = 1;
            StartBeat = 1;
            StopBeat = 1;
            SubBarCopy = false;
        }
    }
}