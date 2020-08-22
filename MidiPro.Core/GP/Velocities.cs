namespace MidiPro.Core.GP
{
    public class Velocities
    {
        public const int MinVelocity = 15;
        public const int VelocityIncrement = 16;
        public const int PianoPianissimo = MinVelocity;
        public const int Pianissimo = MinVelocity + VelocityIncrement;
        public const int Piano = MinVelocity + VelocityIncrement * 2;
        public const int MezzoPiano = MinVelocity + VelocityIncrement * 3;
        public const int MezzoForte = MinVelocity + VelocityIncrement * 4;
        public const int Forte = MinVelocity + VelocityIncrement * 5;
        public const int Fortissimo = MinVelocity + VelocityIncrement * 6;
        public const int ForteFortissimo = MinVelocity + VelocityIncrement * 7;
        public const int Def = Forte;
    }
}