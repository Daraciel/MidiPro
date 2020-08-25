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

        public string title = "";
        public string subtitle = "";
        public string artist = "";
        public string album = "";
        public string words = "";
        public string music = "";

        public List<DirectionSign> directions = new List<DirectionSign>();
        public List<Annotation> annotations = new List<Annotation>();
        public List<Tempo> tempos = new List<Tempo>();
        public List<MasterBar> barMaster = new List<MasterBar>();
        public List<Track> tracks = new List<Track>();
        public List<Lyrics> lyrics = new List<Lyrics>();


        private List<int> notesInMeasures = new List<int>();
        public static bool[] availableChannels = new bool[16];
        public Midi.MidiExport toMidi()
        {
            Midi.MidiExport mid = new Midi.MidiExport();
            mid.midiTracks.Add(getMidiHeader()); //First, untitled track
            foreach (Track track in tracks)
            {
                mid.midiTracks.Add(track.getMidi());
            }
            return mid;
        }

        private Midi.MidiTrack getMidiHeader()
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
            midiHeader.messages.Add(new Midi.MidiMessage("track_name", new string[] { "untitled" }, 0));
            midiHeader.messages.Add(new Midi.MidiMessage("text", new string[] { title }, 0));
            midiHeader.messages.Add(new Midi.MidiMessage("text", new string[] { subtitle }, 0));
            midiHeader.messages.Add(new Midi.MidiMessage("text", new string[] { artist }, 0));
            midiHeader.messages.Add(new Midi.MidiMessage("text", new string[] { album }, 0));
            midiHeader.messages.Add(new Midi.MidiMessage("text", new string[] { words }, 0));
            midiHeader.messages.Add(new Midi.MidiMessage("text", new string[] { music }, 0));
            midiHeader.messages.Add(new Midi.MidiMessage("copyright", new string[] { "Copyright 2017 by Gitaro" }, 0));
            midiHeader.messages.Add(new Midi.MidiMessage("marker", new string[] { title + " / " + artist + " - Copyright 2017 by Gitaro" }, 0));
            midiHeader.messages.Add(new Midi.MidiMessage("midi_port", new string[] { "0" }, 0));

            //Get tempos from List tempos, get key_signature and time_signature from barMaster
            var tempoIndex = 0;
            var masterBarIndex = 0;
            var currentIndex = 0;
            var oldTimeSignature = "";
            var oldKeySignature = "";
            if (tempos.Count == 0) tempos.Add(new Tempo());
            while (tempoIndex < tempos.Count || masterBarIndex < barMaster.Count)
            {

                //Compare next entry of both possible sources
                if (tempoIndex == tempos.Count || tempos[tempoIndex].position >= barMaster[masterBarIndex].index) //next measure comes first
                {
                    if (!barMaster[masterBarIndex].keyBoth.Equals(oldKeySignature))
                    {
                        //Add Key-Sig to midiHeader
                        midiHeader.messages.Add(new Midi.MidiMessage("key_signature", new string[] { "" + barMaster[masterBarIndex].key, "" + barMaster[masterBarIndex].keyType }, barMaster[masterBarIndex].index - currentIndex));
                        currentIndex = barMaster[masterBarIndex].index;

                        oldKeySignature = barMaster[masterBarIndex].keyBoth;
                    }
                    if (!barMaster[masterBarIndex].time.Equals(oldTimeSignature))
                    {
                        //Add Time-Sig to midiHeader
                        midiHeader.messages.Add(new Midi.MidiMessage("time_signature", new string[] { "" + barMaster[masterBarIndex].num, "" + barMaster[masterBarIndex].den, "24", "8" }, barMaster[masterBarIndex].index - currentIndex));
                        currentIndex = barMaster[masterBarIndex].index;

                        oldTimeSignature = barMaster[masterBarIndex].time;
                    }
                    masterBarIndex++;
                }
                else //next tempo signature comes first
                {
                    //Add Tempo-Sig to midiHeader
                    int _tempo = (int)(Math.Round((60 * 1000000) / tempos[tempoIndex].value));
                    midiHeader.messages.Add(new Midi.MidiMessage("set_tempo", new string[] { "" + _tempo }, tempos[tempoIndex].position - currentIndex));
                    currentIndex = tempos[tempoIndex].position;
                    tempoIndex++;
                }
            }



            midiHeader.messages.Add(new Midi.MidiMessage("end_of_track", new string[] { }, 0));


            return midiHeader;
        }


        public NativeFormat(GpFile fromFile)
        {
            title = fromFile.Title;
            subtitle = fromFile.Subtitle;
            artist = fromFile.Interpret;
            album = fromFile.Album;
            words = fromFile.Words;
            music = fromFile.Music;
            tempos = retrieveTempos(fromFile);
            directions = fromFile.Directions;
            barMaster = retrieveMasterBars(fromFile);
            tracks = retrieveTracks(fromFile);
            lyrics = fromFile.Lyrics;
            updateAvailableChannels();
        }

        private void updateAvailableChannels()
        {

            for (int x = 0; x < 16; x++) { if (x != 9) { availableChannels[x] = true; } else { availableChannels[x] = false; } }
            foreach (Track track in tracks)
            {
                availableChannels[track.channel] = false;
            }
        }

        public List<Track> retrieveTracks(GpFile file)
        {
            List<Track> tracks = new List<Track>();
            foreach (GP.Track tr in file.Tracks)
            {
                Track track = new Track();
                track.name = tr.Name;
                track.patch = tr.Channel.Instrument;
                track.port = tr.Port;
                track.channel = tr.Channel.Channel;
                track.playbackState = PlaybackStates.def;
                track.capo = tr.Offset;
                if (tr.IsMute) track.playbackState = PlaybackStates.mute;
                if (tr.IsSolo) track.playbackState = PlaybackStates.solo;
                track.tuning = getTuning(tr.Strings);

                track.notes = retrieveNotes(tr, track.tuning, track);
                tracks.Add(track);
            }

            return tracks;
        }

        public void addToTremoloBarList(int index, int duration, BendEffect bend, Track myTrack)
        {
            int at;
            myTrack.tremoloPoints.Add(new TremoloPoint(0.0f, index)); //So that it can later be recognized as the beginning
            foreach (GP.BendPoint bp in bend.Points)
            {
                at = index + (int)(bp.Gp6Position * duration / 100.0f);
                var point = new TremoloPoint();
                point.index = at;
                point.value = bp.Gp6Value;
                myTrack.tremoloPoints.Add(point);
            }
            var tp = new TremoloPoint();
            tp.index = index + duration;
            tp.value = 0;
            myTrack.tremoloPoints.Add(tp); //Back to 0 -> Worst case there will be on the same index the final of tone 1, 0, and the beginning of tone 2.


        }

        public List<BendPoint> getBendPoints(int index, int duration, BendEffect bend)
        {
            List<BendPoint> ret = new List<BendPoint>();
            int at;
            foreach (GP.BendPoint bp in bend.Points)
            {
                at = index + (int)(bp.Gp6Position * duration / 100.0f);
                var point = new BendPoint();
                point.index = at;
                point.value = bp.Gp6Value;
                ret.Add(point);
            }

            return ret;
        }



        public List<Note> retrieveNotes(GP.Track track, int[] tuning, Track myTrack)
        {

            List<Note> notes = new List<Note>();
            int index = 0;
            Note[] last_notes = new Note[10];
            bool[] last_was_tie = new bool[10];
            for (int x = 0; x < 10; x++) { last_was_tie[x] = false; }

            //GraceNotes if on beat - reducing the next note's length
            bool rememberGrace = false;
            bool rememberedGrace = false;
            int graceLength = 0;
            int subtractSubindex = 0;

            for (int x = 0; x < 10; x++) last_notes[x] = null;
            int measureIndex = -1;
            int notesInMeasure = 0;
            foreach (Measure m in track.Measures)
            {

                notesInMeasure = 0;
                measureIndex++;
                bool skipVoice = false;
                if (m.SimileMark == SimileMarks.Simple) //Repeat last measure
                {
                    int amountNotes = notesInMeasures[notesInMeasures.Count - 1]; //misuse prohibited by guitarpro
                    int endPoint = notes.Count;
                    for (int x = endPoint - amountNotes; x < endPoint; x++)
                    {
                        Note newNote = new Note(notes[x]);
                        Measure oldM = track.Measures[measureIndex - 1];
                        newNote.index += flipDuration(oldM.Header.TimeSignature.Denominator) * oldM.Header.TimeSignature.Numerator;
                        notes.Add(newNote);
                        notesInMeasure++;
                    }
                    skipVoice = true;
                }
                if (m.SimileMark == SimileMarks.FirstOfDouble || m.SimileMark == SimileMarks.SecondOfDouble) //Repeat first or second of last two measures
                {
                    int secondAmount = notesInMeasures[notesInMeasures.Count - 1]; //misuse prohibited by guitarpro
                    int firstAmount = notesInMeasures[notesInMeasures.Count - 2];
                    int endPoint = notes.Count - secondAmount;
                    for (int x = endPoint - firstAmount; x < endPoint; x++)
                    {
                        Note newNote = new Note(notes[x]);
                        Measure oldM1 = track.Measures[measureIndex - 2];
                        Measure oldM2 = track.Measures[measureIndex - 1];
                        newNote.index += flipDuration(oldM1.Header.TimeSignature.Denominator) * oldM1.Header.TimeSignature.Numerator;
                        newNote.index += flipDuration(oldM2.Header.TimeSignature.Denominator) * oldM2.Header.TimeSignature.Numerator;
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

                        if (b.Text != null && !b.Text.Value.Equals("")) annotations.Add(new Annotation(b.Text.Value, index + subIndex));

                        if (b.Effect.TremoloBar != null) addToTremoloBarList(index + subIndex, flipDuration(b.Duration), b.Effect.TremoloBar, myTrack);


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
                                int brushTotalDuration = flipDuration(temp);
                                int beatTotalDuration = flipDuration(b.Duration);


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
                            note.isTremBarVibrato = b.Effect.Vibrato;
                            note.fading = Fadings.none;
                            if (b.Effect.FadeIn) note.fading = Fadings.fadeIn;
                            if (b.Effect.FadeOut) note.fading = Fadings.fadeOut;
                            if (b.Effect.VolumeSwell) note.fading = Fadings.volumeSwell;
                            note.isSlapped = b.Effect.SlapEffect == SlapEffects.Slapping;
                            note.isPopped = b.Effect.SlapEffect == SlapEffects.Popping;
                            note.isHammer = n.Effect.Hammer;
                            note.isRHTapped = b.Effect.SlapEffect == SlapEffects.Tapping;
                            note.index = index + subIndex;
                            note.duration = flipDuration(b.Duration);


                            //Note values
                            note.fret = n.Value;
                            note.str = n.Str;
                            note.velocity = n.Velocity;
                            note.isVibrato = n.Effect.Vibrato;
                            note.isPalmMuted = n.Effect.PalmMute;
                            note.isMuted = n.Type == NoteTypes.Dead;

                            if (n.Effect.Harmonic != null)
                            {
                                note.harmonicFret = n.Effect.Harmonic.Fret;
                                if (n.Effect.Harmonic.Fret == 0) //older format..
                                {
                                    if (n.Effect.Harmonic.Type == 2) note.harmonicFret = ((ArtificialHarmonic)n.Effect.Harmonic).Pitch.ActualOvertone;
                                }
                                switch (n.Effect.Harmonic.Type)
                                {
                                    case 1: note.harmonic = HarmonicTypes.natural; break;
                                    case 2: note.harmonic = HarmonicTypes.artificial; break;
                                    case 3: note.harmonic = HarmonicTypes.pinch; break;
                                    case 4: note.harmonic = HarmonicTypes.tapped; break;
                                    case 5: note.harmonic = HarmonicTypes.semi; break;

                                    default:
                                        note.harmonic = HarmonicTypes.natural;
                                        break;
                                }
                            }
                            if (n.Effect.Slides != null)
                            {
                                foreach (SlideTypes sl in n.Effect.Slides)
                                {
                                    note.slidesToNext = note.slidesToNext || sl == SlideTypes.ShiftSlideTo || sl == SlideTypes.LegatoSlideTo;
                                    note.slideInFromAbove = note.slideInFromAbove || sl == SlideTypes.IntoFromAbove;
                                    note.slideInFromBelow = note.slideInFromBelow || sl == SlideTypes.IntoFromBelow;
                                    note.slideOutDownwards = note.slideOutDownwards || sl == SlideTypes.OutDownwards;
                                    note.slideOutUpwards = note.slideOutUpwards || sl == SlideTypes.OutUpwards;
                                }
                            }

                            if (n.Effect.Bend != null) note.bendPoints = getBendPoints(index + subIndex, flipDuration(b.Duration), n.Effect.Bend);

                            //Ties

                            bool dontAddNote = false;

                            if (n.Type == NoteTypes.Tie)
                            {


                                dontAddNote = true;
                                //Find if note can simply be added to previous note

                                var last = last_notes[Math.Max(0, note.str - 1)];



                                if (last != null)
                                {
                                    note.fret = last.fret; //For GP3 & GP4
                                    if (last.harmonic != note.harmonic || last.harmonicFret != note.harmonicFret
                                        ) dontAddNote = false;

                                    if (dontAddNote)
                                    {
                                        note.connect = true;
                                        last.duration += note.duration;
                                        last.addBendPoints(note.bendPoints);

                                    }
                                }

                            }
                            else // not a tie
                            {

                                last_was_tie[Math.Max(0, note.str - 1)] = false;
                            }

                            //Extra notes to replicate certain effects


                            //Triplet Feel
                            if (!barMaster[measureIndex].tripletFeel.Equals("None"))
                            {
                                TripletFeels trip = barMaster[measureIndex].tripletFeel;
                                //Check if at regular 8th or 16th beat position
                                bool is_8th_pos = subIndex % 480 == 0;
                                bool is_16th_pos = subIndex % 240 == 0;
                                bool is_first = true; //first of note pair
                                if (is_8th_pos) is_first = subIndex % 960 == 0;
                                if (is_16th_pos) is_first = is_8th_pos;
                                bool is_8th = b.Duration.Value == 8 && !b.Duration.IsDotted && !b.Duration.IsDoubleDotted && b.Duration.Tuplet.Enters == 1 && b.Duration.Tuplet.Times == 1;
                                bool is_16th = b.Duration.Value == 16 && !b.Duration.IsDotted && !b.Duration.IsDoubleDotted && b.Duration.Tuplet.Enters == 1 && b.Duration.Tuplet.Times == 1;

                                if ((trip == TripletFeels.Eigth && is_8th_pos && is_8th) || (trip == TripletFeels.Sixteenth && is_16th_pos && is_16th))
                                {
                                    if (is_first) note.duration = (int)(note.duration * (4.0f / 3.0f));
                                    if (!is_first)
                                    {
                                        note.duration = (int)(note.duration * (2.0f / 3.0f));
                                        note.resizeValue *= (2.0f / 3.0f);
                                        note.index += (int)(note.duration * (1.0f / 3.0f));
                                    }

                                }
                                if ((trip == TripletFeels.Dotted8Th && is_8th_pos && is_8th) || (trip == TripletFeels.Dotted16Th && is_16th_pos && is_16th))
                                {
                                    if (is_first) note.duration = (int)(note.duration * 1.5f);
                                    if (!is_first)
                                    {
                                        note.duration = (int)(note.duration * 0.5f);
                                        note.resizeValue *= (0.5f);
                                        note.index += (int)(note.duration * 0.5f);
                                    }
                                }
                                if ((trip == TripletFeels.Scottish8Th && is_8th_pos && is_8th) || (trip == TripletFeels.Scottish16Th && is_16th_pos && is_16th))
                                {
                                    if (is_first) note.duration = (int)(note.duration * 0.5f);
                                    if (!is_first)
                                    {
                                        note.duration = (int)(note.duration * 1.5f);
                                        note.resizeValue *= (1.5f);
                                        note.index -= (int)(note.duration * 0.5f);
                                    }
                                }


                            }


                            //Tremolo Picking & Trill
                            if (n.Effect.TremoloPicking != null || n.Effect.Trill != null)
                            {
                                int len = note.duration;
                                if (n.Effect.TremoloPicking != null) len = flipDuration(n.Effect.TremoloPicking.Duration);
                                if (n.Effect.Trill != null) len = flipDuration(n.Effect.Trill.Duration);
                                int origDuration = note.duration;
                                note.duration = len;
                                note.resizeValue *= ((float)len / origDuration);
                                int currentIndex = note.index + len;

                                last_notes[Math.Max(0, note.str - 1)] = note;
                                notes.Add(note);
                                notesInMeasure++;

                                dontAddNote = true; //Because we're doing it here already
                                bool originalFret = false;
                                int secondFret = note.fret;

                                if (n.Effect.Trill != null) { secondFret = n.Effect.Trill.Fret - tuning[note.str - 1]; }

                                while (currentIndex + len <= note.index + origDuration)
                                {
                                    Note newOne = new Note(note);
                                    newOne.index = currentIndex;
                                    if (!originalFret) newOne.fret = secondFret; //For trills
                                    last_notes[Math.Max(0, note.str - 1)] = newOne;
                                    if (n.Effect.Trill != null) newOne.isHammer = true;
                                    notes.Add(newOne);
                                    notesInMeasure++;
                                    currentIndex += len;
                                    originalFret = !originalFret;
                                }

                            }


                            //Grace Note
                            if (rememberGrace && note.duration > graceLength)
                            {
                                int orig = note.duration;
                                note.duration -= graceLength;
                                note.resizeValue *= ((float)note.duration / orig);
                                //subIndex -= graceLength;
                                rememberedGrace = true;
                            }
                            if (n.Effect.Grace != null)
                            {
                                bool isOnBeat = n.Effect.Grace.IsOnBeat;

                                if (n.Effect.Grace.Duration != -1)
                                { //GP3,4,5 format

                                    Note graceNote = new Note();
                                    graceNote.index = note.index;
                                    graceNote.fret = n.Effect.Grace.Fret;
                                    graceNote.str = note.str;
                                    Duration dur = new Duration();
                                    dur.Value = n.Effect.Grace.Duration;
                                    graceNote.duration = flipDuration(dur); //works at least for GP5
                                    if (isOnBeat)
                                    {
                                        int orig = note.duration;
                                        note.duration -= graceNote.duration;
                                        note.index += graceNote.duration;
                                        note.resizeValue *= ((float)note.duration / orig);
                                    }
                                    else
                                    {
                                        graceNote.index -= graceNote.duration;

                                    }

                                    notes.Add(graceNote); //TODO: insert at correct position!
                                    notesInMeasure++;

                                }
                                else
                                {


                                    if (isOnBeat) // shorten next note
                                    {
                                        rememberGrace = true;
                                        graceLength = note.duration;
                                    }
                                    else //Change previous note
                                    {
                                        if (notes.Count > 0)
                                        {
                                            note.index -= note.duration; //Can lead to negative indices. Midi should handle that
                                            subtractSubindex = note.duration;

                                        }
                                    }

                                }

                            }


                            //Dead Notes
                            if (n.Type == NoteTypes.Dead)
                            {
                                int orig = note.duration;
                                note.velocity = (int)(note.velocity * 0.9f); note.duration /= 6;
                                note.resizeValue *= ((float)note.duration / orig);
                            }

                            //Ghost Notes
                            if (n.Effect.PalmMute)
                            {
                                int orig = note.duration;
                                note.velocity = (int)(note.velocity * 0.7f); note.duration /= 2;
                                note.resizeValue *= ((float)note.duration / orig);
                            }
                            if (n.Effect.GhostNote) { note.velocity = (int)(note.velocity * 0.8f); }


                            //Staccato, Accented, Heavy Accented
                            if (n.Effect.Staccato)
                            {
                                int orig = note.duration;
                                note.duration /= 2;
                                note.resizeValue *= ((float)note.duration / orig);
                            }
                            if (n.Effect.AccentuatedNote) note.velocity = (int)(note.velocity * 1.2f);
                            if (n.Effect.HeavyAccentuatedNote) note.velocity = (int)(note.velocity * 1.4f);

                            //Arpeggio / Brush
                            if (hasBrush)
                            {
                                note.index = brushInit;
                                brushInit += brushIncrease;

                            }

                            if (!dontAddNote)
                            {
                                last_notes[Math.Max(0, note.str - 1)] = note;
                                notes.Add(note);
                                notesInMeasure++;
                            }


                        }
                        if (rememberedGrace) { subIndex -= graceLength; rememberGrace = false; rememberedGrace = false; } //After the change in duration for the second beat has been done

                        subIndex -= subtractSubindex;
                        subtractSubindex = 0;
                        subIndex += flipDuration(b.Duration);

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
                int measureDuration = flipDuration(m.Header.TimeSignature.Denominator) * m.Header.TimeSignature.Numerator;
                barMaster[measureIndex].duration = measureDuration;
                barMaster[measureIndex].index = index;
                index += measureDuration;
                notesInMeasures.Add(notesInMeasure);
            }


            return notes;
        }



        public int[] getTuning(List<GuitarString> strings)
        {
            int[] tuning = new int[strings.Count];
            for (int x = 0; x < tuning.Length; x++)
            {
                tuning[x] = strings[x].Value;
            }

            return tuning;
        }

        public List<MasterBar> retrieveMasterBars(GpFile file)
        {
            List<MasterBar> masterBars = new List<MasterBar>();
            foreach (MeasureHeader mh in file.MeasureHeaders)
            {
                //(mh.timeSignature.denominator) * mh.timeSignature.numerator;
                MasterBar mb = new MasterBar();
                mb.time = mh.TimeSignature.Numerator + "/" + mh.TimeSignature.Denominator.Value;
                mb.num = mh.TimeSignature.Numerator;
                mb.den = mh.TimeSignature.Denominator.Value;
                string keyFull = "" + (int)mh.KeySignature;
                if (!(keyFull.Length == 1))
                {
                    mb.keyType = int.Parse(keyFull.Substring(keyFull.Length - 1));
                    mb.key = int.Parse(keyFull.Substring(0, keyFull.Length - 1));
                }
                else
                {
                    mb.key = 0;
                    mb.keyType = int.Parse(keyFull);
                }
                mb.keyBoth = keyFull; //Useful for midiExport later

                mb.tripletFeel = mh.TripletFeel;

                masterBars.Add(mb);
            }

            return masterBars;
        }

        public List<Tempo> retrieveTempos(GpFile file)
        {
            List<Tempo> tempos = new List<Tempo>();
            //Version < 4 -> look at Measure Headers, >= 4 look at mixtablechanges


            int version = file.VersionTuple[0];
            if (version < 4) //Look at MeasureHeaders
            {
                //Get inital tempo from file header
                Tempo init = new Tempo();
                init.position = 0;
                init.value = file.Tempo;
                if (init.value != 0) tempos.Add(init);

                int pos = 0;
                float oldTempo = file.Tempo;
                foreach (MeasureHeader mh in file.MeasureHeaders)
                {
                    Tempo t = new Tempo();
                    t.value = mh.Tempo.Value;
                    t.position = pos;
                    pos += flipDuration(mh.TimeSignature.Denominator) * mh.TimeSignature.Numerator;
                    if (oldTempo != t.value) tempos.Add(t);
                    oldTempo = t.value;
                }

            }
            else //Look at MixtableChanges - only on track 1, voice 1
            {
                int pos = 0;

                //Get inital tempo from file header
                Tempo init = new Tempo();
                init.position = 0;
                init.value = file.Tempo;
                if (init.value != 0) tempos.Add(init);
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
                                    t.value = tempo.Value;
                                    t.position = pos + smallPos;

                                    tempos.Add(t);
                                }
                            }
                        }

                        smallPos += flipDuration(b.Duration);
                    }
                    pos += flipDuration(m.Header.TimeSignature.Denominator) * m.Header.TimeSignature.Numerator;
                }
            }

            return tempos;
        }

        private static int flipDuration(Duration d)
        {
            int ticks_per_beat = 960;
            int result = 0;
            switch (d.Value)
            {
                case 1: result += ticks_per_beat * 4; break;
                case 2: result += ticks_per_beat * 2; break;
                case 4: result += ticks_per_beat; break;
                case 8: result += ticks_per_beat / 2; break;
                case 16: result += ticks_per_beat / 4; break;
                case 32: result += ticks_per_beat / 8; break;
                case 64: result += ticks_per_beat / 16; break;
                case 128: result += ticks_per_beat / 32; break;
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