using System.Collections.Generic;
using UnityEngine;

public class GlobalRhythmManager : MonoBehaviour
{
    // public //

    [Header("Settings")]

    public int bpm;

    public AudioClip rhythmSound;

    public List<RhythmPlayer> telegrams = new();

    [Tooltip("How many seconds until the rhythm starts")]
    [SerializeField] private float m_startOffset = 0f;


    // private //

    private AudioSource m_rhythmPlayer;


    private float m_currentBeat = 0;

    private float m_timeOffset = 0f;

    private bool m_isPlaying = false;

    // getters + setters //

    public bool IsPlaying()
    {
        return m_isPlaying;
    }

    public float GetCurrentTime()
    {
        if(!m_isPlaying) return -1f;

        return Time.time - m_timeOffset;
    }

    public float GetCurrentBeat()
    {
        return m_currentBeat;
    }

    public int GetCurrentBeatInt()
    {
        return Mathf.FloorToInt(m_currentBeat);
    }

    // event listeners //

    void Awake()
    {
        m_rhythmPlayer = GetComponent<AudioSource>();

        foreach(RhythmPlayer telegram in telegrams)
        {
            telegram.SetRhythmManager(this);
        }
    }

    void Start()
    {
        m_rhythmPlayer.clip = rhythmSound;
        m_rhythmPlayer.loop = false;

        Play();
    }

    void Update()
    {
        if(!IsPlaying()) return;

        UpdateRhythm();

        foreach(RhythmPlayer telegram in telegrams)
        {
            telegram.SetLinePosition(GetCurrentTime());
        }
    }

    /// <summary>
    /// Syncs the rhythm to the current time using BPM
    /// </summary>
    void UpdateRhythm()
    {
        int t_lastBeat = GetCurrentBeatInt();

        m_currentBeat = RhythmUtils.SecondsToBeats(GetCurrentTime(), bpm);

        if(GetCurrentBeatInt() != t_lastBeat)
        {
            m_rhythmPlayer.Play();
        }
    }


    // state managers //

    public void Play()
    {
        m_timeOffset = Time.time + m_startOffset;
        m_isPlaying = true;
    }
}
