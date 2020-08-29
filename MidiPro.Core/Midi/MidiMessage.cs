using System;
using System.Collections.Generic;

namespace MidiPro.Core.Midi
{
    public class MidiMessage
    {
        public string Type { get; set; }
        public int Time { get; set; }
        public bool IsMeta { get; set; }
        public int Number { get; set; }
        public string Text { get; set; }
        public string Name { get; set; }
        public int Channel { get; set; }
        public int Port { get; set; }
        public int Tempo { get; set; }
        public int Numerator { get; set; }
        public int Denominator { get; set; }
        public int ClocksPerClick { get; set; }
        public int Notated32NdNotesPerBeat { get; set; }
        public int Key { get; set; }

        public int Note { get; set; }
        public int Velocity { get; set; }
        public int Value { get; set; }
        public int Control { get; set; }
        public int Program { get; set; }
        public int Pitch { get; set; }
        public byte[] Data { get; set; }


        private byte _code;
        private bool _isMajor;

        private MidiMessage()
        {
            Type = string.Empty;
            Time = 0;
            IsMeta = false;
            Number = 0;
            Text = string.Empty;
            Name = string.Empty;
            Channel = 0;
            Port = 0;
            Tempo = 500000;
            Numerator = 4;
            Denominator = 2;
            ClocksPerClick = 24;
            Notated32NdNotesPerBeat = 8;
            Key = 0;
            Note = 0;
            Velocity = 0;
            Value = 0;
            Control = 0;
            Program = 0;
            Pitch = 0;
            Data = null;

            _code = 0x00;
            _isMajor = true;
        }

        //Others not needed..
        public MidiMessage(string type, string[] args, int time, byte[] data = null) :this()
        {
            IsMeta = false;
            this.Type = type;
            this.Time = time;

            //Meta Messages
            if (type.Equals("sequence_number")) { IsMeta = true; _code = 0x00; Number = int.Parse(args[0]); }
            if (type.Equals("text") || type.Equals("copyright") || type.Equals("lyrics") || type.Equals("marker") || type.Equals("cue_marker"))
            {
                IsMeta = true;
                Text = args[0];
            }
            if (type.Equals("text")) _code = 0x01;
            if (type.Equals("copyright")) _code = 0x02;
            if (type.Equals("lyrics")) _code = 0x05;
            if (type.Equals("marker")) _code = 0x06;
            if (type.Equals("cue_marker")) _code = 0x07;

            if (type.Equals("track_name") || type.Equals("instrument_name") || type.Equals("device_name"))
            {
                IsMeta = true; _code = 0x03;
                Name = args[0];
            }
            if (type.Equals("instrument_name")) _code = 0x04;
            if (type.Equals("device_name")) _code = 0x08;

            if (type.Equals("channel_prefix")) { _code = 0x20; Channel = int.Parse(args[0]); IsMeta = true; }
            if (type.Equals("midi_port")) { _code = 0x21; Port = int.Parse(args[0]); IsMeta = true; }
            if (type.Equals("end_of_track")) { _code = 0x2f; IsMeta = true; }
            if (type.Equals("set_tempo")) { _code = 0x51; Tempo = int.Parse(args[0]); IsMeta = true; }

            if (type.Equals("time_signature"))
            {
                IsMeta = true; _code = 0x58;
                Numerator = int.Parse(args[0]);  //4
                Denominator = int.Parse(args[1]); //4 
                ClocksPerClick = int.Parse(args[2]); //24
                Notated32NdNotesPerBeat = int.Parse(args[3]); //8
            }

            if (type.Equals("key_signature"))
            {
                IsMeta = true; _code = 0x59;
                Key = int.Parse(args[0]);
                _isMajor = args[1].Equals("0"); //"0" or "1"
            }


            //Normal Messages
            if (type.Equals("note_off")) { _code = 0x80; Channel = int.Parse(args[0]); Note = int.Parse(args[1]); Velocity = int.Parse(args[2]); }
            if (type.Equals("note_on")) { _code = 0x90; Channel = int.Parse(args[0]); Note = int.Parse(args[1]); Velocity = int.Parse(args[2]); }
            if (type.Equals("polytouch")) { _code = 0xa0; Channel = int.Parse(args[0]); Note = int.Parse(args[1]); Value = int.Parse(args[2]); }
            if (type.Equals("control_change")) { _code = 0xb0; Channel = int.Parse(args[0]); Control = int.Parse(args[1]); Value = int.Parse(args[2]); }
            if (type.Equals("program_change")) { _code = 0xc0; Channel = int.Parse(args[0]); Program = int.Parse(args[1]); }
            if (type.Equals("aftertouch")) { _code = 0xd0; Channel = int.Parse(args[0]); Value = int.Parse(args[1]); }
            if (type.Equals("pitchwheel")) { _code = 0xe0; Channel = int.Parse(args[0]); Pitch = int.Parse(args[1]); }
            if (type.Equals("sysex")) { _code = 0xf0; this.Data = data; }

        }

