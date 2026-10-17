using UnityEngine;

public class RhythmUtils
{
    public static float SecondsToBeats(float seconds, int bpm)
    {
        float minutes = seconds / 60f;

        return bpm * minutes;
    }
}
