using System;

namespace MidiPro.Core.GP
{
    public class PitchClass
    {
        /*Constructor provides several overloads. Each overload provides keyword
        argument *intonation* that may be either "sharp" or "flat".

        First of overloads is (tone, accidental):

        :param tone: integer of whole-tone.
        :param accidental: flat (-1), none (0) or sharp (1).

        >>> p = PitchClass(4, -1)
        >>> vars(p)
        {'accidental': -1, 'intonation': 'flat', 'just': 4, 'value': 3}
        >>> print p
        Eb
        >>> p = PitchClass(4, -1, intonation='sharp')
        >>> vars(p)
        {'accidental': -1, 'intonation': 'flat', 'just': 4, 'value': 3}
        >>> print p
        D#

        Second, semitone number can be directly passed to constructor:

        :param semitone: integer of semitone.

        >>> p = PitchClass(3)
        >>> print p
        Eb
        >>> p = PitchClass(3, intonation='sharp')
        >>> print p
        D#

        And last, but not least, note name:

        :param name: string representing note.

        >>> p = PitchClass('D#')
        >>> print p
        D#*/
        public int Just { get; set; }
        public int Accidental { get; set; }
        public int Value { get; set; }
        public string Intonation { get; set; }
        public float ActualOvertone { get; set; }

        private PitchClass()
        {
            Just = 0;
            Accidental = 0;
            Value = 0;
            Intonation = null;
            ActualOvertone = 0.0f;
        }

        public PitchClass(int arg0I = 0, int arg1I = -1, string arg0S = "", string intonation = "", float actualOvertone = 0.0f) : this()
        {
            string[] notesSharp = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            string[] notesFlat = { "C", "Db", "D", "Eb", "E", "F", "Gb", "G", "Ab", "A", "Bb", "B" };
            int value = 0;
            string str = "";
            int accidental = 0;
            int pitch = 0;
            ActualOvertone = actualOvertone; //Make it simpler to use later in internal format

            if (arg1I == -1)
            {
                if (!arg0S.Equals(""))
                {
                    str = arg0S;
                    for (int x = 0; x < notesSharp.Length; x++)
                    {
                        if (str.Equals(notesSharp[x])) { value = x; break; }
                        if (str.Equals(notesFlat[x])) { value = x; break; }
                    }

                }
                else
                {
                    value = arg0I % 12;

                    str = notesSharp[Math.Max(value, 0)];
                    if (intonation.Equals("flat")) str = notesFlat[value];
                }

                if (str.EndsWith("b")) { accidental = -1; }
                else if (str.EndsWith("#")) { accidental = 1; }

            }
            else
            {
                pitch = arg0I; accidental = arg1I;
                Just = pitch % 12;
                Accidental = accidental;
                Value = this.Just + accidental;
                if (intonation != null) { Intonation = intonation; }
                else
                {
                    if (accidental == -1) { Intonation = "flat"; }
                    else { this.Intonation = "sharp"; }
                }

            }
        }
    }
}