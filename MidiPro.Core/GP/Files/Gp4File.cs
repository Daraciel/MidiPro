using System;
using System.Collections.Generic;
using MidiPro.Core.BE;
using MidiPro.Core.Enums;
using MidiPro.Core.GP.Beat;
using MidiPro.Core.GP.Enums;
using MidiPro.Core.GP.Harmonic;

namespace MidiPro.Core.GP.Files
{
    public class Gp4File : GpFile
    {
        //Members of GPFile
        /*
        public string version;
        public int[] versionTuple;
        public string title = "";
        public string subtitle = "";
        public string interpret = "";
        public string album = "";
        public string author = "";
        public string copyright = "";
        public string tab_author = "";
        public string instructional = "";
        public Lyrics lyrics;
        public int tempo;
        public List<Track> tracks = new List<Track>();
        public List<MeasureHeader> measureHeaders = new List<MeasureHeader>();
        public TripletFeel TripletFeel;
        */
        public string[] Notice;

        public KeySignatures Key;
        public MidiChannel[] Channels;
        public int MeasureCount;
        public int TrackCount;

        public RepeatGroup CurrentRepeatGroup = new RepeatGroup();


        public Gp4File(byte[] data) : base()
        {
            GpBase.Pointer = 0;
            GpBase.Data = data;
        }

        public void AddMeasureHeader(MeasureHeader header)
        {
            //header.Song = this;
            MeasureHeaders.Add(header);
            if (header.IsRepeatOpen || (header.RepeatAlternatives.Count > 0 && CurrentRepeatGroup.IsClosed && header.RepeatAlternatives[0] <= 0))
            {
                CurrentRepeatGroup = new RepeatGroup();
            }
            CurrentRepeatGroup.AddMeasureHeader(header);
        }

        public void AddTrack(Track track)
        {
            track.Song = this;
            Tracks.Add(track);
        }


        public override void ReadSong()
        {
            //HEADERS
            //VERSION
            Version = ReadVersion();
            VersionTuple = ReadVersionTuple();
            Clipboard = ReadClipboard();

            //INFORMATION ABOUT THE PIECE
            ReadInfo();
            TripletFeel = GpBase.ReadBool()[0] ? TripletFeels.Eigth : TripletFeels.None;
            ReadLyrics();
            Tempo = GpBase.ReadInt()[0];
            Key = (KeySignatures)(GpBase.ReadInt()[0] * 10); //key + 0
            GpBase.ReadSignedByte(); //octave
            ReadMidiChannels();
            MeasureCount = GpBase.ReadInt()[0];
            TrackCount = GpBase.ReadInt()[0];

            ReadMeasureHeaders(MeasureCount);
            ReadTracks(TrackCount, Channels);
            ReadMeasures();
        }

        public Clipboard ReadClipboard()
        {
            if (!IsClipboard()) return null;
            var clipboard = new Clipboard();
            clipboard.StartMeasure = GpBase.ReadInt()[0];
            clipboard.StopMeasure = GpBase.ReadInt()[0];
            clipboard.StartTrack = GpBase.ReadInt()[0];
            clipboard.StopTrack = GpBase.ReadInt()[0];
            return clipboard;
        }

        private bool IsClipboard()
        {
            return Version.StartsWith("CLIPBOARD");
        }
        private string ReadVersion()
        {
            var version = GpBase.ReadByteSizeString(30);
            return version;
        }

        private int[] ReadVersionTuple() //bl0.12
        {
            if (Version.Equals("")) return new int[] { 4, 0 };
            var tuple = Version.Substring(Version.Length - 4).Split('.');
            return new int[] { Convert.ToInt32(tuple[0]), Convert.ToInt32(tuple[1]) };
        }

        private void ReadMeasures()
        {
            /*Read measures.

            Measures are written in the following order:

            - measure 1/track 1
            - measure 1/track 2
            - ...
            - measure 1/track m
            - measure 2/track 1
            - measure 2/track 2
            - ...
            - measure 2/track m
            - ...
            - measure n/track 1
            - measure n/track 2
            - ...
            - measure n/track m
    */
            var tempo = new Tempo(Tempo);
            var start = (int)Durations.QuarterTime;
            foreach (MeasureHeader header in MeasureHeaders)
            {

                header.Start = start;
                foreach (Track track in Tracks)
                {
                    var measure = new Measure(track, header);
                    tempo = header.Tempo;
                    track.Measures.Add(measure);
                    ReadMeasure(measure);
                }
                header.Tempo = tempo;
                start += header.Length;
            }

        }

        private void ReadMeasure(Measure measure)
        {
            /*The measure is written as number of beats followed by sequence
            of beats.*/
            var start = measure.Start;
            var voice = measure.Voices[0];
            ReadVoice(start, voice);
        }

        private void ReadVoice(int start, Voice voice)
        {
            //TODO: The pointer is 13 bytes too early here (when reading for measure 0xa of track 0x2, beats should return 1, not 898989)
            var beats = GpBase.ReadInt()[0];
            for (int beat = 0; beat < beats; beat++)
            {
                start += ReadBeat(start, voice);
            }
        }

