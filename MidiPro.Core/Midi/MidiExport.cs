using System;
using System.Collections.Generic;

namespace MidiPro.Core.Midi
{
    public class MidiExport
    {
        public static System.Text.Encoding ascii = System.Text.Encoding.ASCII;

        public int fileType = 1;
        public int ticksPerBeat = 960;

        public List<MidiTrack> midiTracks = new List<MidiTrack>();
        public MidiExport(int fileType = 1, int ticksPerBeat = 960)
        {
            this.fileType = fileType;
            this.ticksPerBeat = ticksPerBeat;
        }

        public List<byte> createBytes()
        {
            List<byte> data = new List<byte>();
            data.AddRange(createHeader());
            foreach (MidiTrack track in midiTracks)
            {
                data.AddRange(track.createBytes());
            }

            return data;
        }

        public List<byte> createHeader()
        {
            List<byte> data = new List<byte>();

            List<byte> header = new List<byte>();
            header.AddRange(toBEShort(fileType));
            header.AddRange(toBEShort(midiTracks.Count));
            header.AddRange(toBEShort(ticksPerBeat));

            data.AddRange(writeChunk("MThd", header));

            return data;
        }

        public static List<byte> writeChunk(string name, List<byte> data)
        {
            List<byte> _data = new List<byte>();

            _data.AddRange(ascii.GetBytes(name));
            _data.AddRange(toBEULong(data.Count));
            _data.AddRange(data);
            return _data;
        }

        public static List<byte> toBEULong(int val)
        {
            List<byte> data = new List<byte>();
            byte[] LEdata = BitConverter.GetBytes((System.UInt32)val);

            for (int x = LEdata.Length - 1; x >= 0; x--)
            {
                data.Add(LEdata[x]);
            }

            return data;
        }

        public static List<byte> toBEShort(int val)
        {
            List<byte> data = new List<byte>();
            byte[] LEdata = BitConverter.GetBytes((System.Int16)val);

            for (int x = LEdata.Length - 1; x >= 0; x--)
            {
                data.Add(LEdata[x]);
            }

            return data;
        }

        public static List<byte> encodeVariableInt(int val)
        {
            if (val < 0) throw new FormatException("Variable int must be positive.");

            List<byte> data = new List<byte>();
            while (val > 0)
            {
                data.Add((byte)(val & 0x7f));
                val >>= 7;
            }

            if (data.Count > 0)
            {
                data.Reverse();
                for (int x = 0; x < data.Count - 1; x++)
                {
                    data[x] |= 0x80;
                }
            }
            else
            {
                data.Add(0x00);
            }

            return data;
        }
    }
}