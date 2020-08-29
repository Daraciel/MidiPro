using System;
using System.Collections.Generic;
using MidiPro.Core.BE;
using MidiPro.Core.BE.Lyrics;
using MidiPro.Core.Enums;
using MidiPro.Core.GP;
using MidiPro.Core.GP.Beat;
using MidiPro.Core.GP.Enums;
using MidiPro.Core.GP.Files;
using MidiPro.Core.GP.Harmonic;
using MidiPro.Core.Native.Enums;

namespace MidiPro.Core.Native
{
    public class NativeFormat
    {
        public static bool[] AvailableChannels = new bool[16];

        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Words { get; set; }
        public string Music { get; set; }

        public List<DirectionSign> Directions { get; set; }
        public List<Annotation> Annotations { get; set; }
        public List<Tempo> Tempos { get; set; }
        public List<MasterBar> BarMaster { get; set; }
        public List<Track> Tracks { get; set; }
        public List<Lyrics> Lyrics { get; set; }


        private List<int> _notesInMeasures;

        public NativeFormat()
        {
            Title = string.Empty;
            Subtitle = string.Empty;
            Artist = string.Empty;
            Album = string.Empty;
            Words = string.Empty;
            Music = string.Empty;

            Directions = new List<DirectionSign>();
            Annotations = new List<Annotation>();
            Tempos = new List<Tempo>();
            BarMaster = new List<MasterBar>();
            Tracks = new List<Track>();
            Lyrics = new List<Lyrics>();

            _notesInMeasures = new List<int>();
        }

        public NativeFormat(GpFile fromFile) : this()
        {
            Title = fromFile.Title;
            Subtitle = fromFile.Subtitle;
            Artist = fromFile.Interpret;
            Album = fromFile.Album;
            Words = fromFile.Words;
            Music = fromFile.Music;
            Tempos = RetrieveTempos(fromFile);
            Directions = fromFile.Directions;
            BarMaster = RetrieveMasterBars(fromFile);
            Tracks = RetrieveTracks(fromFile);
            Lyrics = fromFile.Lyrics;
            UpdateAvailableChannels();
        }

        public Midi.MidiExport ToMidi()
        {
            Midi.MidiExport mid = new Midi.MidiExport();
            mid.MidiTracks.Add(GetMidiHeader()); //First, untitled track
            foreach (Track track in Tracks)
            {
                mid.MidiTracks.Add(track.GetMidi());
            }
            return mid;
        }