        private int ReadBeat(int start, Voice voice)
        {
            /* The first byte is the beat flags. It lists the data present in
            the current beat:

            - *0x01*: dotted notes
            - *0x02*: presence of a chord diagram
            - *0x04*: presence of a text
            - *0x08*: presence of effects
            - *0x10*: presence of a mix table change event
            - *0x20*: the beat is a n-tuplet
            - *0x40*: status: True if the beat is empty of if it is a rest
            - *0x80*: *blank*

            Flags are followed by:

            - Status: :ref:`byte`. If flag at *0x40* is true, read one byte.
              If value of the byte is *0x00* then beat is empty, if value is
              *0x02* then the beat is rest.

            - Beat duration: :ref:`byte`. See :meth:`readDuration`.

            - Chord diagram. See :meth:`readChord`.

            - Text. See :meth:`readText`.

            - Beat effects. See :meth:`readBeatEffects`.

            - Mix table change effect. See :meth:`readMixTableChange`.*/

            var flags = GpBase.ReadByte()[0];
            var beat = GetBeat(voice, start);
            if ((flags & 0x40) != 0)
            {
                beat.Status = (BeatStatuses)((int)GpBase.ReadByte()[0]);
            }
            else
            {
                beat.Status = BeatStatuses.Normal;
            }
            var duration = ReadDuration(flags);
            var effect = new NoteEffect();
            if ((flags & 0x02) != 0) beat.Effect.Chord = ReadChord(voice.Measure.Track.Strings.Count);
            if ((flags & 0x04) != 0) beat.Text = ReadText();
            if ((flags & 0x08) != 0) beat.Effect = ReadBeatEffects(effect);
            if ((flags & 0x10) != 0)
            {
                var mixTableChange = ReadMixTableChange(voice.Measure);
                beat.Effect.MixTableChange = mixTableChange;
            }
            ReadNotes(voice.Measure.Track, beat, duration, effect);
            return (beat.Status != BeatStatuses.Empty) ? duration.Time() : 0;
        }

        private void ReadNotes(Track track, Beat.Beat beat, Duration duration, NoteEffect effect)
        {
            /* First byte lists played strings:

            - *0x01*: 7th string
            - *0x02*: 6th string
            - *0x04*: 5th string
            - *0x08*: 4th string
            - *0x10*: 3th string
            - *0x20*: 2th string
            - *0x40*: 1th string
            - *0x80*: *blank**/

            var stringFlags = GpBase.ReadByte()[0];
            foreach (var str in track.Strings)
            {
                if ((stringFlags & 1 << (7 - str.Number)) != 0)
                {
                    var note = new Note(beat);
                    beat.Notes.Add(note);
                    ReadNote(note, str, track);
                }
                beat.Duration = duration;
            }
        }

        private void ReadNote(Note note, GuitarString guitarString, Track track)
        {
            /*The first byte is note flags:

            - *0x01*: time-independent duration
            - *0x02*: heavy accentuated note
            - *0x04*: ghost note
            - *0x08*: presence of note effects
            - *0x10*: dynamics
            - *0x20*: fret
            - *0x40*: accentuated note
            - *0x80*: right hand or left hand fingering

            Flags are followed by:

            - Note type: :ref:`byte`. Note is normal if values is 1, tied if
              value is 2, dead if value is 3.

            - Time-independent duration: 2 :ref:`SignedBytes <signed-byte>`.
              Correspond to duration and tuplet. See :meth:`readDuration`
              for reference.

            - Note dynamics: :ref:`signed-byte`. See :meth:`unpackVelocity`.

            - Fret number: :ref:`signed-byte`. If flag at *0x20* is set then
              read fret number.

            - Fingering: 2 :ref:`SignedBytes <signed-byte>`. See
              :class:`guitarpro.models.Fingering`.

            - Note effects. See :meth:`readNoteEffects`.*/

            var flags = GpBase.ReadByte()[0];
            note.Str = guitarString.Number;
            note.Effect.GhostNote = ((flags & 0x04) != 0);
            if ((flags & 0x20) != 0) note.Type = (NoteTypes)(GpBase.ReadByte()[0]);
            if ((flags & 0x01) != 0)
            {
                note.Duration = GpBase.ReadSignedByte()[0];
                note.Tuplet = GpBase.ReadSignedByte()[0];
            }
            if ((flags & 0x10) != 0)
            {
                var dyn = GpBase.ReadSignedByte()[0];
                note.Velocity = UnpackVelocity(dyn);
            }
            if ((flags & 0x20) != 0)
            {
                int value;
                var fret = GpBase.ReadSignedByte()[0];
                if (note.Type == NoteTypes.Tie) { value = GetTiedNoteValue(guitarString.Number, track); }
                else { value = fret; }
                note.Value = Math.Max(0, Math.Min(99, value));
            }
            if ((flags & 0x80) != 0)
            {
                note.Effect.LeftHandFinger = (Fingerings)GpBase.ReadSignedByte()[0];
                note.Effect.RightHandFinger = (Fingerings)GpBase.ReadSignedByte()[0];
            }
            if ((flags & 0x08) != 0)
            {
                note.Effect = ReadNoteEffects(note);
                if (note.Effect.IsHarmonic && note.Effect.Harmonic is TappedHarmonic)
                {
                    note.Effect.Harmonic.Fret = note.Value + 12;
                }
            }

        }

