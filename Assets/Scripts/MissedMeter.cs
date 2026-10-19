using UnityEngine;
using UnityEngine.Events;

public class MissedMeter : MonoBehaviour
{
    [Header("Objects")]

    [SerializeField] private Transform m_innerScale;

    [SerializeField] private SpriteRenderer m_innerScaleSprite;

    [Header("Settings")]

    public int maxMisses = 20;

    public Color baseColor = Color.white;

    public Color badColor = Color.red;

    [Tooltip("Delta-corrected")]
    public float lerpRate = 0.01f;

    public UnityEvent onFail = new();

    // private //

    private int m_currentMisses = 0;

    public void AddMiss()
    {
        SetMisses(m_currentMisses + 1);        
    }

    public void SetMisses(int misses)
    {
        m_currentMisses = misses;

        if(m_currentMisses >= maxMisses)
        {
            onFail.Invoke();
        }
    }

    private float m_currentFractionalProgress = 0f;

	void Update()
	{
		// lerp fractional progress
        m_currentFractionalProgress = Mathf.Lerp(m_currentFractionalProgress, m_currentMisses, lerpRate * Time.deltaTime);

        // set scale and such appropriately
        float percent = m_currentFractionalProgress / maxMisses;

        percent = Mathf.Min(1, percent);

        Color c = Color.Lerp(baseColor, badColor, percent);

        m_innerScale.localScale = new(m_innerScale.localScale.x, percent, m_innerScale.localScale.z);
        m_innerScaleSprite.color = c;
	}
}