        private Midi.MidiTrack GetMidiHeader()
        {
            var midiHeader = new Midi.MidiTrack();
            //text(s) - name of song, artist etc., created by Gitaro 
            //copyright - by Gitaro
            //midi port 0 
            //time signature
            //key signature
            //set tempo
            ///////marker text (will be seen in file) - also Gitaro copyright blabla
            //end_of_track
            midiHeader.Messages.Add(new Midi.MidiMessage("track_name", new string[] { "untitled" }, 0));
            midiHeader.Messages.Add(new Midi.MidiMessage("text", new string[] { Title }, 0));
            midiHeader.Messages.Add(new Midi.MidiMessage("text", new string[] { Subtitle }, 0));
            midiHeader.Messages.Add(new Midi.MidiMessage("text", new string[] { Artist }, 0));
            midiHeader.Messages.Add(new Midi.MidiMessage("text", new string[] { Album }, 0));
            midiHeader.Messages.Add(new Midi.MidiMessage("text", new string[] { Words }, 0));
            midiHeader.Messages.Add(new Midi.MidiMessage("text", new string[] { Music }, 0));
            midiHeader.Messages.Add(new Midi.MidiMessage("copyright", new string[] { "Copyright 2017 by Gitaro" }, 0));
            midiHeader.Messages.Add(new Midi.MidiMessage("marker", new string[] { Title + " / " + Artist + " - Copyright 2017 by Gitaro" }, 0));
            midiHeader.Messages.Add(new Midi.MidiMessage("midi_port", new string[] { "0" }, 0));

            //Get tempos from List tempos, get key_signature and time_signature from barMaster
            var tempoIndex = 0;
            var masterBarIndex = 0;
            var currentIndex = 0;
            var oldTimeSignature = "";
            var oldKeySignature = "";
            if (Tempos.Count == 0) Tempos.Add(new Tempo());
            while (tempoIndex < Tempos.Count || masterBarIndex < BarMaster.Count)
            {

                //Compare next entry of both possible sources
                if (tempoIndex == Tempos.Count || Tempos[tempoIndex].Position >= BarMaster[masterBarIndex].Index) //next measure comes first
                {
                    if (!BarMaster[masterBarIndex].KeyBoth.Equals(oldKeySignature))
                    {
                        //Add Key-Sig to midiHeader
                        midiHeader.Messages.Add(new Midi.MidiMessage("key_signature", new string[] { "" + BarMaster[masterBarIndex].Key, "" + BarMaster[masterBarIndex].KeyType }, BarMaster[masterBarIndex].Index - currentIndex));
                        currentIndex = BarMaster[masterBarIndex].Index;

                        oldKeySignature = BarMaster[masterBarIndex].KeyBoth;
                    }
                    if (!BarMaster[masterBarIndex].Time.Equals(oldTimeSignature))
                    {
                        //Add Time-Sig to midiHeader
                        midiHeader.Messages.Add(new Midi.MidiMessage("time_signature", new string[] { "" + BarMaster[masterBarIndex].Num, "" + BarMaster[masterBarIndex].Den, "24", "8" }, BarMaster[masterBarIndex].Index - currentIndex));
                        currentIndex = BarMaster[masterBarIndex].Index;

                        oldTimeSignature = BarMaster[masterBarIndex].Time;
                    }
                    masterBarIndex++;
                }
                else //next tempo signature comes first
                {
                    //Add Tempo-Sig to midiHeader
                    int tempo = (int)(Math.Round((60 * 1000000) / Tempos[tempoIndex].Value));
                    midiHeader.Messages.Add(new Midi.MidiMessage("set_tempo", new string[] { "" + tempo }, Tempos[tempoIndex].Position - currentIndex));
                    currentIndex = Tempos[tempoIndex].Position;
                    tempoIndex++;
                }
            }



            midiHeader.Messages.Add(new Midi.MidiMessage("end_of_track", new string[] { }, 0));


            return midiHeader;
        }


        private void UpdateAvailableChannels()
        {

            for (int x = 0; x < 16; x++) { if (x != 9) { AvailableChannels[x] = true; } else { AvailableChannels[x] = false; } }
            foreach (Track track in Tracks)
            {
                AvailableChannels[track.Channel] = false;
            }
        }

        public List<Track> RetrieveTracks(GpFile file)
        {
            List<Track> tracks = new List<Track>();
            foreach (GP.Track tr in file.Tracks)
            {
                Track track = new Track();
                track.Name = tr.Name;
                track.Patch = tr.Channel.Instrument;
                track.Port = tr.Port;
                track.Channel = tr.Channel.Channel;
                track.PlaybackState = PlaybackStates.Def;
                track.Capo = tr.Offset;
                if (tr.IsMute) track.PlaybackState = PlaybackStates.Mute;
                if (tr.IsSolo) track.PlaybackState = PlaybackStates.Solo;
                track.Tuning = GetTuning(tr.Strings);

                track.Notes = RetrieveNotes(tr, track.Tuning, track);
                tracks.Add(track);
            }

            return tracks;
        }

        public void AddToTremoloBarList(int index, int duration, BendEffect bend, Track myTrack)
        {
            int at;
            myTrack.TremoloPoints.Add(new TremoloPoint(0.0f, index)); //So that it can later be recognized as the beginning
            foreach (GP.BendPoint bp in bend.Points)
            {
                at = index + (int)(bp.Gp6Position * duration / 100.0f);
                var point = new TremoloPoint();
                point.Index = at;
                point.Value = bp.Gp6Value;
                myTrack.TremoloPoints.Add(point);
            }
            var tp = new TremoloPoint();
            tp.Index = index + duration;
            tp.Value = 0;
            myTrack.TremoloPoints.Add(tp); //Back to 0 -> Worst case there will be on the same index the final of tone 1, 0, and the beginning of tone 2.


        }

        public List<BendPoint> GetBendPoints(int index, int duration, BendEffect bend)
        {
            List<BendPoint> ret = new List<BendPoint>();
            int at;
            foreach (GP.BendPoint bp in bend.Points)
            {
                at = index + (int)(bp.Gp6Position * duration / 100.0f);
                var point = new BendPoint();
                point.Index = at;
                point.Value = bp.Gp6Value;
                ret.Add(point);
            }

            return ret;
        }



