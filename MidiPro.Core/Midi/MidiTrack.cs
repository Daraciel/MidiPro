using System.Collections.Generic;

namespace MidiPro.Core.Midi
{
    public class MidiTrack
    {
        public List<MidiMessage> messages = new List<MidiMessage>();
        public List<byte> createBytes()
        {
            List<byte> data = new List<byte>();
            byte runningStatusByte = 0x00;
            bool statusByteSet = false;
            foreach (MidiMessage message in messages)
            {
                if (message.time < 0)
                {
                    message.time = 0;
                }
                data.AddRange(MidiExport.encodeVariableInt(message.time));
                if (message.type.Equals("sysex"))
                {
                    statusByteSet = false;
                    data.Add(0xf0);
                    data.AddRange(MidiExport.encodeVariableInt(message.data.Length + 1));
                    data.AddRange(message.data);
                    data.Add(0xf7);
                }
                else
                {
                    List<byte> raw = new List<byte>();
                    raw = message.createBytes();

                    byte temp = raw[0];
                    if (statusByteSet && !message.is_meta && raw[0] < 0xf0 && raw[0] == runningStatusByte)
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

            return MidiExport.writeChunk("MTrk", data);
        }
    }
}