        private NoteEffect ReadNoteEffects(Note note)
        {
            /*First byte is note effects flags:

            - *0x01*: bend presence
            - *0x02*: hammer-on/pull-off
            - *0x04*: slide
            - *0x08*: let-ring
            - *0x10*: grace note presence

            Flags are followed by:

            - Bend. See :meth:`readBend`.

            - Grace note. See :meth:`readGrace`.*/

            var noteEffect = note.Effect;
            if (noteEffect == null) noteEffect = new NoteEffect();
            var flags1 = GpBase.ReadSignedByte()[0];
            var flags2 = GpBase.ReadSignedByte()[0];

            noteEffect.Hammer = ((flags1 & 0x02) != 0);
            noteEffect.LetRing = ((flags1 & 0x08) != 0);
            noteEffect.Staccato = ((flags2 & 0x01) != 0);
            noteEffect.PalmMute = ((flags2 & 0x02) != 0);
            noteEffect.Vibrato = ((flags2 & 0x40) != 0) || noteEffect.Vibrato;

            if ((flags1 & 0x01) != 0) noteEffect.Bend = ReadBend();
            if ((flags1 & 0x10) != 0) noteEffect.Grace = ReadGrace();
            if ((flags2 & 0x04) != 0) noteEffect.TremoloPicking = ReadTremoloPicking();
            if ((flags2 & 0x08) != 0) noteEffect.Slides = ReadSlides();
            if ((flags2 & 0x10) != 0) noteEffect.Harmonic = ReadHarmonic(note);
            if ((flags2 & 0x20) != 0) noteEffect.Trill = ReadTrill();

            return noteEffect;

        }

        private TremoloPickingEffect ReadTremoloPicking()
        {
            var value = GpBase.ReadSignedByte()[0];
            var tp = new TremoloPickingEffect();
            tp.Duration.Value = FromTremoloValue(value);
            return tp;
        }

        private int FromTremoloValue(sbyte value)
        {
            switch (value)
            {
                case 1:
                    return (int)Durations.Eigth;
                case 2:
                    return (int)Durations.Sixteenth;
                case 3:
                    return (int)Durations.ThirtySecond;
            }
            return 8;
        }

        private List<SlideTypes> ReadSlides()
        {
            var retVal = new List<SlideTypes>();
            retVal.Add((SlideTypes)GpBase.ReadSignedByte()[0]);
            return retVal;
        }

        private HarmonicEffect ReadHarmonic(Note note)
        {
            /*Harmonic is encoded in :ref:`signed-byte`. Values correspond to:

            - *1*: natural harmonic
            - *3*: tapped harmonic
            - *4*: pinch harmonic
            - *5*: semi-harmonic
            - *15*: artificial harmonic on (*n + 5*)th fret
            - *17*: artificial harmonic on (*n + 7*)th fret
            - *22*: artificial harmonic on (*n + 12*)th fret
    */
            var harmonicType = GpBase.ReadSignedByte()[0];
            HarmonicEffect harmonic = null;
            switch (harmonicType)
            {
                case 1:
                    harmonic = new NaturalHarmonic(); break;
                case 3:
                    harmonic = new TappedHarmonic(); break;
                case 4:
                    harmonic = new PinchHarmonic(); break;
                case 5:
                    harmonic = new SemiHarmonic(); break;
                case 15:
                    var pitch = new PitchClass((note.RealValue() + 7) % 12, -1, "", "", 7.0f);
                    var octave = Octaves.Ottava;
                    harmonic = new ArtificialHarmonic(pitch, octave);
                    break;
                case 17:
                    pitch = new PitchClass(note.RealValue(), -1, "", "", 12.0f);
                    octave = Octaves.Quindicesima;
                    harmonic = new ArtificialHarmonic(pitch, octave);
                    break;
                case 22:
                    pitch = new PitchClass(note.RealValue(), -1, "", "", 5.0f);
                    octave = Octaves.Ottava;
                    harmonic = new ArtificialHarmonic(pitch, octave);
                    break;
            }
            return harmonic;
        }

        private TrillEffect ReadTrill()
        {
            var trill = new TrillEffect();
            trill.Fret = GpBase.ReadSignedByte()[0];
            trill.Duration.Value = FromTrillPeriod(GpBase.ReadSignedByte()[0]);
            return trill;
        }

        private int FromTrillPeriod(sbyte period)
        {
            switch (period)
            {
                case 1:
                    return (int)Durations.Sixteenth;
                case 2:
                    return (int)Durations.ThirtySecond;
                case 3:
                    return (int)Durations.SixtyFourth;
            }
            return (int)Durations.Sixteenth;
        }

        private GraceEffect ReadGrace()
        {
            /*- Fret: :ref:`signed-byte`. Number of fret.

            - Dynamic: :ref:`byte`. Dynamic of a grace note, as in
              :attr:`guitarpro.models.Note.velocity`.

            - Transition: :ref:`byte`. See
              :class:`guitarpro.models.GraceEffectTransition`.

            - Duration: :ref:`byte`. Values are:

              - *1*: Thirty-second note.
              - *2*: Twenty-fourth note.
              - *3*: Sixteenth note.*/
            var grace = new GraceEffect();
            grace.Fret = GpBase.ReadSignedByte()[0];
            grace.Velocity = UnpackVelocity(GpBase.ReadByte()[0]);
            grace.Duration = 1 << (7 - GpBase.ReadByte()[0]);
            grace.IsDead = (grace.Fret == -1);
            grace.IsOnBeat = false;
            grace.Transition = (GraceEffectTransitions)GpBase.ReadSignedByte()[0];
            return grace;
        }

