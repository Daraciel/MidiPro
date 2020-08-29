using System.Collections.Generic;

namespace MidiPro.Core.Native
{
    public class BendingPlan
    {
        public List<BendPoint> BendingPoints { get; set; }
        public int OriginalChannel { get; set; }
        public int UsedChannel { get; set; }

        public BendingPlan(int originalChannel, int usedChannel, List<BendPoint> bendingPoints)
        {
            BendingPoints = bendingPoints;
            OriginalChannel = originalChannel;
            UsedChannel = usedChannel;

        }
    }
}