using System;
using System.Collections.Generic;
using UnityEngine;

public class RhythmPlayer : MonoBehaviour
{
    // serialized //

    public string rhythm;

    [Header("Objects/Prefabs")]

    public Transform lineParent;

    public GameObject dotPrefab;

    public GameObject dashPrefab;

    public AudioClip dotSound;
    public AudioClip dashSound;

    [Header("Settings")]

    public float beatSize = 1f;

    [Tooltip("Given as percentage of a total beat (only effects the last dash)")]
    public float dashSize = 0.8f;


    // non-serialized //

    private GlobalRhythmManager m_manager;

    private AudioSource m_notePlayer;

    private List<GameObject> m_rhythmObjects;

    private bool m_hitNextNote = false;

    // getters + setters //

    public void SetRhythmManager(GlobalRhythmManager manager)
    {
        m_manager = manager;
    }

    public int GetBPM()
    {
        if(m_manager == null)
        {
            throw new Exception("manager not assigned!");
        }

        return m_manager.bpm;
    }

    public char GetRhythmAtBeat(int beat)
    {
        if(beat < 0 || beat >= rhythm.Length) return ' ';

        return rhythm[beat];
    }

    /// <summary>
    /// leftBound and rightBound should be provided as fractions of a beat
    /// </summary>
    /// <param name="leftBound"></param>
    /// <param name="rightBound"></param>
    /// <returns></returns>
    public char GetCurrentRhythm(float leftBound, float rightBound)
    {
        float currentBeat = m_manager.GetCurrentBeat();

        float t_leftBound = currentBeat + leftBound;
        float t_rightBound = currentBeat + rightBound;

        // detect if there's a beat change between
        if(Mathf.FloorToInt(t_leftBound) != Mathf.FloorToInt(t_rightBound))
        {
            int currentBeatInt = Mathf.FloorToInt(t_rightBound);

            return GetRhythmAtBeat(currentBeatInt);
        }

        return ' ';
    }

    public void MarkGotNextNote()
    {
        // check if we should play sound now
        char currentRhythm = GetRhythmAtBeat(m_manager.GetCurrentBeatFloored());

        m_hitNextNote = !PlayNote(currentRhythm);
    }

    // event listeners //

    void Awake()
    {
        m_notePlayer = GetComponent<AudioSource>();
    }

    void Start()
    {
        BuildRhythmObjects(beatSize);
    }

    private int m_lastBeat = -1;

    void Update()
    {
        char lastRhythm = GetRhythmAtBeat(m_lastBeat);

        int currentBeat = m_manager.GetCurrentBeatFloored();

        char currentRhythm = GetRhythmAtBeat(currentBeat);

        if(currentBeat != m_lastBeat)
        {
            // consecutive dots still count for rhythm edge
            if( (lastRhythm != currentRhythm) || (lastRhythm == '.' && currentRhythm == '.'))
            {
                OnRhythmEdge();
            }
        }

        m_lastBeat = currentBeat;
    }

    /// <summary>
    /// Called when rhythm is changing from one thing to another
    /// (consecutive dashes do not trigger this)
    /// </summary>
    void OnRhythmEdge()
    {
        if(!m_hitNextNote) return;

        char currentRhythm = GetRhythmAtBeat(m_manager.GetCurrentBeatFloored());

        PlayNote(currentRhythm);

        m_hitNextNote = false;
    }


    // state managers //

    public void SetLinePosition(float seconds)
    {
        float beats = RhythmUtils.SecondsToBeats(seconds, GetBPM());

        float pos = PositionFromBeat(beats);

        lineParent.localPosition = new(pos, 0, 0);
    }

    public bool PlayNote(char rhythm)
    {
        switch (rhythm)
        {
            case '.':
                PlayDotNote();
                return true;
            case '-':
                PlayDashNote();
                return true;
            case ' ':
            default:
                StopNote();
                return false;
        }
    }

    public void PlayDotNote()
    {
        m_notePlayer.clip = dotSound;

        m_notePlayer.Play();
    }

    public void PlayDashNote()
    {
        m_notePlayer.clip = dashSound;

        m_notePlayer.Play();
    }

    public void StopNote()
    {
        m_notePlayer.Stop();
    }


    // utils //

    /// <summary>
    /// NOTE: negates value so that scrolling goes left
    /// </summary>
    /// <param name="beat"></param>
    /// <returns></returns>
    public float PositionFromBeat(float beat)
    {
        return -beat * beatSize;
    }

    void BuildRhythmObjects(float beatSpacing, float xOffset = 0)
    {
        if(rhythm.Length < 1) return;

        m_rhythmObjects = new();

        float currentX = xOffset;

        bool buildingDash = false;
        float dashStart = -1f;

        for(int i = 0; i < rhythm.Length+1; i++)
        {
            char c = i == rhythm.Length ? ' ' : rhythm[i];

            Vector3 currentPosition = new(currentX, 0);
            
            if(c == '-')
            {
                if(!buildingDash)
                {
                    dashStart = currentX;
                    buildingDash = true;
                }
            } else
            {
                if (buildingDash)
                {
                    Vector3 t_dashPos = new(dashStart, 0);

                    GameObject t_dash = Instantiate(dashPrefab, lineParent);

                    t_dash.transform.localPosition = t_dashPos;

                    float t_dashSize = currentX - dashStart - beatSpacing*(1-dashSize);
                    t_dash.transform.localScale = new(t_dashSize, 1, 1);
                    
                    m_rhythmObjects.Add(t_dash);

                    buildingDash = false;
                }

                if(c == '.')
                {
                    GameObject t_dot = Instantiate(dotPrefab, lineParent);

                    t_dot.transform.localPosition = currentPosition;

                    m_rhythmObjects.Add(t_dot);
                }
            }

            currentX += beatSpacing;
        }
    }
}