        private BendEffect ReadBend()
        {
            /*Encoded as:

            -Bend type: :ref:`signed - byte`. See
               :class:`guitarpro.models.BendType`.

            - Bend value: :ref:`int`.

            - Number of bend points: :ref:`int`.

            - List of points.Each point consists of:

              * Position: :ref:`int`. Shows where point is set along
                *x*-axis.

              * Value: :ref:`int`. Shows where point is set along *y*-axis.

              * Vibrato: :ref:`bool`. */
            var bendEffect = new BendEffect();
            bendEffect.Type = (BendTypes)GpBase.ReadSignedByte()[0];
            bendEffect.Value = GpBase.ReadInt()[0];
            var pointCount = GpBase.ReadInt()[0];
            for (int x = 0; x < pointCount; x++)
            {
                var position = (int)Math.Round(GpBase.ReadInt()[0] * BendEffect.MaxPosition / (float)GpBase.BendPosition);
                var value = (int)Math.Round(GpBase.ReadInt()[0] * BendEffect.SemitoneLength / (float)GpBase.BendSemitone);
                var vibrato = GpBase.ReadBool()[0];
                bendEffect.Points.Add(new BendPoint(position, value, vibrato));
            }
            return bendEffect;
        }

        private int GetTiedNoteValue(int stringIndex, Track track)
        {
            for (int measure = track.Measures.Count - 1; measure >= 0; measure--)
            {
                for (int voice = track.Measures[measure].Voices.Count - 1; voice >= 0; voice--)
                {
                    foreach (var beat in track.Measures[measure].Voices[voice].Beats)
                    {
                        if (beat.Status != BeatStatuses.Empty)
                        {
                            foreach (var note in beat.Notes)
                            {
                                if (note.Str == stringIndex) return note.Value;
                            }
                        }
                    }
                }
            }
            return -1;
        }

        private int UnpackVelocity(sbyte dyn)
        {
            return (Velocities.MinVelocity +
                Velocities.VelocityIncrement * dyn -
                Velocities.VelocityIncrement);
        }
        private int UnpackVelocity(byte dyn)
        {
            return (Velocities.MinVelocity +
                Velocities.VelocityIncrement * dyn -
                Velocities.VelocityIncrement);
        }


        private MixTableChange ReadMixTableChange(Measure measure)
        {
            var tableChange = new MixTableChange();
            ReadMixTableChangeValues(tableChange, measure);
            ReadMixTableChangeDurations(tableChange);
            ReadMixTableChangeFlags(tableChange);
            return tableChange;
        }

        private void ReadMixTableChangeFlags(MixTableChange tableChange)
        {
            /* The meaning of flags:

            - *0x01*: change volume for all tracks
            - *0x02*: change balance for all tracks
            - *0x04*: change chorus for all tracks
            - *0x08*: change reverb for all tracks
            - *0x10*: change phaser for all tracks
            - *0x20*: change tremolo for all tracks*/

            var flags = GpBase.ReadSignedByte()[0];
            if (tableChange.Volume != null) tableChange.Volume.AllTracks = ((flags & 0x01) != 0);
            if (tableChange.Balance != null) tableChange.Balance.AllTracks = ((flags & 0x02) != 0);
            if (tableChange.Chorus != null) tableChange.Chorus.AllTracks = ((flags & 0x04) != 0);
            if (tableChange.Reverb != null) tableChange.Reverb.AllTracks = ((flags & 0x08) != 0);
            if (tableChange.Phaser != null) tableChange.Phaser.AllTracks = ((flags & 0x10) != 0);
            if (tableChange.Tremolo != null) tableChange.Tremolo.AllTracks = ((flags & 0x20) != 0);

        }

        private void ReadMixTableChangeDurations(MixTableChange tableChange)
        {
            if (tableChange.Volume != null) tableChange.Volume.Duration = GpBase.ReadSignedByte()[0];
            if (tableChange.Balance != null) tableChange.Balance.Duration = GpBase.ReadSignedByte()[0];
            if (tableChange.Chorus != null) tableChange.Chorus.Duration = GpBase.ReadSignedByte()[0];
            if (tableChange.Reverb != null) tableChange.Reverb.Duration = GpBase.ReadSignedByte()[0];
            if (tableChange.Phaser != null) tableChange.Phaser.Duration = GpBase.ReadSignedByte()[0];
            if (tableChange.Tremolo != null) tableChange.Tremolo.Duration = GpBase.ReadSignedByte()[0];
            if (tableChange.Tempo != null)
            {
                tableChange.Tempo.Duration = GpBase.ReadSignedByte()[0];
                tableChange.HideTempo = false;
            }

        }

        private void ReadMixTableChangeValues(MixTableChange tableChange, Measure measure)
        {
            var instrument = GpBase.ReadSignedByte()[0];
            var volume = GpBase.ReadSignedByte()[0];
            var balance = GpBase.ReadSignedByte()[0];
            var chorus = GpBase.ReadSignedByte()[0];
            var reverb = GpBase.ReadSignedByte()[0];
            var phaser = GpBase.ReadSignedByte()[0];
            var tremolo = GpBase.ReadSignedByte()[0];
            var tempo = GpBase.ReadInt()[0];
            if (instrument >= 0)
            {
                tableChange.Instrument = new MixTableItem(instrument);
            }
            if (volume >= 0)
            {
                tableChange.Volume = new MixTableItem(volume);
            }
            if (balance >= 0)
            {
                tableChange.Balance = new MixTableItem(balance);
            }
            if (chorus >= 0)
            {
                tableChange.Chorus = new MixTableItem(chorus);
            }
            if (reverb >= 0)
            {
                tableChange.Reverb = new MixTableItem(reverb);
            }
            if (phaser >= 0)
            {
                tableChange.Phaser = new MixTableItem(phaser);
            }
            if (tremolo >= 0)
            {
                tableChange.Tremolo = new MixTableItem(tremolo);
            }
            if (tempo >= 0)
            {
                tableChange.Tempo = new MixTableItem(tempo);
                measure.Tempo.Value = tempo;
            }

        }

