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
        public MidiExport.MidiExport toMidi()
        {
            MidiExport.MidiExport mid = new MidiExport.MidiExport();
            mid.midiTracks.Add(getMidiHeader()); //First, untitled track
            foreach (Track track in tracks)
            {
                mid.midiTracks.Add(track.getMidi());
            }
            return mid;
        }

        private MidiExport.MidiTrack getMidiHeader()
        {
            var midiHeader = new MidiExport.MidiTrack();
            //text(s) - name of song, artist etc., created by Gitaro 
            //copyright - by Gitaro
            //midi port 0 
            //time signature
            //key signature
            //set tempo
            ///////marker text (will be seen in file) - also Gitaro copyright blabla
            //end_of_track
            midiHeader.messages.Add(new MidiExport.MidiMessage("track_name", new string[] { "untitled" }, 0));
            midiHeader.messages.Add(new MidiExport.MidiMessage("text", new string[] { title }, 0));
            midiHeader.messages.Add(new MidiExport.MidiMessage("text", new string[] { subtitle }, 0));
            midiHeader.messages.Add(new MidiExport.MidiMessage("text", new string[] { artist }, 0));
            midiHeader.messages.Add(new MidiExport.MidiMessage("text", new string[] { album }, 0));
            midiHeader.messages.Add(new MidiExport.MidiMessage("text", new string[] { words }, 0));
            midiHeader.messages.Add(new MidiExport.MidiMessage("text", new string[] { music }, 0));
            midiHeader.messages.Add(new MidiExport.MidiMessage("copyright", new string[] { "Copyright 2017 by Gitaro" }, 0));
            midiHeader.messages.Add(new MidiExport.MidiMessage("marker", new string[] { title + " / " + artist + " - Copyright 2017 by Gitaro" }, 0));
            midiHeader.messages.Add(new MidiExport.MidiMessage("midi_port", new string[] { "0" }, 0));

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
                        midiHeader.messages.Add(new MidiExport.MidiMessage("key_signature", new string[] { "" + barMaster[masterBarIndex].key, "" + barMaster[masterBarIndex].keyType }, barMaster[masterBarIndex].index - currentIndex));
                        currentIndex = barMaster[masterBarIndex].index;

                        oldKeySignature = barMaster[masterBarIndex].keyBoth;
                    }
                    if (!barMaster[masterBarIndex].time.Equals(oldTimeSignature))
                    {
                        //Add Time-Sig to midiHeader
                        midiHeader.messages.Add(new MidiExport.MidiMessage("time_signature", new string[] { "" + barMaster[masterBarIndex].num, "" + barMaster[masterBarIndex].den, "24", "8" }, barMaster[masterBarIndex].index - currentIndex));
                        currentIndex = barMaster[masterBarIndex].index;

                        oldTimeSignature = barMaster[masterBarIndex].time;
                    }
                    masterBarIndex++;
                }
                else //next tempo signature comes first
                {
                    //Add Tempo-Sig to midiHeader
                    int _tempo = (int)(Math.Round((60 * 1000000) / tempos[tempoIndex].value));
                    midiHeader.messages.Add(new MidiExport.MidiMessage("set_tempo", new string[] { "" + _tempo }, tempos[tempoIndex].position - currentIndex));
                    currentIndex = tempos[tempoIndex].position;
                    tempoIndex++;
                }
            }



            midiHeader.messages.Add(new MidiExport.MidiMessage("end_of_track", new string[] { }, 0));


            return midiHeader;
        }


        public NativeFormat(GPFile fromFile)
        {
            title = fromFile.title;
            subtitle = fromFile.subtitle;
            artist = fromFile.interpret;
            album = fromFile.album;
            words = fromFile.words;
            music = fromFile.music;
            tempos = retrieveTempos(fromFile);
            directions = fromFile.directions;
            barMaster = retrieveMasterBars(fromFile);
            tracks = retrieveTracks(fromFile);
            lyrics = fromFile.lyrics;
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

        public List<Track> retrieveTracks(GPFile file)
        {
            List<Track> tracks = new List<Track>();
            foreach (global::Track tr in file.tracks)
            {
                Track track = new Track();
                track.name = tr.name;
                track.patch = tr.channel.instrument;
                track.port = tr.port;
                track.channel = tr.channel.channel;
                track.playbackState = PlaybackState.def;
                track.capo = tr.offset;
                if (tr.isMute) track.playbackState = PlaybackState.mute;
                if (tr.isSolo) track.playbackState = PlaybackState.solo;
                track.tuning = getTuning(tr.strings);

                track.notes = retrieveNotes(tr, track.tuning, track);
                tracks.Add(track);
            }

            return tracks;
        }

        public void addToTremoloBarList(int index, int duration, BendEffect bend, Track myTrack)
        {
            int at;
            myTrack.tremoloPoints.Add(new TremoloPoint(0.0f, index)); //So that it can later be recognized as the beginning
            foreach (global::BendPoint bp in bend.points)
            {
                at = index + (int)(bp.GP6position * duration / 100.0f);
                var point = new TremoloPoint();
                point.index = at;
                point.value = bp.GP6value;
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
            foreach (global::BendPoint bp in bend.points)
            {
                at = index + (int)(bp.GP6position * duration / 100.0f);
                var point = new BendPoint();
                point.index = at;
                point.value = bp.GP6value;
                ret.Add(point);
            }

            return ret;
        }



        public List<Note> retrieveNotes(global::Track track, int[] tuning, Track myTrack)
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
            foreach (Measure m in track.measures)
            {

                notesInMeasure = 0;
                measureIndex++;
                bool skipVoice = false;
                if (m.simileMark == SimileMark.simple) //Repeat last measure
                {
                    int amountNotes = notesInMeasures[notesInMeasures.Count - 1]; //misuse prohibited by guitarpro
                    int endPoint = notes.Count;
                    for (int x = endPoint - amountNotes; x < endPoint; x++)
                    {
                        Note newNote = new Note(notes[x]);
                        Measure oldM = track.measures[measureIndex - 1];
                        newNote.index += flipDuration(oldM.header.timeSignature.denominator) * oldM.header.timeSignature.numerator;
                        notes.Add(newNote);
                        notesInMeasure++;
                    }
                    skipVoice = true;
                }
                if (m.simileMark == SimileMark.firstOfDouble || m.simileMark == SimileMark.secondOfDouble) //Repeat first or second of last two measures
                {
                    int secondAmount = notesInMeasures[notesInMeasures.Count - 1]; //misuse prohibited by guitarpro
                    int firstAmount = notesInMeasures[notesInMeasures.Count - 2];
                    int endPoint = notes.Count - secondAmount;
                    for (int x = endPoint - firstAmount; x < endPoint; x++)
                    {
                        Note newNote = new Note(notes[x]);
                        Measure oldM1 = track.measures[measureIndex - 2];
                        Measure oldM2 = track.measures[measureIndex - 1];
                        newNote.index += flipDuration(oldM1.header.timeSignature.denominator) * oldM1.header.timeSignature.numerator;
                        newNote.index += flipDuration(oldM2.header.timeSignature.denominator) * oldM2.header.timeSignature.numerator;
                        notes.Add(newNote);
                        notesInMeasure++;
                    }
                    skipVoice = true;
                }

                foreach (Voice v in m.voices)
                {
                    if (skipVoice) break;
                    int subIndex = 0;
                    foreach (Beat b in v.beats)
                    {

                        if (b.text != null && !b.text.value.Equals("")) annotations.Add(new Annotation(b.text.value, index + subIndex));

                        if (b.effect.tremoloBar != null) addToTremoloBarList(index + subIndex, flipDuration(b.duration), b.effect.tremoloBar, myTrack);


                        //Prepare Brush or Arpeggio
                        bool hasBrush = false;
                        int brushInit = 0;
                        int brushIncrease = 0;
                        BeatStrokeDirection brushDirection = BeatStrokeDirection.none;

                        if (b.effect.stroke != null)
                        {
                            int notesCnt = b.notes.Count;
                            brushDirection = b.effect.stroke.direction;
                            if (brushDirection != BeatStrokeDirection.none && notesCnt > 1)
                            {
                                hasBrush = true;
                                Duration temp = new Duration();
                                temp.value = b.effect.stroke.value;
                                int brushTotalDuration = flipDuration(temp);
                                int beatTotalDuration = flipDuration(b.duration);


                                brushIncrease = brushTotalDuration / (notesCnt);
                                int startPos = index + subIndex + (int)((brushTotalDuration - brushIncrease) * (b.effect.stroke.startTime - 1));
                                int endPos = startPos + brushTotalDuration - brushIncrease;

                                if (brushDirection == BeatStrokeDirection.down)
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

                        foreach (global::Note n in b.notes)
                        {
                            Note note = new Note();
                            //Beat values
                            note.isTremBarVibrato = b.effect.vibrato;
                            note.fading = Fading.none;
                            if (b.effect.fadeIn) note.fading = Fading.fadeIn;
                            if (b.effect.fadeOut) note.fading = Fading.fadeOut;
                            if (b.effect.volumeSwell) note.fading = Fading.volumeSwell;
                            note.isSlapped = b.effect.slapEffect == SlapEffect.slapping;
                            note.isPopped = b.effect.slapEffect == SlapEffect.popping;
                            note.isHammer = n.effect.hammer;
                            note.isRHTapped = b.effect.slapEffect == SlapEffect.tapping;
                            note.index = index + subIndex;
                            note.duration = flipDuration(b.duration);


                            //Note values
                            note.fret = n.value;
                            note.str = n.str;
                            note.velocity = n.velocity;
                            note.isVibrato = n.effect.vibrato;
                            note.isPalmMuted = n.effect.palmMute;
                            note.isMuted = n.type == NoteType.dead;

                            if (n.effect.harmonic != null)
                            {
                                note.harmonicFret = n.effect.harmonic.fret;
                                if (n.effect.harmonic.fret == 0) //older format..
                                {
                                    if (n.effect.harmonic.type == 2) note.harmonicFret = ((ArtificialHarmonic)n.effect.harmonic).pitch.actualOvertone;
                                }
                                switch (n.effect.harmonic.type)
                                {
                                    case 1: note.harmonic = HarmonicType.natural; break;
                                    case 2: note.harmonic = HarmonicType.artificial; break;
                                    case 3: note.harmonic = HarmonicType.pinch; break;
                                    case 4: note.harmonic = HarmonicType.tapped; break;
                                    case 5: note.harmonic = HarmonicType.semi; break;

                                    default:
                                        note.harmonic = HarmonicType.natural;
                                        break;
                                }
                            }
                            if (n.effect.slides != null)
                            {
                                foreach (SlideType sl in n.effect.slides)
                                {
                                    note.slidesToNext = note.slidesToNext || sl == SlideType.shiftSlideTo || sl == SlideType.legatoSlideTo;
                                    note.slideInFromAbove = note.slideInFromAbove || sl == SlideType.intoFromAbove;
                                    note.slideInFromBelow = note.slideInFromBelow || sl == SlideType.intoFromBelow;
                                    note.slideOutDownwards = note.slideOutDownwards || sl == SlideType.outDownwards;
                                    note.slideOutUpwards = note.slideOutUpwards || sl == SlideType.outUpwards;
                                }
                            }

                            if (n.effect.bend != null) note.bendPoints = getBendPoints(index + subIndex, flipDuration(b.duration), n.effect.bend);

                            //Ties

                            bool dontAddNote = false;

                            if (n.type == NoteType.tie)
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
                            if (!barMaster[measureIndex].tripletFeel.Equals("none"))
                            {
                                TripletFeel trip = barMaster[measureIndex].tripletFeel;
                                //Check if at regular 8th or 16th beat position
                                bool is_8th_pos = subIndex % 480 == 0;
                                bool is_16th_pos = subIndex % 240 == 0;
                                bool is_first = true; //first of note pair
                                if (is_8th_pos) is_first = subIndex % 960 == 0;
                                if (is_16th_pos) is_first = is_8th_pos;
                                bool is_8th = b.duration.value == 8 && !b.duration.isDotted && !b.duration.isDoubleDotted && b.duration.tuplet.enters == 1 && b.duration.tuplet.times == 1;
                                bool is_16th = b.duration.value == 16 && !b.duration.isDotted && !b.duration.isDoubleDotted && b.duration.tuplet.enters == 1 && b.duration.tuplet.times == 1;

                                if ((trip == TripletFeel.eigth && is_8th_pos && is_8th) || (trip == TripletFeel.sixteenth && is_16th_pos && is_16th))
                                {
                                    if (is_first) note.duration = (int)(note.duration * (4.0f / 3.0f));
                                    if (!is_first)
                                    {
                                        note.duration = (int)(note.duration * (2.0f / 3.0f));
                                        note.resizeValue *= (2.0f / 3.0f);
                                        note.index += (int)(note.duration * (1.0f / 3.0f));
                                    }

                                }
                                if ((trip == TripletFeel.dotted8th && is_8th_pos && is_8th) || (trip == TripletFeel.dotted16th && is_16th_pos && is_16th))
                                {
                                    if (is_first) note.duration = (int)(note.duration * 1.5f);
                                    if (!is_first)
                                    {
                                        note.duration = (int)(note.duration * 0.5f);
                                        note.resizeValue *= (0.5f);
                                        note.index += (int)(note.duration * 0.5f);
                                    }
                                }
                                if ((trip == TripletFeel.scottish8th && is_8th_pos && is_8th) || (trip == TripletFeel.scottish16th && is_16th_pos && is_16th))
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
                            if (n.effect.tremoloPicking != null || n.effect.trill != null)
                            {
                                int len = note.duration;
                                if (n.effect.tremoloPicking != null) len = flipDuration(n.effect.tremoloPicking.duration);
                                if (n.effect.trill != null) len = flipDuration(n.effect.trill.duration);
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

                                if (n.effect.trill != null) { secondFret = n.effect.trill.fret - tuning[note.str - 1]; }

                                while (currentIndex + len <= note.index + origDuration)
                                {
                                    Note newOne = new Note(note);
                                    newOne.index = currentIndex;
                                    if (!originalFret) newOne.fret = secondFret; //For trills
                                    last_notes[Math.Max(0, note.str - 1)] = newOne;
                                    if (n.effect.trill != null) newOne.isHammer = true;
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
                            if (n.effect.grace != null)
                            {
                                bool isOnBeat = n.effect.grace.isOnBeat;

                                if (n.effect.grace.duration != -1)
                                { //GP3,4,5 format

                                    Note graceNote = new Note();
                                    graceNote.index = note.index;
                                    graceNote.fret = n.effect.grace.fret;
                                    graceNote.str = note.str;
                                    Duration dur = new Duration();
                                    dur.value = n.effect.grace.duration;
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
                            if (n.type == NoteType.dead)
                            {
                                int orig = note.duration;
                                note.velocity = (int)(note.velocity * 0.9f); note.duration /= 6;
                                note.resizeValue *= ((float)note.duration / orig);
                            }

                            //Ghost Notes
                            if (n.effect.palmMute)
                            {
                                int orig = note.duration;
                                note.velocity = (int)(note.velocity * 0.7f); note.duration /= 2;
                                note.resizeValue *= ((float)note.duration / orig);
                            }
                            if (n.effect.ghostNote) { note.velocity = (int)(note.velocity * 0.8f); }


                            //Staccato, Accented, Heavy Accented
                            if (n.effect.staccato)
                            {
                                int orig = note.duration;
                                note.duration /= 2;
                                note.resizeValue *= ((float)note.duration / orig);
                            }
                            if (n.effect.accentuatedNote) note.velocity = (int)(note.velocity * 1.2f);
                            if (n.effect.heavyAccentuatedNote) note.velocity = (int)(note.velocity * 1.4f);

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
                        subIndex += flipDuration(b.duration);

                        //Sort brushed tones
                        if (hasBrush && brushDirection == BeatStrokeDirection.up)
                        {
                            //Have to reorder them xxx123 -> xxx321
                            int notesCnt = b.notes.Count;
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
                int measureDuration = flipDuration(m.header.timeSignature.denominator) * m.header.timeSignature.numerator;
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
                tuning[x] = strings[x].value;
            }

            return tuning;
        }

        public List<MasterBar> retrieveMasterBars(GPFile file)
        {
            List<MasterBar> masterBars = new List<MasterBar>();
            foreach (MeasureHeader mh in file.measureHeaders)
            {
                //(mh.timeSignature.denominator) * mh.timeSignature.numerator;
                MasterBar mb = new MasterBar();
                mb.time = mh.timeSignature.numerator + "/" + mh.timeSignature.denominator.value;
                mb.num = mh.timeSignature.numerator;
                mb.den = mh.timeSignature.denominator.value;
                string keyFull = "" + (int)mh.keySignature;
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

                mb.tripletFeel = mh.tripletFeel;

                masterBars.Add(mb);
            }

            return masterBars;
        }

        public List<Tempo> retrieveTempos(GPFile file)
        {
            List<Tempo> tempos = new List<Tempo>();
            //Version < 4 -> look at Measure Headers, >= 4 look at mixtablechanges


            int version = file.versionTuple[0];
            if (version < 4) //Look at MeasureHeaders
            {
                //Get inital tempo from file header
                Tempo init = new Tempo();
                init.position = 0;
                init.value = file.tempo;
                if (init.value != 0) tempos.Add(init);

                int pos = 0;
                float oldTempo = file.tempo;
                foreach (MeasureHeader mh in file.measureHeaders)
                {
                    Tempo t = new Tempo();
                    t.value = mh.tempo.value;
                    t.position = pos;
                    pos += flipDuration(mh.timeSignature.denominator) * mh.timeSignature.numerator;
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
                init.value = file.tempo;
                if (init.value != 0) tempos.Add(init);
                foreach (Measure m in file.tracks[0].measures)
                {
                    int smallPos = 0; //inner measure position 
                    if (m.voices.Count == 0) continue;

                    foreach (Beat b in m.voices[0].beats)
                    {

                        if (b.effect != null)
                        {
                            if (b.effect.mixTableChange != null)
                            {
                                MixTableItem tempo = b.effect.mixTableChange.tempo;
                                if (tempo != null)
                                {
                                    Tempo t = new Tempo();
                                    t.value = tempo.value;
                                    t.position = pos + smallPos;

                                    tempos.Add(t);
                                }
                            }
                        }

                        smallPos += flipDuration(b.duration);
                    }
                    pos += flipDuration(m.header.timeSignature.denominator) * m.header.timeSignature.numerator;
                }
            }

            return tempos;
        }

        private static int flipDuration(Duration d)
        {
            int ticks_per_beat = 960;
            int result = 0;
            switch (d.value)
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
            if (d.isDotted) result = (int)(result * 1.5f);
            if (d.isDoubleDotted) result = (int)(result * 1.75f);

            int enters = d.tuplet.enters;
            int times = d.tuplet.times;

            //3:2 = standard triplet, 3 notes in the time of 2
            result = (int)((result * times) / (float)enters);


            return result;
        }
    }
}