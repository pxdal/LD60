using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GlobalRhythmManager : MonoBehaviour
{
    // public //

    [Header("External Objects")]
    public MissedMeter missedMeter;

    public TMP_Text notesMissedText;

    public GameObject bg;

    [Header("Play Settings")]

    public int bpm;

    public AudioClip rhythmClip;

    public List<RhythmPlayer> telegrams = new();


    public bool autostart = true;

    [Header("Tap Settings")]

    [Tooltip("The left and right bounds of the tolerance, defined in fractions of a beat")]
    public Vector2 tapToleranceRange = new(-0.25f, 0.1f);

    [Header("Other")]

    public float normalBgOpacity = 1f;

    public float playingBgOpacity = 0.75f;

    public float bgFadeDuration = 1f;

    public UnityEvent onBeat = new();

    public UnityEvent onEnd = new();

    // private //

    private AudioSource m_rhythmAudioSource;


    private float m_currentBeat = 0;

    private float m_smoothedTimeOffset = -1f;

    // getters + setters //

    public bool IsPlaying()
    {
        return m_rhythmAudioSource.time > 0;
    }

    public float GetCurrentTime()
    {
        if(!IsPlaying()) return -1;

        // return m_rhythmAudioSource.time;

        // smoother motion
        return Time.time - m_smoothedTimeOffset;
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

    public int GetNumMisses()
    {
        int sum = 0;

        foreach(RhythmPlayer telegram in telegrams)
        {
            sum += telegram.GetNotesMissedCount();
        }

        return sum;
    }

    // event listeners //

    void Awake()
    {
        m_rhythmAudioSource = GetComponent<AudioSource>();

        for(int i = 0; i < telegrams.Count; i++)
        {
            RhythmPlayer telegram = telegrams[i];

            telegram.SetRhythmManager(this);
            telegram.SetLane(i+1);
        }
    }

    void Start()
    {
        m_rhythmAudioSource.clip = rhythmClip;
        m_rhythmAudioSource.loop = false;

        onEnd.AddListener(OnEnd);

        if(autostart) Play(1f);
    }

    private bool m_wasStopped = false;

    void Update()
    {
        if(!IsPlaying()) return;

        if(m_smoothedTimeOffset < 0f)
        {
            // first set of smooth time
            float t_audioPosition = m_rhythmAudioSource.time;
            
            m_smoothedTimeOffset = Time.time - t_audioPosition;
        }

        UpdateRhythm();

        UpdateLinePositions();

        missedMeter.SetMisses(GetNumMisses());
        // CheckIfTapped();

        if(m_rhythmAudioSource.time > m_rhythmAudioSource.clip.length-0.1f && !m_wasStopped)
        {
            FadeInBg(0f);

            onEnd.Invoke();
        }
    }

    void OnEnd()
    {
        notesMissedText.text = "Missed: " + GetNumMisses();
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

    public void FadeOutBg(float after)
    {
        StartCoroutine(bg.FadeObjectAfter(playingBgOpacity, bgFadeDuration, after));
    }

    public void FadeInBg(float after)
    {
        StartCoroutine(bg.FadeObjectAfter(normalBgOpacity, bgFadeDuration, after));
    }

    public void Play(float delay)
    {
        float startTime = (float)AudioSettings.dspTime + delay;

        m_rhythmAudioSource.PlayScheduled(startTime);

        FadeOutBg(0f);

        foreach(RhythmPlayer telegram in telegrams)
        {
            telegram.PlayScheduled(startTime);
        }
    }

    public void Stop()
    {
        m_wasStopped = true;

        m_rhythmAudioSource.Stop();

        foreach(RhythmPlayer telegram in telegrams)
        {
            telegram.StopNote();
        }    
    }

    public void Reset()
    {
        // NOTE: resets on next loop
        m_smoothedTimeOffset = -1f;

        m_wasStopped = false;

        foreach(RhythmPlayer telegram in telegrams)
        {
            telegram.Reset();
            telegram.SetLinePosition(0f);  
        }

        missedMeter.SetMisses(0);

        m_rhythmAudioSource.Stop();

        Play(1f);
    }
}