        private BeatEffect ReadBeatEffects(NoteEffect effect)
        {
            /*
             * The first byte is effects flags:

            - *0x01*: vibrato
            - *0x02*: wide vibrato
            - *0x04*: natural harmonic
            - *0x08*: artificial harmonic
            - *0x10*: fade in
            - *0x20*: tremolo bar or slap effect
            - *0x40*: beat stroke direction
            - *0x80*: *blank*

            - Tremolo bar or slap effect: :ref:`byte`. If it's 0 then
              tremolo bar should be read (see :meth:`readTremoloBar`). Else
              it's tapping and values of the byte map to:

              - *1*: tap
              - *2*: slap
              - *3*: pop

            - Beat stroke direction. See :meth:`readBeatStroke`.*/
            var beatEffect = new BeatEffect();
            var flags1 = GpBase.ReadSignedByte()[0];
            var flags2 = GpBase.ReadSignedByte()[0];
            //effect.vibrato = ((flags1 & 0x01) != 0) || effect.vibrato;
            beatEffect.Vibrato = ((flags1 & 0x02) != 0) || beatEffect.Vibrato;
            beatEffect.FadeIn = ((flags1 & 0x10) != 0);
            if ((flags1 & 0x20) != 0)
            {
                var value = GpBase.ReadSignedByte()[0];
                beatEffect.SlapEffect = (SlapEffects)value;

            }
            if ((flags2 & 0x04) != 0) beatEffect.TremoloBar = ReadTremoloBar();

            if ((flags1 & 0x40) != 0) beatEffect.Stroke = ReadBeatStroke();
            if ((flags2 & 0x02) != 0)
            {
                var direction = GpBase.ReadSignedByte()[0];
                beatEffect.PickStroke = (BeatStrokeDirections)direction;
            }
            return beatEffect;
        }

        private BeatStroke ReadBeatStroke()
        {
            var strokeDown = GpBase.ReadSignedByte()[0];
            var strokeUp = GpBase.ReadSignedByte()[0];
            if (strokeUp > 0)
            {
                return new BeatStroke(BeatStrokeDirections.Up, ToStrokeValue(strokeUp), 0.0f);
            }
            else
            {
                return new BeatStroke(BeatStrokeDirections.Down, ToStrokeValue(strokeDown), 0.0f);
            }
        }

        private int ToStrokeValue(sbyte value)
        {
            int result = 0;
            /*Unpack stroke value.

            Stroke value maps to:

            - *1*: hundred twenty-eighth
            - *2*: sixty-fourth
            - *3*: thirty-second
            - *4*: sixteenth
            - *5*: eighth
            - *6*: quarter*/
            switch (value)
            {
                case 1: 
                    result = (int)Durations.HundredTwentyEigth; 
                    break;
                case 2: 
                    result = (int)Durations.SixtyFourth; 
                    break;
                case 3: 
                    result = (int)Durations.ThirtySecond;
                    break;
                case 4: 
                    result = (int)Durations.Sixteenth; 
                    break;
                case 5: 
                    result = (int)Durations.Eigth;
                    break;
                case 6: 
                    result = (int)Durations.Quarter; 
                    break;
                default: 
                    result = (int)Durations.SixtyFourth; 
                    break;
            }

            return result;
        }

        private BendEffect ReadTremoloBar()
        {
            return ReadBend();
        }

        private BeatText ReadText()
        {
            var text = new BeatText();
            text.Value = GpBase.ReadIntByteSizeString();
            return text;
        }

        private Chord ReadChord(int stringCount)
        {
            var chord = new Chord(stringCount);
            chord.NewFormat = GpBase.ReadBool()[0];
            if (!chord.NewFormat)
            {
                ReadOldChord(chord);
            }
            else
            {
                ReadNewChord(chord);
            }
            if ((chord.Notes().Length) > 0) return chord;
            return null;
        }

        private void ReadOldChord(Chord chord)
        {
            /*Read chord diagram encoded in GP3 format.

            Chord diagram is read as follows:

            - Name: :ref:`int-byte-size-string`. Name of the chord, e.g.
              *Em*.

            - First fret: :ref:`int`. The fret from which the chord is
              displayed in chord editor.

            - List of frets: 6 :ref:`Ints <int>`. Frets are listed in order:
              fret on the string 1, fret on the string 2, ..., fret on the
              string 6. If string is untouched then the values of fret is
              *-1*.*/

            chord.Name = GpBase.ReadIntByteSizeString();
            chord.FirstFret = GpBase.ReadInt()[0];
            if (chord.FirstFret > 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    var fret = GpBase.ReadInt()[0];
                    if (i < chord.Strings.Length) chord.Strings[i] = fret;
                }
            }
        }

