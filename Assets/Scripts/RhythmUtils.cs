using UnityEngine;

public class RhythmUtils
{
    public static float SecondsToBeats(float seconds, int bpm)
    {
        float minutes = seconds / 60f;

        return bpm * minutes;
    }

    public static float BeatsToSeconds(float beats, int bpm)
    {
        float minutes = beats / bpm;

        return minutes * 60f;
    }
}
