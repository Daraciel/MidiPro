namespace MidiPro.Core.GP
{
    public class TrackSettings
    {
        public bool Tablature { get; set; }
        public bool Notation { get; set; }
        public bool DiagramsAreBelow { get; set; }
        public bool ShowRyhthm { get; set; }
        public bool ForceHorizontal { get; set; }
        public bool ForceChannels { get; set; }
        public bool DiagramList { get; set; }
        public bool DiagramsInScore { get; set; }
        public bool AutoLetRing { get; set; }
        public bool AutoBrush { get; set; }
        public bool ExtendRhythmic { get; set; }

        public TrackSettings()
        {
            Tablature = true;
            Notation = true;
            DiagramsAreBelow = false;
            ShowRyhthm = false;
            ForceHorizontal = false;
            ForceChannels = false;
            DiagramList = true;
            DiagramsInScore = false;
            AutoLetRing = false;
            AutoBrush = false;
            ExtendRhythmic = false;
        }
    }
}