        private void ReadNewChord(Chord chord)
        {
            /*Read new-style (GP4) chord diagram.

            New-style chord diagram is read as follows:

            - Sharp: :ref:`bool`. If true, display all semitones as sharps,
              otherwise display as flats.

            - Blank space, 3 :ref:`Bytes <byte>`.

            - Root: :ref:`int`. Values are:

              * -1 for customized chords
              *  0: C
              *  1: C#
              * ...

            - Type: :ref:`int`. Determines the chord type as followed. See
              :class:`guitarpro.models.ChordType` for mapping.

            - Chord extension: :ref:`int`. See
              :class:`guitarpro.models.ChordExtension` for mapping.

            - Bass note: :ref:`int`. Lowest note of chord as in *C/Am*.

            - Tonality: :ref:`int`. See
              :class:`guitarpro.models.ChordAlteration` for mapping.

            - Add: :ref:`bool`. Determines if an "add" (added note) is
              present in the chord.

            - Name: :ref:`byte-size-string`. Max length is 22.

            - Fifth alteration: :ref:`int`. Maps to
              :class:`guitarpro.models.ChordAlteration`.

            - Ninth alteration: :ref:`int`. Maps to
              :class:`guitarpro.models.ChordAlteration`.

            - Eleventh alteration: :ref:`int`. Maps to
              :class:`guitarpro.models.ChordAlteration`.

            - List of frets: 6 :ref:`Ints <int>`. Fret values are saved as
              in default format.

            - Count of barres: :ref:`int`. Maximum count is 2.

            - Barre frets: 2 :ref:`Ints <int>`.

            - Barre start strings: 2 :ref:`Ints <int>`.

            - Barre end string: 2 :ref:`Ints <int>`.

            - Omissions: 7 :ref:`Bools <bool>`. If the value is true then
              note is played in chord.

            - Blank space, 1 :ref:`byte`.*/

            chord.Sharp = GpBase.ReadBool()[0];
            var intonation = chord.Sharp ? "sharp" : "flat";
            GpBase.Skip(3);
            chord.Root = new PitchClass(GpBase.ReadByte()[0], -1, "", intonation);
            chord.Type = (ChordTypes)GpBase.ReadByte()[0];
            chord.Extension = (ChordExtensions)GpBase.ReadByte()[0];
            chord.Bass = new PitchClass(GpBase.ReadInt()[0], -1, "", intonation);
            chord.Tonality = (ChordAlterations)GpBase.ReadInt()[0];
            chord.Add = GpBase.ReadBool()[0];
            chord.Name = GpBase.ReadByteSizeString(22);
            chord.Fifth = (ChordAlterations)GpBase.ReadByte()[0];
            chord.Ninth = (ChordAlterations)GpBase.ReadByte()[0];
            chord.Eleventh = (ChordAlterations)GpBase.ReadByte()[0];
            chord.FirstFret = GpBase.ReadInt()[0];
            for (int i = 0; i < 7; i++)
            {
                var fret = GpBase.ReadInt()[0];
                if (i < chord.Strings.Length) chord.Strings[i] = fret;
            }
            chord.Barres.Clear();
            var barresCount = GpBase.ReadByte()[0];
            var barreFrets = GpBase.ReadByte(5);
            var barreStarts = GpBase.ReadByte(5);
            var barreEnds = GpBase.ReadByte(5);

            for (int x = 0; x < Math.Min(5, (int)barresCount); x++)
            {
                var barre = new Barre(barreFrets[x], barreStarts[x], barreEnds[x]);
                chord.Barres.Add(barre);
            }
            chord.Omissions = GpBase.ReadBool(7);
            GpBase.Skip(1);
            List<Fingerings> f = new List<Fingerings>();
            for (int x = 0; x < 7; x++)
            {
                f.Add((Fingerings)GpBase.ReadSignedByte()[0]);
            }
            chord.Fingerings = f;
            chord.Show = GpBase.ReadBool()[0];
        }

        private Duration ReadDuration(byte flags)
        {
            /*Duration is composed of byte signifying duration and an integer
            that maps to :class:`guitarpro.models.Tuplet`.

            The byte maps to following values:

            - *-2*: whole note
            - *-1*: half note
            -  *0*: quarter note
            -  *1*: eighth note
            -  *2*: sixteenth note
            -  *3*: thirty-second note

            If flag at *0x20* is true, the tuplet is read.*/

            var duration = new Duration();
            duration.Value = 1 << (GpBase.ReadSignedByte()[0] + 2);
            duration.IsDotted = ((flags & 0x01) != 0);
            if ((flags & 0x20) != 0)
            {
                var iTuplet = GpBase.ReadInt()[0];
                switch (iTuplet)
                {
                    case 3: duration.Tuplet.Enters = 3; duration.Tuplet.Times = 2; break;
                    case 5: duration.Tuplet.Enters = 5; duration.Tuplet.Times = 4; break;
                    case 6: duration.Tuplet.Enters = 6; duration.Tuplet.Times = 4; break;
                    case 7: duration.Tuplet.Enters = 7; duration.Tuplet.Times = 4; break;
                    case 9: duration.Tuplet.Enters = 9; duration.Tuplet.Times = 8; break;
                    case 10: duration.Tuplet.Enters = 10; duration.Tuplet.Times = 8; break;
                    case 11: duration.Tuplet.Enters = 11; duration.Tuplet.Times = 8; break;
                    case 12: duration.Tuplet.Enters = 12; duration.Tuplet.Times = 8; break;
                }
            }
            return duration;
        }

        private Beat.Beat GetBeat(Voice voice, int start)
        {
            for (int x = voice.Beats.Count - 1; x >= 0; x--)
            {
                if (voice.Beats[x].Start == start) return voice.Beats[x];
            }
            var newBeat = new Beat.Beat(voice);
            newBeat.Start = start;
            voice.Beats.Add(newBeat);
            return newBeat;
        }

        private void ReadTracks(int trackCount, MidiChannel[] channels)
        {
            for (int i = 0; i < trackCount; i++)
            {
                Track track = new Track(this, i + 1, new List<GuitarString>(), new List<Measure>());
                ReadTrack(track, channels);
                this.Tracks.Add(track);
            }
        }


