using System.Collections.Generic;

namespace MidiPro.Core.GP
{
    public class RepeatGroup
    {
        public List<MeasureHeader> MeasureHeaders;
        public List<MeasureHeader> Openings;
        public List<MeasureHeader> Closings;
        public bool IsClosed;


        public RepeatGroup()
        {
            MeasureHeaders = new List<MeasureHeader>();
            Openings = new List<MeasureHeader>();
            Closings = new List<MeasureHeader>();
            IsClosed = false;
        }


        public void AddMeasureHeader(MeasureHeader h)
        {
            if (!(Openings.Count > 0)) Openings.Add(h);

            MeasureHeaders.Add(h);
            h.RepeatGroup = this;
            if (h.RepeatClose > 0)
            {
                Closings.Add(h);
                IsClosed = true;
            }
            else if (IsClosed)
            {
                IsClosed = false;
                Openings.Add(h);
            }
        }
    }
}