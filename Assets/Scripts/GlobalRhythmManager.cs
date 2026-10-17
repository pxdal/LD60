using System.Collections.Generic;
using UnityEngine;

public class GlobalRhythmManager : MonoBehaviour
{
    // public //

    [Header("Play Settings")]

    public int bpm;

    public AudioClip rhythmSound;

    public List<RhythmPlayer> telegrams = new();

    [Tooltip("How many seconds until the rhythm starts")]
    [SerializeField] private float m_startOffset = 0f;

    [Header("Tap Settings")]

    [Tooltip("The left and right bounds of the tolerance, defined in fractions of a beat")]
    public Vector2 tapToleranceRange = new(-0.25f, 0.1f);

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

        UpdateLinePositions();

        CheckIfTapped();
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
            m_rhythmPlayer.Play();
        }
    }

    void UpdateLinePositions()
    {
        foreach(RhythmPlayer telegram in telegrams)
        {
            telegram.SetLinePosition(GetCurrentTime());
        }
    }

    public GameObject DEBUG_dot;
    public GameObject DEBUG_dash;

    void CheckIfTapped()
    {
        for(int i = 0; i < telegrams.Count; i++)
        {
            RhythmPlayer telegram = telegrams[i];

            switch (telegram.GetCurrentRhythm(tapToleranceRange.x, tapToleranceRange.y))
            {
                case '.':
                    DEBUG_dot.SetActive(true);
                    DEBUG_dash.SetActive(false);
                    break;
                case '-':
                    DEBUG_dot.SetActive(false);
                    DEBUG_dash.SetActive(true);
                    break;
                case ' ':
                default:
                    DEBUG_dot.SetActive(false);
                    DEBUG_dash.SetActive(false);
                    break;
            }

            int t_lane = i + 1;

            bool t_isTapping = GameController.Instance.Tapper.IsTapping(t_lane);

            if (t_isTapping)
            {
                // TODO: do vis FX and stuff
                Debug.DrawLine(Vector3.right * t_lane, Vector3.up, Color.red);

                // check if it's in range of a beat
                char t_currentRhythm = telegram.GetCurrentRhythm(tapToleranceRange.x, tapToleranceRange.y);

                if(t_currentRhythm == '.')
                {
                    telegram.PlayDotNote();
                } else if(t_currentRhythm == '-')
                {
                    telegram.PlayDashNote();
                }
            }
        }
    }

    // state managers //

    public void Play()
    {
        m_timeOffset = Time.time + m_startOffset;
        m_isPlaying = true;
    }
}
