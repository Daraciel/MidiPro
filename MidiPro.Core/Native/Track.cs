namespace MidiPro.Core.Native
{
    public class Track
    {
        public string name = "";
        public int patch = 0;
        public int port = 0;
        public int channel = 0;
        public int capo = 0;
        public PlaybackState playbackState = PlaybackState.def;
        public int[] tuning = new int[] { 40, 45, 50, 55, 59, 64 };
        public List<Note> notes = new List<Note>();
        public List<TremoloPoint> tremoloPoints = new List<TremoloPoint>();
        private List<int[]> volumeChanges = new List<int[]>();


        public MidiExport.MidiTrack getMidi()
        {
            var midiTrack = new MidiExport.MidiTrack();
            midiTrack.messages.Add(new MidiExport.MidiMessage("midi_port", new string[] { "" + port }, 0));
            midiTrack.messages.Add(new MidiExport.MidiMessage("track_name", new string[] { name }, 0));
            midiTrack.messages.Add(new MidiExport.MidiMessage("program_change", new string[] { "" + channel, "" + patch }, 0));


            List<int[]> noteOffs = new List<int[]>();
            List<int[]> channelConnections = new List<int[]>(); //For bending and trembar: [original Channel, artificial Channel, index at when to delete artificial]
            List<BendingPlan> activeBendingPlans = new List<BendingPlan>();
            int currentIndex = 0;
            Note _temp = new Note();
            _temp.index = notes[notes.Count - 1].index + notes[notes.Count - 1].duration;
            _temp.str = -2;
            notes.Add(_temp);

            tremoloPoints = addDetailsToTremoloPoints(tremoloPoints, 60);

            //var _notes = addSlidesToNotes(notes); //Adding slide notes here, as they should not appear as extra notes during playback

            foreach (Note n in notes)
            {
                noteOffs.Sort((x, y) => x[0].CompareTo(y[0]));



                //Check for active bendings in progress
                List<BendPoint> currentBPs = findAndSortCurrentBendPoints(activeBendingPlans, n.index);
                float _tremBarChange = 0.0f;
                foreach (BendPoint bp in currentBPs)
                {
                    //Check first if there is a note_off event happening in the meantime..
                    List<int[]> newNoteOffs = new List<int[]>();
                    foreach (int[] noteOff in noteOffs)
                    {
                        if (noteOff[0] <= bp.index) //between last and this note, a note off event should occur
                        {
                            midiTrack.messages.Add(
                                new MidiExport.MidiMessage("note_off",
                                new string[] { "" + noteOff[2], "" + noteOff[1], "0" }, noteOff[0] - currentIndex));
                            currentIndex = noteOff[0];
                        }
                        else
                        {
                            newNoteOffs.Add(noteOff);
                        }
                    }
                    noteOffs = newNoteOffs;

                    //Check if there are active tremPoints to be adjusted for
                    List<TremoloPoint> _newTremPoints = new List<TremoloPoint>();

                    foreach (TremoloPoint tp in tremoloPoints)
                    {
                        if (tp.index <= bp.index) //between last and this note, a note off event should occur
                        {
                            _tremBarChange = tp.value;
                        }
                        else
                        {
                            _newTremPoints.Add(tp);
                        }
                    }
                    tremoloPoints = _newTremPoints;

                    //Check if there are active volume changes
                    List<int[]> _newVolumeChanges = new List<int[]>();
                    foreach (int[] vc in volumeChanges)
                    {
                        if (vc[0] <= bp.index) //between last and this note, a volume change event should occur
                        { //channel control value
                            midiTrack.messages.Add(
                   new MidiExport.MidiMessage("control_change",
                   new string[] { "" + bp.usedChannel, "7", "" + vc[1] }, vc[0] - currentIndex));
                            currentIndex = vc[0];
                        }
                        else
                        {
                            _newVolumeChanges.Add(vc);
                        }
                    }
                    volumeChanges = _newVolumeChanges;

                    midiTrack.messages.Add(
                   new MidiExport.MidiMessage("pitchwheel",
                   new string[] { "" + bp.usedChannel, "" + (int)((bp.value + _tremBarChange) * 25.6f) }, bp.index - currentIndex));
                    currentIndex = bp.index;
                }

                //Delete no longer active Bending Plans
                List<BendingPlan> final = new List<BendingPlan>();
                foreach (BendingPlan bpl in activeBendingPlans)
                {

                    BendingPlan newBPL = new BendingPlan(bpl.originalChannel, bpl.usedChannel, new List<BendPoint>());
                    foreach (BendPoint bp in bpl.bendingPoints)
                    {
                        if (bp.index > n.index)
                        {
                            newBPL.bendingPoints.Add(bp);
                        }
                    }
                    if (newBPL.bendingPoints.Count > 0)
                    {
                        final.Add(newBPL);
                    }
                    else //That bending plan has finished
                    {
                        midiTrack.messages.Add(new MidiExport.MidiMessage("pitchwheel", new string[] { "" + bpl.usedChannel, "-128" }, 0));
                        midiTrack.messages.Add(new MidiExport.MidiMessage("control_change", new string[] { "" + bpl.usedChannel, "101", "127" }, 0));
                        midiTrack.messages.Add(new MidiExport.MidiMessage("control_change", new string[] { "" + bpl.usedChannel, "10", "127" }, 0));

                        //Remove the channel from channelConnections
                        List<int[]> newChannelConnections = new List<int[]>();
                        foreach (int[] cc in channelConnections)
                        {
                            if (cc[1] != bpl.usedChannel) newChannelConnections.Add(cc);
                        }
                        channelConnections = newChannelConnections;

                        NativeFormat.availableChannels[bpl.usedChannel] = true;
                    }
                }

                activeBendingPlans = final;




                var activeChannels = getActiveChannels(channelConnections);
                List<TremoloPoint> newTremPoints = new List<TremoloPoint>();
                foreach (TremoloPoint tp in tremoloPoints)
                {
                    if (tp.index <= n.index) //between last and this note, a trembar event should occur
                    {
                        var value = tp.value * 25.6f;
                        value = Math.Min(Math.Max(value, -8192), 8191);
                        foreach (int ch in activeChannels)
                        {
                            midiTrack.messages.Add(
                     new MidiExport.MidiMessage("pitchwheel",
                     new string[] { "" + ch, "" + (int)(value) }, tp.index - currentIndex));
                            currentIndex = tp.index;
                        }
                    }
                    else
                    {
                        newTremPoints.Add(tp);
                    }
                }
                tremoloPoints = newTremPoints;


                //Check if there are active volume changes
                List<int[]> newVolumeChanges = new List<int[]>();
                foreach (int[] vc in volumeChanges)
                {
                    if (vc[0] <= n.index) //between last and this note, a volume change event should occur
                    {

                        foreach (int ch in activeChannels)
                        {
                            midiTrack.messages.Add(
               new MidiExport.MidiMessage("control_change",
               new string[] { "" + ch, "7", "" + vc[1] }, vc[0] - currentIndex));
                            currentIndex = vc[0];
                        }
                    }
                    else
                    {
                        newVolumeChanges.Add(vc);
                    }
                }
                volumeChanges = newVolumeChanges;


                List<int[]> temp = new List<int[]>();
                foreach (int[] noteOff in noteOffs)
                {
                    if (noteOff[0] <= n.index) //between last and this note, a note off event should occur
                    {
                        midiTrack.messages.Add(
                            new MidiExport.MidiMessage("note_off",
                            new string[] { "" + noteOff[2], "" + noteOff[1], "0" }, noteOff[0] - currentIndex));
                        currentIndex = noteOff[0];
                    }
                    else
                    {
                        temp.Add(noteOff);
                    }
                }
                noteOffs = temp;

                int velocity = n.velocity;
                int note;

                if (n.str == -2) break; //Last round

                if (n.str - 1 < 0) Debug.Log("String was -1");
                if (n.str - 1 >= tuning.Length && tuning.Length != 0) Debug.Log("String was higher than string amount (" + n.str + ")");
                if (tuning.Length > 0) note = tuning[n.str - 1] + capo + n.fret;
                else
                {
                    note = capo + n.fret;
                }
                if (n.harmonic != HarmonicType.none) //Has Harmonics
                {
                    int harmonicNote = getHarmonic(tuning[n.str - 1], n.fret, capo, n.harmonicFret, n.harmonic);
                    note = harmonicNote;
                }

                int noteChannel = channel;

                if (n.bendPoints.Count > 0) //Has Bending
                {
                    int usedChannel = tryToFindChannel();
                    if (usedChannel == -1) usedChannel = channel;
                    NativeFormat.availableChannels[usedChannel] = false;
                    channelConnections.Add(new int[] { channel, usedChannel, n.index + n.duration });
                    midiTrack.messages.Add(new MidiExport.MidiMessage("program_change", new string[] { "" + usedChannel, "" + patch }, n.index - currentIndex));
                    noteChannel = usedChannel;
                    currentIndex = n.index;
                    activeBendingPlans.Add(createBendingPlan(n.bendPoints, channel, usedChannel, n.duration, n.index, n.resizeValue, n.isVibrato));
                }

                if (n.isVibrato && n.bendPoints.Count == 0) //Is Vibrato & No Bending
                {
                    int usedChannel = channel;
                    activeBendingPlans.Add(createBendingPlan(n.bendPoints, channel, usedChannel, n.duration, n.index, n.resizeValue, true));

                }

                if (n.fading != Fading.none) //Fading
                {
                    volumeChanges = createVolumeChanges(n.index, n.duration, n.velocity, n.fading);
                }

                midiTrack.messages.Add(new MidiExport.MidiMessage("note_on", new string[] { "" + noteChannel, "" + note, "" + n.velocity }, n.index - currentIndex));
                currentIndex = n.index;

                if (n.bendPoints.Count > 0) //Has Bending cont.
                {
                    midiTrack.messages.Add(new MidiExport.MidiMessage("control_change", new string[] { "" + noteChannel, "101", "0" }, 0));
                    midiTrack.messages.Add(new MidiExport.MidiMessage("control_change", new string[] { "" + noteChannel, "100", "0" }, 0));
                    midiTrack.messages.Add(new MidiExport.MidiMessage("control_change", new string[] { "" + noteChannel, "6", "6" }, 0));
                    midiTrack.messages.Add(new MidiExport.MidiMessage("control_change", new string[] { "" + noteChannel, "38", "0" }, 0));


                }

                noteOffs.Add(new int[] { n.index + n.duration, note, noteChannel });

            }




            midiTrack.messages.Add(new MidiExport.MidiMessage("end_of_track", new string[] { }, 0));
            return midiTrack;
        }

        private List<Note> addSlidesToNotes(List<Note> notes)
        {
            List<Note> ret = new List<Note>();
            int index = -1;
            foreach (Note n in notes)
            {
                index++;
                bool skipWrite = false;

                if ((n.slideInFromBelow && n.str > 1) || n.slideInFromAbove)
                {
                    int myFret = n.fret;
                    int start = n.slideInFromAbove ? myFret + 4 : Math.Max(1, myFret - 4);
                    int beginIndex = n.index - 960 / 4; //16th before
                    int lengthEach = (960 / 4) / Math.Abs(myFret - start);
                    for (int x = 0; x < Math.Abs(myFret - start); x++)
                    {
                        Note newOne = new Note(n);
                        newOne.duration = lengthEach;
                        newOne.index = beginIndex + x * lengthEach;
                        newOne.fret = start + (n.slideInFromAbove ? -x : +x);
                        ret.Add(newOne);
                    }
                }

                if ((n.slideOutDownwards && n.str > 1) || n.slideOutUpwards)
                {
                    int myFret = n.fret;
                    int end = n.slideOutUpwards ? myFret + 4 : Math.Max(1, myFret - 4);
                    int beginIndex = (n.index + n.duration) - 960 / 4; //16th before
                    int lengthEach = (960 / 4) / Math.Abs(myFret - end);
                    n.duration -= 960 / 4;
                    ret.Add(n); skipWrite = true;
                    for (int x = 0; x < Math.Abs(myFret - end); x++)
                    {
                        Note newOne = new Note(n);
                        newOne.duration = lengthEach;
                        newOne.index = beginIndex + x * lengthEach;
                        newOne.fret = myFret + (n.slideOutDownwards ? -x : +x);
                        ret.Add(newOne);
                    }
                }
                /*
                if (n.slidesToNext)
                {
                    int slideTo = -1;
                    //Find next note on same string
                    for (int x = index+1; x < notes.Count; x++)
                    {
                        if (notes[x].str == n.str)
                        {
                            slideTo = notes[x].fret;
                            break;
                        }
                    }

                    if (slideTo != -1 && slideTo != n.fret) //Found next tone on string
                    {
                        int myStr = n.str;
                        int end = slideTo;
                        int beginIndex = (n.index + n.duration) - 960 / 4; //16th before
                        int lengthEach = (960 / 4) / Math.Abs(myStr - end);
                        n.duration -= 960 / 4;
                        ret.Add(n); skipWrite = true;
                        for (int x = 0; x < Math.Abs(myStr - end); x++)
                        {
                            Note newOne = new Note(n);
                            newOne.duration = lengthEach;
                            newOne.index = beginIndex + x * lengthEach;
                            newOne.fret = myStr + (slideTo < n.fret ? -x : +x);
                            ret.Add(newOne);
                        }
                    }
                }
                */

                if (!skipWrite) ret.Add(n);
            }

            return ret;
        }

        private List<int[]> createVolumeChanges(int index, int duration, int velocity, Fading fading)
        {
            int segments = 20;
            List<int[]> changes = new List<int[]>();
            if (fading == Fading.fadeIn || fading == Fading.fadeOut)
            {
                int step = velocity / segments;
                int val = fading == Fading.fadeIn ? 0 : velocity;
                if (fading == Fading.fadeOut) step = (int)(-step * 1.25f);

                for (int x = index; x < index + duration; x += (duration / segments))
                {
                    changes.Add(new int[] { x, Math.Min(127, Math.Max(0, val)) });
                    val += step;
                }

            }

            if (fading == Fading.volumeSwell)
            {
                int step = (int)(velocity / (segments * 0.8f));
                int val = 0;
                int times = 0;
                for (int x = index; x < index + duration; x += (duration / segments))
                {

                    changes.Add(new int[] { x, Math.Min(127, Math.Max(0, val)) });
                    val += step;
                    if (times == segments / 2) step = -step;
                    times++;
                }
            }
            changes.Add(new int[] { index + duration, velocity }); //Definitely go back to normal


            return changes;
        }

        private List<int> getActiveChannels(List<int[]> channelConnections)
        {
            List<int> ret_val = new List<int>();
            ret_val.Add(channel);
            foreach (int[] cc in channelConnections)
            {
                ret_val.Add(cc[1]);
            }
            return ret_val;
        }

        public int tryToFindChannel()
        {
            int cnt = 0;
            foreach (bool available in NativeFormat.availableChannels)
            {
                if (available) return cnt;
                cnt++;
            }
            return -1;
        }

        public int getHarmonic(int baseTone, int fret, int capo, float harmonicFret, HarmonicType type)
        {
            int val = 0;
            //Capo, base tone and fret (if not natural harmonic) shift the harmonics simply
            val = val + baseTone + capo;
            if (type != HarmonicType.natural)
            {
                val += (int)Math.Round(harmonicFret);
            }
            val += fret;

            if (harmonicFret == 2.4f) val += 34;
            if (harmonicFret == 2.7f) val += 31;
            if (harmonicFret == 3.2f) val += 28;
            if (harmonicFret == 4f) val += 24;
            if (harmonicFret == 5f) val += 19;
            if (harmonicFret == 5.8f) val += 28;
            if (harmonicFret == 7f) val += 12;
            if (harmonicFret == 8.2f) val += 28;
            if (harmonicFret == 9f) val += 19;
            if (harmonicFret == 9.6f) val += 24;
            if (harmonicFret == 12f) val += 0;
            if (harmonicFret == 14.7f) val += 19;
            if (harmonicFret == 16f) val += 12;
            if (harmonicFret == 17f) val += 19;
            if (harmonicFret == 19f) val += 0;
            if (harmonicFret == 21.7f) val += 12;
            if (harmonicFret == 24f) val += 0;

            return Math.Min(val, 127);
        }


        public List<BendPoint> findAndSortCurrentBendPoints(List<BendingPlan> activeBendingPlans, int index)
        {
            List<BendPoint> bps = new List<BendPoint>();
            foreach (BendingPlan bpl in activeBendingPlans)
            {
                foreach (BendPoint bp in bpl.bendingPoints)
                {
                    if (bp.index <= index)
                    {
                        bp.usedChannel = bpl.usedChannel;
                        bps.Add(bp);
                    }
                }
            }
            bps.Sort((x, y) => x.index.CompareTo(y.index));

            return bps;
        }

        public List<TremoloPoint> addDetailsToTremoloPoints(List<TremoloPoint> tremoloPoints, int maxDistance)
        {
            List<TremoloPoint> tremPoints = new List<TremoloPoint>();
            float oldValue = 0.0f;
            int oldIndex = 0;
            foreach (TremoloPoint tp in tremoloPoints)
            {
                if ((tp.index - oldIndex) > maxDistance && !(oldValue == 0.0f && tp.value == 0.0f))
                {
                    //Add in-between points
                    for (int x = oldIndex + maxDistance; x < tp.index; x += maxDistance)
                    {
                        float value = oldValue + (tp.value - oldValue) * (((float)x - oldIndex) / ((float)tp.index - oldIndex));
                        tremPoints.Add(new TremoloPoint(value, x));

                    }
                }
                tremPoints.Add(tp);

                oldValue = tp.value;
                oldIndex = tp.index;
            }


            return tremPoints;
        }

        public BendingPlan createBendingPlan(List<BendPoint> bendPoints, int originalChannel, int usedChannel, int duration, int index, float resize, bool isVibrato)
        {
            int maxDistance = duration / 10; //After this there should be a pitchwheel event
            if (isVibrato) maxDistance = Math.Min(maxDistance, 60);

            if (bendPoints.Count == 0)
            {
                //Create Vibrato Plan
                bendPoints.Add(new BendPoint(0.0f, index));
                bendPoints.Add(new BendPoint(0.0f, index + duration));

            }

            List<BendPoint> bendingPoints = new List<BendPoint>();


            //Resize the points according to (changed) note duration
            foreach (BendPoint bp in bendPoints)
            {
                bp.index = (int)(index + ((bp.index - index) * resize));
                bp.usedChannel = usedChannel;
            }

            int old_pos = index;
            float old_value = 0.0f;
            bool start = true;
            int vibratoSize = 0;
            int vibratoChange = 0;
            if (isVibrato) vibratoSize = 12;
            if (isVibrato) vibratoChange = 6;
            int vibrato = 0;
            foreach (BendPoint bp in bendPoints)
            {
                if ((bp.index - old_pos) > maxDistance)
                {
                    //Add in-between points
                    for (int x = old_pos + maxDistance; x < bp.index; x += maxDistance)
                    {
                        float value = old_value + (bp.value - old_value) * (((float)x - old_pos) / ((float)bp.index - old_pos));
                        bendingPoints.Add(new BendPoint(value + vibrato, x));
                        if (isVibrato && Math.Abs(vibrato) == vibratoSize) vibratoChange = -vibratoChange;
                        vibrato += vibratoChange;

                    }
                }
                if (start || bp.index != old_pos)
                {
                    if (isVibrato) bp.value += vibrato;
                    bendingPoints.Add(bp);

                }
                old_pos = bp.index;
                old_value = bp.value;
                if ((start || bp.index != old_pos) && isVibrato) old_value -= vibrato; //Add back, so not to be influenced by it
                start = false;
                if (isVibrato && Math.Abs(vibrato) == vibratoSize) vibratoChange = -vibratoChange;
                vibrato += vibratoChange;
            }
            if (Math.Abs(index + duration - old_pos) > maxDistance)
            {
                bendingPoints.Add(new BendPoint(old_value, index + duration));
            }

            return new BendingPlan(originalChannel, usedChannel, bendingPoints);
        }

    }
}