        public List<Note> RetrieveNotes(GP.Track track, int[] tuning, Track myTrack)
        {

            List<Note> notes = new List<Note>();
            int index = 0;
            Note[] lastNotes = new Note[10];
            bool[] lastWasTie = new bool[10];
            for (int x = 0; x < 10; x++) { lastWasTie[x] = false; }

            //GraceNotes if on beat - reducing the next note's length
            bool rememberGrace = false;
            bool rememberedGrace = false;
            int graceLength = 0;
            int subtractSubindex = 0;

            for (int x = 0; x < 10; x++) lastNotes[x] = null;
            int measureIndex = -1;
            int notesInMeasure = 0;
            foreach (Measure m in track.Measures)
            {

                notesInMeasure = 0;
                measureIndex++;
                bool skipVoice = false;
                if (m.SimileMark == SimileMarks.Simple) //Repeat last measure
                {
                    int amountNotes = _notesInMeasures[_notesInMeasures.Count - 1]; //misuse prohibited by guitarpro
                    int endPoint = notes.Count;
                    for (int x = endPoint - amountNotes; x < endPoint; x++)
                    {
                        Note newNote = new Note(notes[x]);
                        Measure oldM = track.Measures[measureIndex - 1];
                        newNote.Index += FlipDuration(oldM.Header.TimeSignature.Denominator) * oldM.Header.TimeSignature.Numerator;
                        notes.Add(newNote);
                        notesInMeasure++;
                    }
                    skipVoice = true;
                }
                if (m.SimileMark == SimileMarks.FirstOfDouble || m.SimileMark == SimileMarks.SecondOfDouble) //Repeat first or second of last two measures
                {
                    int secondAmount = _notesInMeasures[_notesInMeasures.Count - 1]; //misuse prohibited by guitarpro
                    int firstAmount = _notesInMeasures[_notesInMeasures.Count - 2];
                    int endPoint = notes.Count - secondAmount;
                    for (int x = endPoint - firstAmount; x < endPoint; x++)
                    {
                        Note newNote = new Note(notes[x]);
                        Measure oldM1 = track.Measures[measureIndex - 2];
                        Measure oldM2 = track.Measures[measureIndex - 1];
                        newNote.Index += FlipDuration(oldM1.Header.TimeSignature.Denominator) * oldM1.Header.TimeSignature.Numerator;
                        newNote.Index += FlipDuration(oldM2.Header.TimeSignature.Denominator) * oldM2.Header.TimeSignature.Numerator;
                        notes.Add(newNote);
                        notesInMeasure++;
                    }
                    skipVoice = true;
                }

                foreach (Voice v in m.Voices)
                {
                    if (skipVoice) break;
                    int subIndex = 0;
                    foreach (Beat b in v.Beats)
                    {

                        if (b.Text != null && !b.Text.Value.Equals("")) Annotations.Add(new Annotation(b.Text.Value, index + subIndex));

                        if (b.Effect.TremoloBar != null) AddToTremoloBarList(index + subIndex, FlipDuration(b.Duration), b.Effect.TremoloBar, myTrack);


                        //Prepare Brush or Arpeggio
                        bool hasBrush = false;
                        int brushInit = 0;
                        int brushIncrease = 0;
                        BeatStrokeDirections brushDirection = BeatStrokeDirections.None;

                        if (b.Effect.Stroke != null)
                        {
                            int notesCnt = b.Notes.Count;
                            brushDirection = b.Effect.Stroke.Direction;
                            if (brushDirection != BeatStrokeDirections.None && notesCnt > 1)
                            {
                                hasBrush = true;
                                Duration temp = new Duration();
                                temp.Value = b.Effect.Stroke.Value;
                                int brushTotalDuration = FlipDuration(temp);
                                int beatTotalDuration = FlipDuration(b.Duration);


                                brushIncrease = brushTotalDuration / (notesCnt);
                                int startPos = index + subIndex + (int)((brushTotalDuration - brushIncrease) * (b.Effect.Stroke.StartTime - 1));
                                int endPos = startPos + brushTotalDuration - brushIncrease;

                                if (brushDirection == BeatStrokeDirections.Down)
                                {
                                    brushInit = startPos;
                                }
                                else
                                {
                                    brushInit = endPos;
                                    brushIncrease = -brushIncrease;
                                }
                            }
                        }

                        foreach (GP.Note n in b.Notes)
                        {
                            Note note = new Note();
                            //Beat values
                            note.IsTremoloBarVibrato = b.Effect.Vibrato;
                            note.Fading = Fadings.None;
                            if (b.Effect.FadeIn) note.Fading = Fadings.FadeIn;
                            if (b.Effect.FadeOut) note.Fading = Fadings.FadeOut;
                            if (b.Effect.VolumeSwell) note.Fading = Fadings.VolumeSwell;
                            note.IsSlapped = b.Effect.SlapEffect == SlapEffects.Slapping;
                            note.IsPopped = b.Effect.SlapEffect == SlapEffects.Popping;
                            note.IsHammer = n.Effect.Hammer;
                            note.IsRhTapped = b.Effect.SlapEffect == SlapEffects.Tapping;
                            note.Index = index + subIndex;
                            note.Duration = FlipDuration(b.Duration);


                            //Note values
                            note.Fret = n.Value;
                            note.Str = n.Str;
                            note.Velocity = n.Velocity;
                            note.IsVibrato = n.Effect.Vibrato;
                            note.IsPalmMuted = n.Effect.PalmMute;
                            note.IsMuted = n.Type == NoteTypes.Dead;

                            if (n.Effect.Harmonic != null)
                            {
                                note.HarmonicFret = n.Effect.Harmonic.Fret;
                                if (n.Effect.Harmonic.Fret == 0) //older format..
                                {
                                    if (n.Effect.Harmonic.Type == 2) note.HarmonicFret = ((ArtificialHarmonic)n.Effect.Harmonic).Pitch.ActualOvertone;
                                }
                                switch (n.Effect.Harmonic.Type)
                                {
                                    case 1: note.Harmonic = HarmonicTypes.Natural; break;
                                    case 2: note.Harmonic = HarmonicTypes.Artificial; break;
                                    case 3: note.Harmonic = HarmonicTypes.Pinch; break;
                                    case 4: note.Harmonic = HarmonicTypes.Tapped; break;
                                    case 5: note.Harmonic = HarmonicTypes.Semi; break;

                                    default:
                                        note.Harmonic = HarmonicTypes.Natural;
                                        break;
                                }
                            }
                            if (n.Effect.Slides != null)
                            {
                                foreach (SlideTypes sl in n.Effect.Slides)
                                {
                                    note.SlidesToNext = note.SlidesToNext || sl == SlideTypes.ShiftSlideTo || sl == SlideTypes.LegatoSlideTo;
                                    note.SlideInFromAbove = note.SlideInFromAbove || sl == SlideTypes.IntoFromAbove;
                                    note.SlideInFromBelow = note.SlideInFromBelow || sl == SlideTypes.IntoFromBelow;
                                    note.SlideOutDownwards = note.SlideOutDownwards || sl == SlideTypes.OutDownwards;
                                    note.SlideOutUpwards = note.SlideOutUpwards || sl == SlideTypes.OutUpwards;
                                }
                            }

                            if (n.Effect.Bend != null) note.BendPoints = GetBendPoints(index + subIndex, FlipDuration(b.Duration), n.Effect.Bend);

                            //Ties

                            bool dontAddNote = false;

                            if (n.Type == NoteTypes.Tie)
                            {


                                dontAddNote = true;
                                //Find if note can simply be added to previous note

                                var last = lastNotes[Math.Max(0, note.Str - 1)];



                                if (last != null)
                                {
                                    note.Fret = last.Fret; //For GP3 & GP4
                                    if (last.Harmonic != note.Harmonic || last.HarmonicFret != note.HarmonicFret
                                        ) dontAddNote = false;

                                    if (dontAddNote)
                                    {
                                        note.Connect = true;
                                        last.Duration += note.Duration;
                                        last.AddBendPoints(note.BendPoints);

                                    }
                                }

                            }
                            else // not a tie
                            {

                                lastWasTie[Math.Max(0, note.Str - 1)] = false;
                            }

                            //Extra notes to replicate certain effects


                            //Triplet Feel
                            if (!BarMaster[measureIndex].TripletFeel.Equals("None"))
                            {
                                TripletFeels trip = BarMaster[measureIndex].TripletFeel;
                                //Check if at regular 8th or 16th beat position
                                bool is8ThPos = subIndex % 480 == 0;
                                bool is16ThPos = subIndex % 240 == 0;
                                bool isFirst = true; //first of note pair
                                if (is8ThPos) isFirst = subIndex % 960 == 0;
                                if (is16ThPos) isFirst = is8ThPos;
                                bool is8Th = b.Duration.Value == 8 && !b.Duration.IsDotted && !b.Duration.IsDoubleDotted && b.Duration.Tuplet.Enters == 1 && b.Duration.Tuplet.Times == 1;
                                bool is16Th = b.Duration.Value == 16 && !b.Duration.IsDotted && !b.Duration.IsDoubleDotted && b.Duration.Tuplet.Enters == 1 && b.Duration.Tuplet.Times == 1;

                                if ((trip == TripletFeels.Eigth && is8ThPos && is8Th) || (trip == TripletFeels.Sixteenth && is16ThPos && is16Th))
                                {
                                    if (isFirst) note.Duration = (int)(note.Duration * (4.0f / 3.0f));
                                    if (!isFirst)
                                    {
                                        note.Duration = (int)(note.Duration * (2.0f / 3.0f));
                                        note.ResizeValue *= (2.0f / 3.0f);
                                        note.Index += (int)(note.Duration * (1.0f / 3.0f));
                                    }

                                }
                                if ((trip == TripletFeels.Dotted8Th && is8ThPos && is8Th) || (trip == TripletFeels.Dotted16Th && is16ThPos && is16Th))
                                {
                                    if (isFirst) note.Duration = (int)(note.Duration * 1.5f);
                                    if (!isFirst)
                                    {
                                        note.Duration = (int)(note.Duration * 0.5f);
                                        note.ResizeValue *= (0.5f);
                                        note.Index += (int)(note.Duration * 0.5f);
                                    }
                                }
                                if ((trip == TripletFeels.Scottish8Th && is8ThPos && is8Th) || (trip == TripletFeels.Scottish16Th && is16ThPos && is16Th))
                                {
                                    if (isFirst) note.Duration = (int)(note.Duration * 0.5f);
                                    if (!isFirst)
                                    {
                                        note.Duration = (int)(note.Duration * 1.5f);
                                        note.ResizeValue *= (1.5f);
                                        note.Index -= (int)(note.Duration * 0.5f);
                                    }
                                }


                            }


                            //Tremolo Picking & Trill
                            if (n.Effect.TremoloPicking != null || n.Effect.Trill != null)
                            {
                                int len = note.Duration;
                                if (n.Effect.TremoloPicking != null) len = FlipDuration(n.Effect.TremoloPicking.Duration);
                                if (n.Effect.Trill != null) len = FlipDuration(n.Effect.Trill.Duration);
                                int origDuration = note.Duration;
                                note.Duration = len;
                                note.ResizeValue *= ((float)len / origDuration);
                                int currentIndex = note.Index + len;

                                lastNotes[Math.Max(0, note.Str - 1)] = note;
                                notes.Add(note);
                                notesInMeasure++;

                                dontAddNote = true; //Because we're doing it here already
                                bool originalFret = false;
                                int secondFret = note.Fret;

                                if (n.Effect.Trill != null) { secondFret = n.Effect.Trill.Fret - tuning[note.Str - 1]; }

                                while (currentIndex + len <= note.Index + origDuration)
                                {
                                    Note newOne = new Note(note);
                                    newOne.Index = currentIndex;
                                    if (!originalFret) newOne.Fret = secondFret; //For trills
                                    lastNotes[Math.Max(0, note.Str - 1)] = newOne;
                                    if (n.Effect.Trill != null) newOne.IsHammer = true;
                                    notes.Add(newOne);
                                    notesInMeasure++;
                                    currentIndex += len;
                                    originalFret = !originalFret;
                                }

                            }


                            //Grace Note
                            if (rememberGrace && note.Duration > graceLength)
                            {
                                int orig = note.Duration;
                                note.Duration -= graceLength;
                                note.ResizeValue *= ((float)note.Duration / orig);
                                //subIndex -= graceLength;
                                rememberedGrace = true;
                            }
                            if (n.Effect.Grace != null)
                            {
                                bool isOnBeat = n.Effect.Grace.IsOnBeat;

                                if (n.Effect.Grace.Duration != -1)
                                { //GP3,4,5 format

                                    Note graceNote = new Note();
                                    graceNote.Index = note.Index;
                                    graceNote.Fret = n.Effect.Grace.Fret;
                                    graceNote.Str = note.Str;
                                    Duration dur = new Duration();
                                    dur.Value = n.Effect.Grace.Duration;
                                    graceNote.Duration = FlipDuration(dur); //works at least for GP5
                                    if (isOnBeat)
                                    {
                                        int orig = note.Duration;
                                        note.Duration -= graceNote.Duration;
                                        note.Index += graceNote.Duration;
                                        note.ResizeValue *= ((float)note.Duration / orig);
                                    }
                                    else
                                    {
                                        graceNote.Index -= graceNote.Duration;

                                    }

                                    notes.Add(graceNote); //TODO: insert at correct position!
                                    notesInMeasure++;

                                }
                                else
                                {


                                    if (isOnBeat) // shorten next note
                                    {
                                        rememberGrace = true;
                                        graceLength = note.Duration;
                                    }
                                    else //Change previous note
                                    {
                                        if (notes.Count > 0)
                                        {
                                            note.Index -= note.Duration; //Can lead to negative indices. Midi should handle that
                                            subtractSubindex = note.Duration;

                                        }
                                    }

                                }

                            }


                            //Dead Notes
                            if (n.Type == NoteTypes.Dead)
                            {
                                int orig = note.Duration;
                                note.Velocity = (int)(note.Velocity * 0.9f); note.Duration /= 6;
                                note.ResizeValue *= ((float)note.Duration / orig);
                            }

                            //Ghost Notes
                            if (n.Effect.PalmMute)
                            {
                                int orig = note.Duration;
                                note.Velocity = (int)(note.Velocity * 0.7f); note.Duration /= 2;
                                note.ResizeValue *= ((float)note.Duration / orig);
                            }
                            if (n.Effect.GhostNote) { note.Velocity = (int)(note.Velocity * 0.8f); }


                            //Staccato, Accented, Heavy Accented
                            if (n.Effect.Staccato)
                            {
                                int orig = note.Duration;
                                note.Duration /= 2;
                                note.ResizeValue *= ((float)note.Duration / orig);
                            }
                            if (n.Effect.AccentuatedNote) note.Velocity = (int)(note.Velocity * 1.2f);
                            if (n.Effect.HeavyAccentuatedNote) note.Velocity = (int)(note.Velocity * 1.4f);

                            //Arpeggio / Brush
                            if (hasBrush)
                            {
                                note.Index = brushInit;
                                brushInit += brushIncrease;

                            }

                            if (!dontAddNote)
                            {
                                lastNotes[Math.Max(0, note.Str - 1)] = note;
                                notes.Add(note);
                                notesInMeasure++;
                            }


                        }
                        if (rememberedGrace) { subIndex -= graceLength; rememberGrace = false; rememberedGrace = false; } //After the change in duration for the second beat has been done

                        subIndex -= subtractSubindex;
                        subtractSubindex = 0;
                        subIndex += FlipDuration(b.Duration);

                        //Sort brushed tones
                        if (hasBrush && brushDirection == BeatStrokeDirections.Up)
                        {
                            //Have to reorder them xxx123 -> xxx321
                            int notesCnt = b.Notes.Count;
                            Note[] temp = new Note[notesCnt];
                            for (int x = notes.Count - notesCnt; x < notes.Count; x++)
                            {
                                temp[x - (notes.Count - notesCnt)] = new Note(notes[x]);
                            }
                            for (int x = notes.Count - notesCnt; x < notes.Count; x++)
                            {
                                notes[x] = temp[temp.Length - (x - (notes.Count - notesCnt)) - 1];

                            }


                        }
                        hasBrush = false;
                    }
                    break; //Consider only the first voice
                }
                int measureDuration = FlipDuration(m.Header.TimeSignature.Denominator) * m.Header.TimeSignature.Numerator;
                BarMaster[measureIndex].Duration = measureDuration;
                BarMaster[measureIndex].Index = index;
                index += measureDuration;
                _notesInMeasures.Add(notesInMeasure);
            }


            return notes;
        }



