using System;
using System.Collections.Generic;
using UnityEngine;

public class RhythmPlayer : MonoBehaviour
{
    // serialized //

    [Header("Objects/Prefabs")]

    public Transform lineParent;

    public GameObject dotPrefab;

    public GameObject dashPrefab;

    [Header("Settings")]

    public float beatSize = 1f;

    [Tooltip("Given as percentage of a total beat (only effects the last dash)")]
    public float dashSize = 0.8f;

    // non-serialized //

    [NonSerialized] public string rhythm;


    private List<GameObject> m_rhythmObjects;


    // event listeners //

    void Start()
    {
        // TEMP: assign rhythm
        rhythm = Rhythms.TEST_LEVEL;

        BuildRhythmObjects(beatSize);
    }

    void Update()
    {
        
    }

    // utils //

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

                    buildingDash = false;
                }

                if(c == '.')
                {
                    GameObject t_dot = Instantiate(dotPrefab, lineParent);

                    t_dot.transform.localPosition = currentPosition;
                }
            }

            currentX += beatSpacing;
        }
    }
}
