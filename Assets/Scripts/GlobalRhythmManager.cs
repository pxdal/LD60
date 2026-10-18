using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GlobalRhythmManager : MonoBehaviour
{
    // public //

    [Header("Play Settings")]

    public int bpm;

    public AudioClip rhythmClip;

    public List<RhythmPlayer> telegrams = new();

    [Header("Tap Settings")]

    [Tooltip("The left and right bounds of the tolerance, defined in fractions of a beat")]
    public Vector2 tapToleranceRange = new(-0.25f, 0.1f);

    [Header("Other")]

    public UnityEvent onBeat = new();

    // private //

    private AudioSource m_rhythmPlayer;


    private float m_currentBeat = 0;


    // getters + setters //

    public bool IsPlaying()
    {
        return m_rhythmPlayer.isPlaying;
    }

    public float GetCurrentTime()
    {
        if(!IsPlaying()) return -1;

        return m_rhythmPlayer.time;
    }

    public float GetCurrentBeat()
    {
        return m_currentBeat;
    }

    public int GetCurrentBeatFloored()
    {
        return Mathf.FloorToInt(m_currentBeat);
    }

    public int GetCurrentBeatRounded()
    {
        return Mathf.RoundToInt(m_currentBeat);
    }


    // event listeners //

    void Awake()
    {
        m_rhythmPlayer = GetComponent<AudioSource>();

        for(int i = 0; i < telegrams.Count; i++)
        {
            RhythmPlayer telegram = telegrams[i];

            telegram.SetRhythmManager(this);
            telegram.SetLane(i+1);
        }
    }

    void Start()
    {
        m_rhythmPlayer.clip = rhythmClip;
        m_rhythmPlayer.loop = false;

        Play(1f);
    }

    void Update()
    {
        if(!IsPlaying()) return;

        UpdateRhythm();

        UpdateLinePositions();

        // CheckIfTapped();
    }

    /// <summary>
    /// Syncs the rhythm to the current time using BPM
    /// </summary>
    void UpdateRhythm()
    {
        int t_lastBeat = GetCurrentBeatFloored();

        m_currentBeat = RhythmUtils.SecondsToBeats(GetCurrentTime(), bpm);

        if(GetCurrentBeatFloored() != t_lastBeat)
        {
            onBeat.Invoke();
        }
    }

    void UpdateLinePositions()
    {
        foreach(RhythmPlayer telegram in telegrams)
        {
            telegram.SetLinePosition(GetCurrentTime());
        }
    }

    // state managers //

    public void Play(float delay)
    {
        float startTime = (float)AudioSettings.dspTime + delay;

        m_rhythmPlayer.PlayScheduled(startTime);

        foreach(RhythmPlayer telegram in telegrams)
        {
            telegram.PlayScheduled(startTime);
        }
    }
}
