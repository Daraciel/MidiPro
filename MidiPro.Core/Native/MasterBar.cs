using MidiPro.Core.Enums;
using MidiPro.Core.GP.Enums;

namespace MidiPro.Core.Native
{
    public class MasterBar
    {
        public string Time { get; set; }
        public int Num { get; set; }
        public int Den { get; set; }
        public TripletFeels TripletFeel { get; set; } //additional info -> note values are changed in duration and position too
        public int Duration { get; set; }
        public int Index { get; set; } //Midi Index
        public int Key { get; set; } //C, -1 = F, 1 = G
        public int KeyType { get; set; } //0 = Major, 1 = Minor
        public string KeyBoth { get; set; }

        public MasterBar()
        {
            Time = "4/4";
            Num = 4;
            Den = 4;
            TripletFeel = TripletFeels.None;
            Duration = 0;
            Index = 0;
            Key = 0;
            KeyType = 0;
            KeyBoth = "0";
        }
    }
}