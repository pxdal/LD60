using System;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.Events;


public class RhythmPlayer : MonoBehaviour
{
    // serialized //

    public string rhythmString;

    [Header("Objects/Prefabs")]

    public Transform lineParent;

    public BoxCollider2D lineCollider;

    public GameObject dotPrefab;

    public GameObject dashPrefab;

    public AudioSource errorSound;

    public GameObject paperStart;
    public GameObject paperLoop;
    public GameObject paperEnd;

    public float paperSpacing = 10f;

    [Header("Settings")]

    public float beatSize = 1f;

    [Tooltip("Given as percentage of a total beat (only affects the last dash)")]
    public float dashSize = 0.8f;

    public UnityEvent whileHolding;

    public UnityEvent whileReleased;

    // non-serialized //

    private GlobalRhythmManager m_manager;

    private AudioSource m_audioSource;

    private List<NoteBehaviour> m_rhythmObjects;

    private int m_lane = -1;

    private int m_misses = 0;


    // getters + setters //

    public int GetNotesMissedCount()
    {
        return m_misses;
    }

    public void Reset()
    {
        m_misses = 0;
        m_canMissOnBlank = false;

        foreach(NoteBehaviour note in m_rhythmObjects)
        {
            note.Reset();
        }
    }

    public void SetRhythmManager(GlobalRhythmManager manager)
    {
        m_manager = manager;

        // m_manager.onBeat.AddListener(OnBeat);
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


    public void PlayScheduled(float startTime)
    {
        m_audioSource.PlayScheduled(startTime);
    }

    // event listeners //

    void Awake()
    {
        m_audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {

        BuildRhythmObjects(beatSize);
    }

    private NoteBehaviour m_lastNote = null;

    // not set to true until the first proper note plays
    private bool m_canMissOnBlank = false;

    void Update()
    {
        NoteBehaviour currentNote = GetOverlappingNote();

        bool playerTapping = GameController.Instance.Tapper.IsTapping(m_lane);
        bool playerHolding = GameController.Instance.Tapper.IsHolding(m_lane);

        if (playerHolding)
        {
            whileHolding.Invoke();
        } else
        {
            whileReleased.Invoke();
        }

        if (playerTapping)
        {
            if(currentNote == null)
            {
                StopNote();
                errorSound.Play();


                if(m_canMissOnBlank){
                    m_misses++;
                }
            } else
            {
                currentNote.MarkAsHit();
                PlayNote();
            }
        }

        if(currentNote == null)
        {
            if(m_lastNote != null)
            {
                // note just exited hitbox: did we get it?
                if (!m_lastNote.hitByPlayer)
                {
                    StopNote();
                    errorSound.Play();

                    m_lastNote.MarkAsMissed();

                    m_misses++;
                }
            }
        } else
        {
            // there are no blank notes, so as soon as we any note one mark this
            m_canMissOnBlank = true;
        }

        m_lastNote = currentNote;
    }

    NoteBehaviour GetOverlappingNote()
    {
        Collider2D[] colliders = new Collider2D[1];

        int num = lineCollider.Overlap(ContactFilter2D.noFilter, colliders);

        if(num > 0)
        {
            return colliders[0].GetComponent<NoteBehaviour>();
        }

        return null;
    }


    // state managers //

    public void SetLinePosition(float seconds)
    {
        float beats = RhythmUtils.SecondsToBeats(seconds, GetBPM());

        float pos = PositionFromBeat(beats);

        lineParent.localPosition = new(pos, 0, 0);
    }

    public void PlayNote()
    {
        m_audioSource.volume = 1;
    }

    public void StopNote()
    {
        m_audioSource.volume = 0;
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
        if(rhythmString.Length < 1) return;

        m_rhythmObjects = new();

        float currentX = xOffset;

        bool buildingDash = false;
        float dashStart = -1f;

        float t_lastPaper = 0f;

        // add paper
        for(int i = 0; i < rhythmString.Length; i++)
        {
            // skip first six beats
            if(i < 6){
            } else if(i == 6)
            {
                GameObject t_paperStart = Instantiate(paperStart, lineParent);

                t_paperStart.transform.localPosition = new Vector3(currentX, 0, 0);

                // TODO: gross hardcoding
                t_paperStart.transform.localScale = new Vector3(4, 4, 4);

                t_lastPaper = currentX;
            } else
            {
                if(currentX - t_lastPaper > paperSpacing)
                {
                    GameObject t_paperLoop = Instantiate(paperLoop, lineParent);

                    t_paperLoop.transform.localPosition = new Vector3(currentX, 0, 0);
                    // TODO: gross hardcoding
                    t_paperLoop.transform.localScale = new Vector3(4, 4, 4);

                    t_lastPaper = currentX;
                }
            }

            currentX += beatSpacing;
        }

        currentX = xOffset;

        // add notes
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
                    
                    m_rhythmObjects.Add(t_dash.GetComponent<NoteBehaviour>());

                    buildingDash = false;
                }

                if(c == '.')
                {
                    GameObject t_dot = Instantiate(dotPrefab, lineParent);

                    t_dot.transform.localPosition = currentPosition;

                    m_rhythmObjects.Add(t_dot.GetComponent<NoteBehaviour>());
                }
            }

            currentX += beatSpacing;
        }
    }
}
