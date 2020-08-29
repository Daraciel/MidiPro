using System;
using System.Collections.Generic;
using MidiPro.Core.Midi;
using MidiPro.Core.Native.Enums;

namespace MidiPro.Core.Native
{
    public class Track
    {
        public string Name { get; set; }
        public int Patch { get; set; }
        public int Port { get; set; }
        public int Channel { get; set; }
        public int Capo { get; set; }
        public PlaybackStates PlaybackState { get; set; }
        public int[] Tuning { get; set; }
        public List<Note> Notes { get; set; }
        public List<TremoloPoint> TremoloPoints { get; set; }


        private List<int[]> _volumeChanges;


        public Track()
        {
            Name = string.Empty;
            Patch = 0;
            Port = 0;
            Channel = 0;
            Capo = 0;
            PlaybackState = PlaybackStates.Def;
            Tuning = new int[] { 40, 45, 50, 55, 59, 64 };
            Notes = new List<Note>();
            TremoloPoints = new List<TremoloPoint>();
            _volumeChanges = new List<int[]>();
        }

        public MidiTrack GetMidi()
        {
            var midiTrack = new MidiTrack();
            midiTrack.Messages.Add(new MidiMessage("midi_port", new string[] { "" + Port }, 0));
            midiTrack.Messages.Add(new MidiMessage("track_name", new string[] { Name }, 0));
            midiTrack.Messages.Add(new MidiMessage("program_change", new string[] { "" + Channel, "" + Patch }, 0));


            List<int[]> noteOffs = new List<int[]>();
            List<int[]> channelConnections = new List<int[]>(); //For bending and trembar: [original Channel, artificial Channel, index at when to delete artificial]
            List<BendingPlan> activeBendingPlans = new List<BendingPlan>();
            int currentIndex = 0;
            Note temp = new Note();
            temp.Index = Notes[Notes.Count - 1].Index + Notes[Notes.Count - 1].Duration;
            temp.Str = -2;
            Notes.Add(temp);

            TremoloPoints = AddDetailsToTremoloPoints(TremoloPoints, 60);

            //var _notes = addSlidesToNotes(notes); //Adding slide notes here, as they should not appear as extra notes during playback

            foreach (Note n in Notes)
            {
                noteOffs.Sort((x, y) => x[0].CompareTo(y[0]));



                //Check for active bendings in progress
                List<BendPoint> currentBPs = FindAndSortCurrentBendPoints(activeBendingPlans, n.Index);
                float tremBarChange = 0.0f;
                foreach (BendPoint bp in currentBPs)
                {
                    //Check first if there is a note_off event happening in the meantime..
                    List<int[]> newNoteOffs = new List<int[]>();
                    foreach (int[] noteOff in noteOffs)
                    {
                        if (noteOff[0] <= bp.Index) //between last and this note, a note off event should occur
                        {
                            midiTrack.Messages.Add(
                                new MidiMessage("note_off",
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
                    List<TremoloPoint> newTremPoints = new List<TremoloPoint>();

                    foreach (TremoloPoint tp in TremoloPoints)
                    {
                        if (tp.Index <= bp.Index) //between last and this note, a note off event should occur
                        {
                            tremBarChange = tp.Value;
                        }
                        else
                        {
                            newTremPoints.Add(tp);
                        }
                    }
                    TremoloPoints = newTremPoints;

                    //Check if there are active volume changes
                    List<int[]> newVolumeChanges = new List<int[]>();
                    foreach (int[] vc in _volumeChanges)
                    {
                        if (vc[0] <= bp.Index) //between last and this note, a volume change event should occur
                        { //channel control value
                            midiTrack.Messages.Add(
                   new MidiMessage("control_change",
                   new string[] { "" + bp.UsedChannel, "7", "" + vc[1] }, vc[0] - currentIndex));
                            currentIndex = vc[0];
                        }
                        else
                        {
                            newVolumeChanges.Add(vc);
                        }
                    }
                    _volumeChanges = newVolumeChanges;

                    midiTrack.Messages.Add(
                   new MidiMessage("pitchwheel",
                   new string[] { "" + bp.UsedChannel, "" + (int)((bp.Value + tremBarChange) * 25.6f) }, bp.Index - currentIndex));
                    currentIndex = bp.Index;
                }

                //Delete no longer active Bending Plans
                List<BendingPlan> final = new List<BendingPlan>();
                foreach (BendingPlan bpl in activeBendingPlans)
                {

                    BendingPlan newBpl = new BendingPlan(bpl.OriginalChannel, bpl.UsedChannel, new List<BendPoint>());
                    foreach (BendPoint bp in bpl.BendingPoints)
                    {
                        if (bp.Index > n.Index)
                        {
                            newBpl.BendingPoints.Add(bp);
                        }
                    }
                    if (newBpl.BendingPoints.Count > 0)
                    {
                        final.Add(newBpl);
                    }
                    else //That bending plan has finished
                    {
                        midiTrack.Messages.Add(new MidiMessage("pitchwheel", new string[] { "" + bpl.UsedChannel, "-128" }, 0));
                        midiTrack.Messages.Add(new MidiMessage("control_change", new string[] { "" + bpl.UsedChannel, "101", "127" }, 0));
                        midiTrack.Messages.Add(new MidiMessage("control_change", new string[] { "" + bpl.UsedChannel, "10", "127" }, 0));

                        //Remove the channel from channelConnections
                        List<int[]> newChannelConnections = new List<int[]>();
                        foreach (int[] cc in channelConnections)
                        {
                            if (cc[1] != bpl.UsedChannel) newChannelConnections.Add(cc);
                        }
                        channelConnections = newChannelConnections;

                        NativeFormat.AvailableChannels[bpl.UsedChannel] = true;
                    }
                }

                activeBendingPlans = final;




                var activeChannels = GetActiveChannels(channelConnections);
                List<TremoloPoint> _newTremPoints = new List<TremoloPoint>();
                foreach (TremoloPoint tp in TremoloPoints)
                {
                    if (tp.Index <= n.Index) //between last and this note, a trembar event should occur
                    {
                        var value = tp.Value * 25.6f;
                        value = Math.Min(Math.Max(value, -8192), 8191);
                        foreach (int ch in activeChannels)
                        {
                            midiTrack.Messages.Add(
                     new MidiMessage("pitchwheel",
                     new string[] { "" + ch, "" + (int)(value) }, tp.Index - currentIndex));
                            currentIndex = tp.Index;
                        }
                    }
                    else
                    {
                        _newTremPoints.Add(tp);
                    }
                }
                TremoloPoints = _newTremPoints;


                //Check if there are active volume changes
                List<int[]> _newVolumeChanges = new List<int[]>();
                foreach (int[] vc in _volumeChanges)
                {
                    if (vc[0] <= n.Index) //between last and this note, a volume change event should occur
                    {

                        foreach (int ch in activeChannels)
                        {
                            midiTrack.Messages.Add(
               new MidiMessage("control_change",
               new string[] { "" + ch, "7", "" + vc[1] }, vc[0] - currentIndex));
                            currentIndex = vc[0];
                        }
                    }
                    else
                    {
                        _newVolumeChanges.Add(vc);
                    }
                }
                _volumeChanges = _newVolumeChanges;


                List<int[]> myTemp = new List<int[]>();
                foreach (int[] noteOff in noteOffs)
                {
                    if (noteOff[0] <= n.Index) //between last and this note, a note off event should occur
                    {
                        midiTrack.Messages.Add(
                            new MidiMessage("note_off",
                            new string[] { "" + noteOff[2], "" + noteOff[1], "0" }, noteOff[0] - currentIndex));
                        currentIndex = noteOff[0];
                    }
                    else
                    {
                        myTemp.Add(noteOff);
                    }
                }
                noteOffs = myTemp;

                int velocity = n.Velocity;
                int note;

                if (n.Str == -2) break; //Last round

                //if (n.str - 1 < 0) Debug.Log("String was -1");
                //if (n.str - 1 >= tuning.Length && tuning.Length != 0) Debug.Log("String was higher than string amount (" + n.str + ")");
                if (Tuning.Length > 0) note = Tuning[n.Str - 1] + Capo + n.Fret;
                else
                {
                    note = Capo + n.Fret;
                }
                if (n.Harmonic != HarmonicTypes.None) //Has Harmonics
                {
                    int harmonicNote = GetHarmonic(Tuning[n.Str - 1], n.Fret, Capo, n.HarmonicFret, n.Harmonic);
                    note = harmonicNote;
                }

                int noteChannel = Channel;

                if (n.BendPoints.Count > 0) //Has Bending
                {
                    int usedChannel = TryToFindChannel();
                    if (usedChannel == -1) usedChannel = Channel;
                    NativeFormat.AvailableChannels[usedChannel] = false;
                    channelConnections.Add(new int[] { Channel, usedChannel, n.Index + n.Duration });
                    midiTrack.Messages.Add(new MidiMessage("program_change", new string[] { "" + usedChannel, "" + Patch }, n.Index - currentIndex));
                    noteChannel = usedChannel;
                    currentIndex = n.Index;
                    activeBendingPlans.Add(CreateBendingPlan(n.BendPoints, Channel, usedChannel, n.Duration, n.Index, n.ResizeValue, n.IsVibrato));
                }

                if (n.IsVibrato && n.BendPoints.Count == 0) //Is Vibrato & No Bending
                {
                    int usedChannel = Channel;
                    activeBendingPlans.Add(CreateBendingPlan(n.BendPoints, Channel, usedChannel, n.Duration, n.Index, n.ResizeValue, true));

                }

                if (n.Fading != Fadings.None) //Fading
                {
                    _volumeChanges = CreateVolumeChanges(n.Index, n.Duration, n.Velocity, n.Fading);
                }

                midiTrack.Messages.Add(new MidiMessage("note_on", new string[] { "" + noteChannel, "" + note, "" + n.Velocity }, n.Index - currentIndex));
                currentIndex = n.Index;

                if (n.BendPoints.Count > 0) //Has Bending cont.
                {
                    midiTrack.Messages.Add(new MidiMessage("control_change", new string[] { "" + noteChannel, "101", "0" }, 0));
                    midiTrack.Messages.Add(new MidiMessage("control_change", new string[] { "" + noteChannel, "100", "0" }, 0));
                    midiTrack.Messages.Add(new MidiMessage("control_change", new string[] { "" + noteChannel, "6", "6" }, 0));
                    midiTrack.Messages.Add(new MidiMessage("control_change", new string[] { "" + noteChannel, "38", "0" }, 0));


                }

                noteOffs.Add(new int[] { n.Index + n.Duration, note, noteChannel });

            }




            midiTrack.Messages.Add(new MidiMessage("end_of_track", new string[] { }, 0));
            return midiTrack;
        }

        private List<Note> AddSlidesToNotes(List<Note> notes)
        {
            List<Note> ret = new List<Note>();
            int index = -1;
            foreach (Note n in notes)
            {
                index++;
                bool skipWrite = false;

                if ((n.SlideInFromBelow && n.Str > 1) || n.SlideInFromAbove)
                {
                    int myFret = n.Fret;
                    int start = n.SlideInFromAbove ? myFret + 4 : Math.Max(1, myFret - 4);
                    int beginIndex = n.Index - 960 / 4; //16th before
                    int lengthEach = (960 / 4) / Math.Abs(myFret - start);
                    for (int x = 0; x < Math.Abs(myFret - start); x++)
                    {
                        Note newOne = new Note(n);
                        newOne.Duration = lengthEach;
                        newOne.Index = beginIndex + x * lengthEach;
                        newOne.Fret = start + (n.SlideInFromAbove ? -x : +x);
                        ret.Add(newOne);
                    }
                }

                if ((n.SlideOutDownwards && n.Str > 1) || n.SlideOutUpwards)
                {
                    int myFret = n.Fret;
                    int end = n.SlideOutUpwards ? myFret + 4 : Math.Max(1, myFret - 4);
                    int beginIndex = (n.Index + n.Duration) - 960 / 4; //16th before
                    int lengthEach = (960 / 4) / Math.Abs(myFret - end);
                    n.Duration -= 960 / 4;
                    ret.Add(n); skipWrite = true;
                    for (int x = 0; x < Math.Abs(myFret - end); x++)
                    {
                        Note newOne = new Note(n);
                        newOne.Duration = lengthEach;
                        newOne.Index = beginIndex + x * lengthEach;
                        newOne.Fret = myFret + (n.SlideOutDownwards ? -x : +x);
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

        private List<int[]> CreateVolumeChanges(int index, int duration, int velocity, Fadings fading)
        {
            int segments = 20;
            List<int[]> changes = new List<int[]>();
            if (fading == Fadings.FadeIn || fading == Fadings.FadeOut)
            {
                int step = velocity / segments;
                int val = fading == Fadings.FadeIn ? 0 : velocity;
                if (fading == Fadings.FadeOut) step = (int)(-step * 1.25f);

                for (int x = index; x < index + duration; x += (duration / segments))
                {
                    changes.Add(new int[] { x, Math.Min(127, Math.Max(0, val)) });
                    val += step;
                }

            }

            if (fading == Fadings.VolumeSwell)
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

        private List<int> GetActiveChannels(List<int[]> channelConnections)
        {
            List<int> retVal = new List<int>();
            retVal.Add(Channel);
            foreach (int[] cc in channelConnections)
            {
                retVal.Add(cc[1]);
            }
            return retVal;
        }

        public int TryToFindChannel()
        {
            int cnt = 0;
            foreach (bool available in NativeFormat.AvailableChannels)
            {
                if (available) return cnt;
                cnt++;
            }
            return -1;
        }

        public int GetHarmonic(int baseTone, int fret, int capo, float harmonicFret, HarmonicTypes type)
        {
            int val = 0;
            //Capo, base tone and fret (if not natural harmonic) shift the harmonics simply
            val = val + baseTone + capo;
            if (type != HarmonicTypes.Natural)
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


        public List<BendPoint> FindAndSortCurrentBendPoints(List<BendingPlan> activeBendingPlans, int index)
        {
            List<BendPoint> bps = new List<BendPoint>();
            foreach (BendingPlan bpl in activeBendingPlans)
            {
                foreach (BendPoint bp in bpl.BendingPoints)
                {
                    if (bp.Index <= index)
                    {
                        bp.UsedChannel = bpl.UsedChannel;
                        bps.Add(bp);
                    }
                }
            }
            bps.Sort((x, y) => x.Index.CompareTo(y.Index));

            return bps;
        }

        public List<TremoloPoint> AddDetailsToTremoloPoints(List<TremoloPoint> tremoloPoints, int maxDistance)
        {
            List<TremoloPoint> tremPoints = new List<TremoloPoint>();
            float oldValue = 0.0f;
            int oldIndex = 0;
            foreach (TremoloPoint tp in tremoloPoints)
            {
                if ((tp.Index - oldIndex) > maxDistance && !(oldValue == 0.0f && tp.Value == 0.0f))
                {
                    //Add in-between points
                    for (int x = oldIndex + maxDistance; x < tp.Index; x += maxDistance)
                    {
                        float value = oldValue + (tp.Value - oldValue) * (((float)x - oldIndex) / ((float)tp.Index - oldIndex));
                        tremPoints.Add(new TremoloPoint(value, x));

                    }
                }
                tremPoints.Add(tp);

                oldValue = tp.Value;
                oldIndex = tp.Index;
            }


            return tremPoints;
        }

        public BendingPlan CreateBendingPlan(List<BendPoint> bendPoints, int originalChannel, int usedChannel, int duration, int index, float resize, bool isVibrato)
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
                bp.Index = (int)(index + ((bp.Index - index) * resize));
                bp.UsedChannel = usedChannel;
            }

            int oldPos = index;
            float oldValue = 0.0f;
            bool start = true;
            int vibratoSize = 0;
            int vibratoChange = 0;
            if (isVibrato) vibratoSize = 12;
            if (isVibrato) vibratoChange = 6;
            int vibrato = 0;
            foreach (BendPoint bp in bendPoints)
            {
                if ((bp.Index - oldPos) > maxDistance)
                {
                    //Add in-between points
                    for (int x = oldPos + maxDistance; x < bp.Index; x += maxDistance)
                    {
                        float value = oldValue + (bp.Value - oldValue) * (((float)x - oldPos) / ((float)bp.Index - oldPos));
                        bendingPoints.Add(new BendPoint(value + vibrato, x));
                        if (isVibrato && Math.Abs(vibrato) == vibratoSize) vibratoChange = -vibratoChange;
                        vibrato += vibratoChange;

                    }
                }
                if (start || bp.Index != oldPos)
                {
                    if (isVibrato) bp.Value += vibrato;
                    bendingPoints.Add(bp);

                }
                oldPos = bp.Index;
                oldValue = bp.Value;
                if ((start || bp.Index != oldPos) && isVibrato) oldValue -= vibrato; //Add back, so not to be influenced by it
                start = false;
                if (isVibrato && Math.Abs(vibrato) == vibratoSize) vibratoChange = -vibratoChange;
                vibrato += vibratoChange;
            }
            if (Math.Abs(index + duration - oldPos) > maxDistance)
            {
                bendingPoints.Add(new BendPoint(oldValue, index + duration));
            }

            return new BendingPlan(originalChannel, usedChannel, bendingPoints);
        }

    }
}