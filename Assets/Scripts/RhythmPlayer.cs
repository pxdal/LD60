using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class Note
{
    public char note;

    public int durationBeats;

    public int durationRemaining;

    public bool startedPlaying = false;

    public Note(char note, int durationBeats)
    {
        this.note = note;
        this.durationBeats = durationBeats;
        durationRemaining = durationBeats;
    }
}

public class RhythmPlayer : MonoBehaviour
{
    // serialized //

    public string rhythmString;

    [Header("Objects/Prefabs")]

    public Transform lineParent;

    public GameObject dotPrefab;

    public GameObject dashPrefab;

    public AudioClip dotSound;
    public AudioClip dashSound;

    [Header("Settings")]

    public float beatSize = 1f;

    [Tooltip("Given as percentage of a total beat (only affects the last dash)")]
    public float dashSize = 0.8f;


    // non-serialized //

    private GlobalRhythmManager m_manager;

    private AudioSource m_notePlayer;

    private List<Note> m_parsedRhythm;

    private List<GameObject> m_rhythmObjects;

    private int m_lane = -1;

    private bool m_playerTappedInWindow = false;

    // getters + setters //

    public Note GetCurrentNote()
    {
        if(m_currentNote < 0 || m_currentNote >= m_parsedRhythm.Count) return null;

        return m_parsedRhythm[m_currentNote];
    }

    public void SetRhythmManager(GlobalRhythmManager manager)
    {
        m_manager = manager;

        m_manager.onBeat.AddListener(OnBeat);
    }

    public void SetLane(int lane)
    {
        m_lane = lane;
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
        if(beat < 0 || beat >= rhythmString.Length) return ' ';

        return rhythmString[beat];
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


    // event listeners //

    void Awake()
    {
        m_notePlayer = GetComponent<AudioSource>();
    }

    void Start()
    {
        ParseRhythm();
        BuildRhythmObjects(beatSize);
    }


    private int m_currentNote = 0;

    public TMP_Text DEBUG_text;

    void Update()
    {
        DEBUG_text.text = m_currentNote + "";

        bool playerTapping = GameController.Instance.Tapper.IsTapping(m_lane);
        bool playerHolding = GameController.Instance.Tapper.IsHolding(m_lane);

        if (IsInTapWindow(out float offsetToBeat))
        {
            // DEBUG_text.text = "in tap window (playerTappedInWindow? " + m_playerTappedInWindow + ")";

            if(playerTapping){
                if(offsetToBeat > 0 && !m_playerTappedInWindow)
                {
                    PlayCurrentNote();
                }

                m_playerTappedInWindow = true;
            }
        } else
        {
            // DEBUG_text.text = "not tap window";

            m_playerTappedInWindow = false;
        }
    }

    void OnBeat()
    {
        CheckCurrentNote();
    }

    void CheckCurrentNote()
    {
        Note t_note = GetCurrentNote();

        if(t_note == null) return;

        if (!t_note.startedPlaying)
        {
            if(m_playerTappedInWindow){
                if(t_note.note == '.')
                {
                    PlayDotNote();
                } else if(t_note.note == '-')
                {
                    PlayDashNote();
                }
            }
            
            t_note.startedPlaying = true;
        } else if(t_note.durationRemaining > 0)
        {
            t_note.durationRemaining--;
        }

        if(t_note.durationRemaining <= 0){
            // stop
            StopNote();

            m_currentNote++;

            // run again to check for next note
            CheckCurrentNote();
        }
    }

    void PlayCurrentNote()
    {
        Note t_note = m_parsedRhythm[m_currentNote];

        if(t_note.note == '.')
        {
            PlayDotNote();
        } else if(t_note.note == '-')
        {
            PlayDashNote();
        }

        t_note.startedPlaying = true;
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

    public bool IsInTapWindow(out float offsetToBeat)
    {
        float currentBeat = m_manager.GetCurrentBeat();

        float t_leftBound = currentBeat + m_manager.tapToleranceRange.x;
        float t_rightBound = currentBeat + m_manager.tapToleranceRange.y;

        // detect if there's a beat change between
        bool inTapWindow = Mathf.FloorToInt(t_leftBound) != Mathf.FloorToInt(t_rightBound);

        offsetToBeat = currentBeat - Mathf.FloorToInt(t_rightBound);

        return inTapWindow;
    }

    /// <summary>
    /// NOTE: negates value so that scrolling goes left
    /// </summary>
    /// <param name="beat"></param>
    /// <returns></returns>
    public float PositionFromBeat(float beat)
    {
        return -beat * beatSize;
    }

    void ParseRhythm()
    {
        m_parsedRhythm = new();

        int dashDuration = 0;

        for(int i = 0; i < rhythmString.Length; i++)
        {
            char r = rhythmString[i];

            if(r != '-')
            {
                if(dashDuration > 0)
                {
                    m_parsedRhythm.Add(new('-', dashDuration));    
                    dashDuration = 0;
                }

                m_parsedRhythm.Add(new(r, 1));
            } else
            {
                dashDuration++;
            }
        }

        if(dashDuration > 0)
        {
            m_parsedRhythm.Add(new('-', dashDuration));
        }
    }

    // NOTE: it would be more sensible to implement this using the notes and durations from
    // ParseRhythm, but I wrote this first and don't feel like changing it now.
    void BuildRhythmObjects(float beatSpacing, float xOffset = 0)
    {
        if(rhythmString.Length < 1) return;

        m_rhythmObjects = new();

        float currentX = xOffset;

        bool buildingDash = false;
        float dashStart = -1f;

        for(int i = 0; i < rhythmString.Length+1; i++)
        {
            char c = i == rhythmString.Length ? ' ' : rhythmString[i];

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
