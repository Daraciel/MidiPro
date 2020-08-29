using System;
using System.Collections.Generic;

namespace MidiPro.Core.Midi
{
    public class MidiExport
    {
        public static System.Text.Encoding Ascii = System.Text.Encoding.ASCII;

        public int FileType { get; set; }
        public int TicksPerBeat { get; set; }

        public List<MidiTrack> MidiTracks { get; set; }


        public MidiExport(int fileType = 1, int ticksPerBeat = 960)
        {
            MidiTracks = new List<MidiTrack>();


            this.FileType = fileType;
            this.TicksPerBeat = ticksPerBeat;
        }

        public List<byte> CreateBytes()
        {
            List<byte> data = new List<byte>();
            data.AddRange(CreateHeader());
            foreach (MidiTrack track in MidiTracks)
            {
                data.AddRange(track.CreateBytes());
            }

            return data;
        }

        public List<byte> CreateHeader()
        {
            List<byte> data = new List<byte>();

            List<byte> header = new List<byte>();
            header.AddRange(ToBeShort(FileType));
            header.AddRange(ToBeShort(MidiTracks.Count));
            header.AddRange(ToBeShort(TicksPerBeat));

            data.AddRange(WriteChunk("MThd", header));

            return data;
        }

        public static List<byte> WriteChunk(string name, List<byte> data)
        {
            List<byte> result = new List<byte>();

            result.AddRange(Ascii.GetBytes(name));
            result.AddRange(ToBeuLong(data.Count));
            result.AddRange(data);
            return result;
        }

        public static List<byte> ToBeuLong(int val)
        {
            List<byte> data = new List<byte>();
            byte[] lEdata = BitConverter.GetBytes((System.UInt32)val);

            for (int x = lEdata.Length - 1; x >= 0; x--)
            {
                data.Add(lEdata[x]);
            }

            return data;
        }

        public static List<byte> ToBeShort(int val)
        {
            List<byte> data = new List<byte>();
            byte[] lEdata = BitConverter.GetBytes((System.Int16)val);

            for (int x = lEdata.Length - 1; x >= 0; x--)
            {
                data.Add(lEdata[x]);
            }

            return data;
        }

        public static List<byte> EncodeVariableInt(int val)
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