        private void ReadTrack(Track track, MidiChannel[] channels)
        {
            /*
             * Read track.

            The first byte is the track's flags. It presides the track's
            attributes:

            - *0x01*: drums track
            - *0x02*: 12 stringed guitar track
            - *0x04*: banjo track
            - *0x08*: *blank*
            - *0x10*: *blank*
            - *0x20*: *blank*
            - *0x40*: *blank*
            - *0x80*: *blank*

            Flags are followed by:

            - Name: :ref:`byte-size-string`. A 40 characters long string
              containing the track's name.

            - Number of strings: :ref:`int`. An integer equal to the number
                of strings of the track.

            - Tuning of the strings: List of 7 :ref:`Ints <int>`. The tuning
              of the strings is stored as a 7-integers table, the "Number of
              strings" first integers being really used. The strings are
              stored from the highest to the lowest.

            - Port: :ref:`int`. The number of the MIDI port used.

            - Channel. See :meth:`GP3File.readChannel`.

            - Number of frets: :ref:`int`. The number of frets of the
              instrument.

            - Height of the capo: :ref:`int`. The number of the fret on
              which a capo is set. If no capo is used, the value is 0.

            - Track's color. The track's displayed color in Guitar Pro.*/
            byte flags = GpBase.ReadByte()[0];
            track.IsPercussionTrack = ((flags & 0x01) != 0);
            track.Is12StringedGuitarTrack = ((flags & 0x02) != 0);
            track.IsBanjoTrack = ((flags & 0x04) != 0);
            track.Name = GpBase.ReadByteSizeString(40);
            var stringCount = GpBase.ReadInt()[0];

            for (int i = 0; i < 7; i++)
            {
                int iTuning = GpBase.ReadInt()[0];
                if (stringCount > i)
                {
                    var oString = new GuitarString(i + 1, iTuning);
                    track.Strings.Add(oString);
                }
            }
            track.Port = GpBase.ReadInt()[0];
            track.Channel = ReadChannel(channels);
            if (track.Channel.Channel == 9)
            {
                track.IsPercussionTrack = true;
            }
            track.FretCount = GpBase.ReadInt()[0];
            track.Offset = GpBase.ReadInt()[0];
            track.Color = ReadColor();
        }

        private MidiChannel ReadChannel(MidiChannel[] channels)
        { /*Read MIDI channel.

        MIDI channel in Guitar Pro is represented by two integers. First
        is zero-based number of channel, second is zero-based number of
        channel used for effects.*/
            var index = GpBase.ReadInt()[0] - 1;
            MidiChannel trackChannel = new MidiChannel();
            var effectChannel = GpBase.ReadInt()[0] - 1;
            if (0 <= index && index < channels.Length)
            {
                trackChannel = channels[index];
                if (trackChannel.Instrument < 0)
                {
                    trackChannel.Instrument = 0;
                }
                if (!trackChannel.IsPercussionChannel)
                {
                    trackChannel.EffectChannel = effectChannel;
                }
            }
            return trackChannel;
        }

        private void ReadMeasureHeaders(int measureCount)
        {
            /*Read measure headers.

            The *measures* are written one after another, their number have
            been specified previously.

            :param measureCount: number of measures to expect.*/
            MeasureHeader previous = null;
            for (int number = 1; number < measureCount + 1; number++)
            {
                var header = ReadMeasureHeader(number, previous);
                this.AddMeasureHeader(header);
                previous = header;
            }

        }

        private MeasureHeader ReadMeasureHeader(int number, MeasureHeader previous = null)
        {
            /*Read measure header.

            The first byte is the measure's flags. It lists the data given in the
            current measure.

            - *0x01*: numerator of the key signature
            - *0x02*: denominator of the key signature
            - *0x04*: beginning of repeat
            - *0x08*: end of repeat
            - *0x10*: number of alternate ending
            - *0x20*: presence of a marker
            - *0x40*: tonality of the measure
            - *0x80*: presence of a double bar

            Each of these elements is present only if the corresponding bit
            is a 1.

            The different elements are written (if they are present) from
            lowest to highest bit.

            Exceptions are made for the double bar and the beginning of
            repeat whose sole presence is enough, complementary data is not
            necessary.

            - Numerator of the key signature: :ref:`byte`.

            - Denominator of the key signature: :ref:`byte`.

            - End of repeat: :ref:`byte`.
              Number of repeats until the previous beginning of repeat.

            - Number of alternate ending: :ref:`byte`.

            - Marker: see :meth:`GP3File.readMarker`.

            - Tonality of the measure: 2 :ref:`Bytes <byte>`. These values
              encode a key signature change on the current piece. First byte
              is key signature root, second is key signature type.
              */

            byte flags = GpBase.ReadByte()[0];
            MeasureHeader header = new MeasureHeader();
            header.Number = number;
            header.Start = 0;
            header.Tempo.Value = Tempo;
            header.TripletFeel = TripletFeel;
            if ((flags & 0x01) != 0)
            {
                header.TimeSignature.Numerator = GpBase.ReadSignedByte()[0];
            }
            else
            {
                header.TimeSignature.Numerator = previous.TimeSignature.Numerator;
            }
            if ((flags & 0x02) != 0)
            {
                header.TimeSignature.Denominator.Value = GpBase.ReadSignedByte()[0];
            }
            else
            {
                header.TimeSignature.Denominator.Value = previous.TimeSignature.Denominator.Value;
            }
            header.IsRepeatOpen = (bool)((flags & 0x04) != 0);
            if ((flags & 0x08) != 0)
            {
                header.RepeatClose = GpBase.ReadSignedByte()[0];
            }
            if ((flags & 0x10) != 0)
            {
                header.RepeatAlternatives.Add(ReadRepeatAlternative(MeasureHeaders));
            }
            if ((flags & 0x20) != 0)
            {
                header.Marker = ReadMarker(header);
            }
            if ((flags & 0x40) != 0)
            {
                sbyte root = GpBase.ReadSignedByte()[0];
                sbyte type = GpBase.ReadSignedByte()[0];
                int dir = (root < 0) ? -1 : 1;
                header.KeySignature = (KeySignatures)((int)root * 10 + dir * type);
            }
            else if (header.Number > 1)
            {
                header.KeySignature = previous.KeySignature;
            }
            header.HasDoubleBar = ((flags & 0x80) != 0);

            return header;
        }

