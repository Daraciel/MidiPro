using System.Collections.Generic;

namespace MidiPro.Core.Midi
{
    public class MidiTrack
    {
        public List<MidiMessage> Messages { get; set; }

        public MidiTrack()
        {
            Messages = new List<MidiMessage>();
        }
        public List<byte> CreateBytes()
        {
            List<byte> data = new List<byte>();
            byte runningStatusByte = 0x00;
            bool statusByteSet = false;
            foreach (MidiMessage message in Messages)
            {
                if (message.Time < 0)
                {
                    message.Time = 0;
                }
                data.AddRange(MidiExport.EncodeVariableInt(message.Time));
                if (message.Type.Equals("sysex"))
                {
                    statusByteSet = false;
                    data.Add(0xf0);
                    data.AddRange(MidiExport.EncodeVariableInt(message.Data.Length + 1));
                    data.AddRange(message.Data);
                    data.Add(0xf7);
                }
                else
                {
                    List<byte> raw = new List<byte>();
                    raw = message.CreateBytes();

                    byte temp = raw[0];
                    if (statusByteSet && !message.IsMeta && raw[0] < 0xf0 && raw[0] == runningStatusByte)
                    {
                        raw.RemoveAt(0);
                        data.AddRange(raw);
                    }
                    else
                    {
                        data.AddRange(raw);
                    }
                    runningStatusByte = temp;
                    statusByteSet = true;
                }
            }

            return MidiExport.WriteChunk("MTrk", data);
        }
    }
}