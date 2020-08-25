using MidiPro.Core.Enums;
using MidiPro.Core.GP.Enums;

namespace MidiPro.Core.Native
{
    public class MasterBar
    {
        public string time = "4/4";
        public int num = 4;
        public int den = 4;
        public TripletFeels tripletFeel = TripletFeels.None; //additional info -> note values are changed in duration and position too
        public int duration = 0;
        public int index = 0; //Midi Index
        public int key = 0; //C, -1 = F, 1 = G
        public int keyType = 0; //0 = Major, 1 = Minor
        public string keyBoth = "0";
    }
}