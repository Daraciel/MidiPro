using System;
using System.Collections.Generic;
using MidiPro.Core.BE;
using MidiPro.Core.GP.Enums;

namespace MidiPro.Core.GP
{
    public class Chord
    {
        public int[] Strings { get; set; }
        public string Name { get; set; }
        public List<Barre> Barres { get; set; }
        public bool[] Omissions { get; set; }
        public List<Fingerings> Fingerings { get; set; }
        public bool NewFormat { get; set; }
        public int FirstFret { get; set; }
        public bool Sharp { get; set; }
        public PitchClass Root { get; set; }
        public ChordTypes Type { get; set; }
        public ChordExtensions Extension { get; set; }
        public PitchClass Bass { get; set; }
        public ChordAlterations Tonality { get; set; }
        public bool Add { get; set; }
        public ChordAlterations Fifth { get; set; }
        public ChordAlterations Ninth { get; set; }
        public ChordAlterations Eleventh { get; set; }
        public bool Show { get; set; }

        private Chord()
        {
            Strings = null;
            Name = String.Empty;
            Barres = new List<Barre>();
            Omissions = new bool[7];
            Fingerings = new List<Fingerings>();
            NewFormat = false;
            FirstFret = 0;
            Sharp = false;
            Root = null;
            Type = ChordTypes.Major;
            Extension = ChordExtensions.None;
            Bass = null;
            Tonality = ChordAlterations.Perfect;
            Add = false;
            Fifth = ChordAlterations.Perfect;
            Ninth = ChordAlterations.Perfect;
            Eleventh = ChordAlterations.Perfect;
            Show = true;
        }

        public Chord(int length) : this()
        {

            Strings = new int[length];
            for (int x = 0; x < length; x++)
            {
                Strings[x] = -1;
            }
        }

        public int[] Notes()
        {
            List<int> result = new List<int>();

            foreach (int s in Strings)
            {
                if (s >= 0) result.Add(s);
            }
            return result.ToArray();
        }
    }
}