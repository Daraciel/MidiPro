namespace MidiPro.Core.GP
{
    public class MidiChannel
    {

        static int _defaultPercussionChannel = 9;

        public int Channel { get; set; }

        public int EffectChannel { get; set; }

        public int Instrument { get; set; }

        public int Volume { get; set; }

        public int Balance { get; set; }

        public int Chorus { get; set; }

        public int Reverb { get; set; }

        public int Phaser { get; set; }

        public int Tremolo { get; set; }

        public int Bank { get; set; }

        public bool IsPercussionChannel => Channel % 16 == _defaultPercussionChannel;

        public MidiChannel()
        {
            Channel = 0; 
            EffectChannel = 1; 
            Instrument = 25; 
            Volume = 104;
            Balance = 64; 
            Chorus = 0; 
            Reverb = 0;
            Phaser = 0;
            Tremolo = 0; 
            Bank = 0;
        }
    }
}