        public int[] GetTuning(List<GuitarString> strings)
        {
            int[] tuning = new int[strings.Count];
            for (int x = 0; x < tuning.Length; x++)
            {
                tuning[x] = strings[x].Value;
            }

            return tuning;
        }

        public List<MasterBar> RetrieveMasterBars(GpFile file)
        {
            List<MasterBar> masterBars = new List<MasterBar>();
            foreach (MeasureHeader mh in file.MeasureHeaders)
            {
                //(mh.timeSignature.denominator) * mh.timeSignature.numerator;
                MasterBar mb = new MasterBar();
                mb.Time = mh.TimeSignature.Numerator + "/" + mh.TimeSignature.Denominator.Value;
                mb.Num = mh.TimeSignature.Numerator;
                mb.Den = mh.TimeSignature.Denominator.Value;
                string keyFull = "" + (int)mh.KeySignature;
                if (!(keyFull.Length == 1))
                {
                    mb.KeyType = int.Parse(keyFull.Substring(keyFull.Length - 1));
                    mb.Key = int.Parse(keyFull.Substring(0, keyFull.Length - 1));
                }
                else
                {
                    mb.Key = 0;
                    mb.KeyType = int.Parse(keyFull);
                }
                mb.KeyBoth = keyFull; //Useful for midiExport later

                mb.TripletFeel = mh.TripletFeel;

                masterBars.Add(mb);
            }

            return masterBars;
        }