        public List<byte> CreateBytes()
        {
            List<byte> data;
            if (IsMeta) data = CreateMetaBytes();
            else data = CreateMessageBytes();

            return data;
        }

        public List<byte> CreateMetaBytes()
        {
            List<byte> data = new List<byte>();

            if (Type.Equals("sequence_number"))
            {
                data.Add((byte)(Number >> 8));
                data.Add((byte)(Number & 0xff));
            }
            if (Type.Equals("text") || Type.Equals("copyright") || Type.Equals("lyrics") || Type.Equals("marker") || Type.Equals("cue_marker"))
            {
                if (Text == null) Text = "";
                data.AddRange(MidiExport.Ascii.GetBytes(Text));
            }

            if (Type.Equals("track_name") || Type.Equals("instrument_name") || Type.Equals("device_name"))
            {
                data.AddRange(MidiExport.Ascii.GetBytes(Name));
            }
            if (Type.Equals("channel_prefix"))
            {
                data.Add((byte)Channel);
            }
            if (Type.Equals("midi_port"))
            {
                data.Add((byte)Port);
            }
            if (Type.Equals("set_tempo"))
            {
                //return [tempo >> 16, tempo >> 8 & 0xff, tempo & 0xff]
                data.Add((byte)(Tempo >> 16));
                data.Add((byte)((Tempo >> 8) & 0xff));
                data.Add((byte)(Tempo & 0xff));
            }

            if (Type.Equals("time_signature"))
            {
                data.Add((byte)Numerator);
                data.Add((byte)Math.Log(Denominator, 2));
                data.Add((byte)ClocksPerClick);
                data.Add((byte)Notated32NdNotesPerBeat);
            }

            if (Type.Equals("key_signature"))
            {
                data.Add((byte)(Key & 0xff));
                data.Add(_isMajor ? (byte)0x00 : (byte)0x01);
            }

            int dataLength = data.Count;
            data.InsertRange(0, MidiExport.EncodeVariableInt(dataLength));
            data.Insert(0, _code);
            data.Insert(0, 0xff);

            return data;
        }


        public List<byte> CreateMessageBytes()
        {

            List<byte> data = new List<byte>();
            /* if (type.Equals("note_off")) { code = 0x80; channel = int.Parse(args[0]); note = int.Parse(args[1]); velocity = int.Parse(args[2]); }
            if (type.Equals("note_on")) { code = 0x90; channel = int.Parse(args[0]); note = int.Parse(args[1]); velocity = int.Parse(args[2]); }
            if (type.Equals("polytouch")) { code = 0xa0; channel = int.Parse(args[0]); note = int.Parse(args[1]); value = int.Parse(args[2]); }
            if (type.Equals("control_change")) { code = 0xb0; channel = int.Parse(args[0]); control = int.Parse(args[1]); value = int.Parse(args[2]); }
            if (type.Equals("program_change")) { code = 0xc0; channel = int.Parse(args[0]); program = int.Parse(args[1]); }
            if (type.Equals("aftertouch")) { code = 0xd0; channel = int.Parse(args[0]); value = int.Parse(args[1]); }
            if (type.Equals("pitchwheel")) { code = 0xe0; channel = int.Parse(args[0]); pitch = int.Parse(args[1]); }
            if (type.Equals("sysex")) { code = 0xf0; this.data = data; }
              
             */
            if (Type.Equals("note_off") || Type.Equals("note_on"))
            {
                data.Add((byte)(_code | (byte)Channel));
                data.Add((byte)Note); data.Add((byte)Velocity);
            }

            if (Type.Equals("polytouch"))
            {
                data.Add((byte)(_code | (byte)Channel));
                data.Add((byte)Note); data.Add((byte)Value);
            }
            if (Type.Equals("control_change"))
            {
                data.Add((byte)(_code | (byte)Channel));
                data.Add((byte)Control); data.Add((byte)Value);
            }
            if (Type.Equals("program_change"))
            {
                data.Add((byte)(_code | (byte)Channel));
                data.Add((byte)Program);
            }
            if (Type.Equals("aftertouch"))
            {
                data.Add((byte)(_code | (byte)Channel));
                data.Add((byte)Value);
            }
            if (Type.Equals("pitchwheel"))  //14 bit signed integer
            {
                data.Add((byte)(_code | (byte)Channel));
                //data.Add((byte)pitch);
                Pitch -= -8192;
                data.Add((byte)(Pitch & 0x7f));
                data.Add((byte)(Pitch >> 7));
            }
            if (Type.Equals("sysex"))
            {
                data.AddRange(this.Data);
            }


            return data;
        }
    }
}