using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TutorialManager : MonoBehaviour
{
    public GameObject pressZ;
    public GameObject pressM;
    public GameObject tapTheNotes;

    public UnityEvent onTutorialEnd;
    
    public float fadeDuration = 0.5f;

    public float tapTheNotesHoldTime = 1f;

    private bool m_tapped1 = false;
    private bool m_tapped2 = false;

    private bool m_startedFinalFade = false;

    // Update is called once per frame
    void Update()
    {
        if (GameController.Instance.Tapper.IsTapping1() && !m_tapped1)
        {
            StartCoroutine(pressZ.FadeObject(0f, fadeDuration, true));

            m_tapped1 = true;
        }

        if (GameController.Instance.Tapper.IsTapping2() && !m_tapped2)
        {
            StartCoroutine(pressM.FadeObject(0f, fadeDuration, true));

            m_tapped2 = true;
        }

        if(m_tapped1 && m_tapped2 && !m_startedFinalFade)
        {
            // fade in when first fade is done
            StartCoroutine(tapTheNotes.FadeObjectAfter(1f, fadeDuration, fadeDuration, false));

            // fade out after fade in + hold time
            StartCoroutine(tapTheNotes.FadeObjectAfter(0f, fadeDuration, fadeDuration + tapTheNotesHoldTime, true));

            // tutorial ends when the last fade starts
            StartCoroutine(TriggerEndAfter(fadeDuration + tapTheNotesHoldTime));

            m_startedFinalFade = true;
        }
    }

    IEnumerator TriggerEndAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        onTutorialEnd.Invoke();    
    }

    
}