        public List<Tempo> RetrieveTempos(GpFile file)
        {
            List<Tempo> tempos = new List<Tempo>();
            //Version < 4 -> look at Measure Headers, >= 4 look at mixtablechanges


            int version = file.VersionTuple[0];
            if (version < 4) //Look at MeasureHeaders
            {
                //Get inital tempo from file header
                Tempo init = new Tempo();
                init.Position = 0;
                init.Value = file.Tempo;
                if (init.Value != 0) tempos.Add(init);

                int pos = 0;
                float oldTempo = file.Tempo;
                foreach (MeasureHeader mh in file.MeasureHeaders)
                {
                    Tempo t = new Tempo();
                    t.Value = mh.Tempo.Value;
                    t.Position = pos;
                    pos += FlipDuration(mh.TimeSignature.Denominator) * mh.TimeSignature.Numerator;
                    if (oldTempo != t.Value) tempos.Add(t);
                    oldTempo = t.Value;
                }

            }
            else //Look at MixtableChanges - only on track 1, voice 1
            {
                int pos = 0;

                //Get inital tempo from file header
                Tempo init = new Tempo();
                init.Position = 0;
                init.Value = file.Tempo;
                if (init.Value != 0) tempos.Add(init);
                foreach (Measure m in file.Tracks[0].Measures)
                {
                    int smallPos = 0; //inner measure position 
                    if (m.Voices.Count == 0) continue;

                    foreach (Beat b in m.Voices[0].Beats)
                    {

                        if (b.Effect != null)
                        {
                            if (b.Effect.MixTableChange != null)
                            {
                                MixTableItem tempo = b.Effect.MixTableChange.Tempo;
                                if (tempo != null)
                                {
                                    Tempo t = new Tempo();
                                    t.Value = tempo.Value;
                                    t.Position = pos + smallPos;

                                    tempos.Add(t);
                                }
                            }
                        }

                        smallPos += FlipDuration(b.Duration);
                    }
                    pos += FlipDuration(m.Header.TimeSignature.Denominator) * m.Header.TimeSignature.Numerator;
                }
            }

            return tempos;
        }

        private static int FlipDuration(Duration d)
        {
            int ticksPerBeat = 960;
            int result = 0;
            switch (d.Value)
            {
                case 1: result += ticksPerBeat * 4; break;
                case 2: result += ticksPerBeat * 2; break;
                case 4: result += ticksPerBeat; break;
                case 8: result += ticksPerBeat / 2; break;
                case 16: result += ticksPerBeat / 4; break;
                case 32: result += ticksPerBeat / 8; break;
                case 64: result += ticksPerBeat / 16; break;
                case 128: result += ticksPerBeat / 32; break;
            }
            if (d.IsDotted) result = (int)(result * 1.5f);
            if (d.IsDoubleDotted) result = (int)(result * 1.75f);

            int enters = d.Tuplet.Enters;
            int times = d.Tuplet.Times;

            //3:2 = standard triplet, 3 notes in the time of 2
            result = (int)((result * times) / (float)enters);


            return result;
        }
    }
}