        private int ReadRepeatAlternative(List<MeasureHeader> measureHeaders)
        {
            byte value = GpBase.ReadByte()[0];
            int existingAlternatives = 0;
            for (int x = measureHeaders.Count - 1; x >= 0; x--)
            {
                if (measureHeaders[x].IsRepeatOpen) break;
                if (measureHeaders[x].RepeatAlternatives.Count > 0)
                    existingAlternatives |= measureHeaders[x].RepeatAlternatives[0];
            }

            return (1 << value) - 1 ^ existingAlternatives;
        }

        private Marker ReadMarker(MeasureHeader header)
        {
            Marker marker = new Marker();
            marker.Title = GpBase.ReadIntByteSizeString();
            marker.Color = ReadColor();
            marker.MeasureHeader = header;

            return marker;

        }

        private Color ReadColor()
        {
            byte r = GpBase.ReadByte()[0];
            byte g = GpBase.ReadByte()[0];
            byte b = GpBase.ReadByte()[0];
            GpBase.Skip(1);
            return new Color(r, g, b);
        }

        private void ReadMidiChannels()
        {
            /*Read MIDI channels.

            Guitar Pro format provides 64 channels(4 MIDI ports by 16
            channels), the channels are stored in this order:

            -port1 / channel1
            - port1 / channel2
            - ...
            - port1 / channel16
            - port2 / channel1
            - ...
            - port4 / channel16

            Each channel has the following form:
            -Instrument: :ref:`int`.
            -Volume: :ref:`byte`.
            -Balance: :ref:`byte`.
            -Chorus: :ref:`byte`.
            -Reverb: :ref:`byte`.
            -Phaser: :ref:`byte`.
            -Tremolo: :ref:`byte`.
            -blank1: :ref:`byte`.
            -blank2: :ref:`byte`.*/
            MidiChannel[] channels = new MidiChannel[64];
            for (int i = 0; i < 64; i++)
            {
                var newChannel = new MidiChannel();
                newChannel.Channel = i;
                newChannel.EffectChannel = i;
                var instrument = GpBase.ReadInt()[0];
                if (newChannel.IsPercussionChannel && instrument == -1)
                {
                    instrument = 0;
                }
                newChannel.Instrument = instrument;
                newChannel.Volume = ToChannelShort(GpBase.ReadByte()[0]);
                newChannel.Balance = ToChannelShort(GpBase.ReadByte()[0]);
                newChannel.Chorus = ToChannelShort(GpBase.ReadByte()[0]);
                newChannel.Reverb = ToChannelShort(GpBase.ReadByte()[0]);
                newChannel.Phaser = ToChannelShort(GpBase.ReadByte()[0]);
                newChannel.Tremolo = ToChannelShort(GpBase.ReadByte()[0]);
                channels[i] = newChannel;
                GpBase.Skip(2);
            }
            Channels = channels;
        }

        private int ToChannelShort(byte d)
        {
            sbyte data = (sbyte)d;
            //transform signed byte to short
            var value = Math.Max(-32768, Math.Min(32767, (data << 3) - 1));
            return Math.Max(value, -1) + 1;
        }

        private void ReadLyrics()
        {
            /*Read lyrics.
            First, read an :ref:`int` that points to the track lyrics are
            bound to. Then it is followed by 5 lyric lines. Each one
            constists of number of starting measure encoded in :ref:`int`
            and :ref:`int-size-string` holding text of the lyric line.*/

            Lyrics = new List<BE.Lyrics.Lyrics>();
            var lyrics = new BE.Lyrics.Lyrics();
            lyrics.TrackChoice = GpBase.ReadInt()[0];
            for (int x = 0; x < lyrics.Lines.Length; x++)
            {
                lyrics.Lines[x].StartingMeasure = GpBase.ReadInt()[0];
                lyrics.Lines[x].Lyrics = GpBase.ReadIntSizeString();
            }
            Lyrics.Add(lyrics);
        }


        private void ReadInfo()
        {
            /*Read score information.

            Score information consists of sequence of
            :ref:`IntByteSizeStrings <int-byte-size-string>`:

            - title
            - subtitle
            - artist
            - album
            - words
            - copyright
            - tabbed by
            - instructions

            The sequence if followed by notice. Notice starts with the
            number of notice lines stored in :ref:`int`. Each line is
            encoded in :ref:`int-byte-size-string`.*/

            Title = GpBase.ReadIntByteSizeString();
            Subtitle = GpBase.ReadIntByteSizeString();
            Interpret = GpBase.ReadIntByteSizeString();
            Album = GpBase.ReadIntByteSizeString();
            Author = GpBase.ReadIntByteSizeString();
            Copyright = GpBase.ReadIntByteSizeString();
            TabAuthor = GpBase.ReadIntByteSizeString();
            Instructional = GpBase.ReadIntByteSizeString();
            int notesCount = GpBase.ReadInt()[0];
            Notice = new string[notesCount];
            for (int x = 0; x < notesCount; x++)
            {
                Notice[x] = GpBase.ReadIntByteSizeString();
            }
        }
    }
}