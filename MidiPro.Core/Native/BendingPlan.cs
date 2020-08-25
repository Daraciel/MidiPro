using System.Collections.Generic;

namespace MidiPro.Core.Native
{
    public class BendingPlan
    {
        public List<BendPoint> bendingPoints = new List<BendPoint>();
        //List<int> positions = new List<int>(); //index where to put the points
        public int originalChannel = 0;
        public int usedChannel = 0;
        public BendingPlan(int originalChannel, int usedChannel, List<BendPoint> bendingPoints)
        {
            this.bendingPoints = bendingPoints;
            //this.positions = positions;
            this.originalChannel = originalChannel;
            this.usedChannel = usedChannel;

        }
    }
}