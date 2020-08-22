using System;
using MidiPro.Core.BE;
using MidiPro.Core.GP.Enums;

namespace MidiPro.Core.GP.Beat
{
    public class BeatStroke
    {
        public BeatStrokeDirections Direction { get; set; }

        public int Value { get; set; } //4 = quarter etc.

        public float StartTime { get; set; } //0 = falls on time, 1 = starts on time

        public BeatStroke()
        {
            Direction = BeatStrokeDirections.None;
            Value = 0;
            StartTime = 0.0f;
        }

        public BeatStroke(BeatStrokeDirections d, int v, float s)
        {
            Direction = d;
            Value = v;
            StartTime = s;
        }

        public void SetByGp6Standard(int gp6Duration)
        {
            //GP6 will use value as 30 to 480 (64th to quarter note)
            int[] possibleVals = { 1, 2, 4, 8, 16, 32, 64 };
            int translated = 64 / (gp6Duration / 30);
            int lastVal = 0;
            foreach (int val in possibleVals)
            {
                if (val == translated) { Value = val; break; }
                if (val > translated && lastVal < translated)
                {
                    Value = (translated - lastVal > val - translated) ? val : lastVal;
                    break;
                }
                lastVal = val;
            }
        }

        public int GetIncrementTime(Beat beat)
        {
            int duration = 0;
            if (Value > 0)
            {
                foreach (Voice voice in beat.Voices)
                {
                    if (voice.IsEmpty) continue;

                    int currentDuration = voice.Duration.Time();
                    if (duration == 0 || currentDuration < duration)
                    {
                        duration = ((currentDuration <= (int)Durations.QuarterTime) ? currentDuration : (int)Durations.QuarterTime);
                    }
                    if (duration > 0)
                    {
                        return (int)Math.Round((duration / 8.0f) * (4.0f / Value));
                    }
                }
            }
            return 0;
        }

        public BeatStroke SwapDirection()
        {
            if (Direction == BeatStrokeDirections.Up)
            {
                Direction = BeatStrokeDirections.Down;
            }
            else if (Direction == BeatStrokeDirections.Down)
            {
                Direction = BeatStrokeDirections.Up;
            }
            return new BeatStroke(Direction, Value, 0.0f);
        }
    }
}