using UnityEngine;

public class NoteBehaviour : MonoBehaviour
{
    public bool hitByPlayer = false;

    public bool missed = false;
    
    public SpriteRenderer spriteRenderer;
    
    public Color hitColor;

    public Color missedColor;

    private Color m_baseColor;

    void Start()
    {
        m_baseColor = spriteRenderer.color;    
    }

    public void MarkAsHit()
    {
        spriteRenderer.color = hitColor;

        hitByPlayer = true;
    }

    public void MarkAsMissed()
    {
        spriteRenderer.color = missedColor;

        missed = true;
    }

    public void Reset()
    {
        spriteRenderer.color = m_baseColor;

        missed = false;
        hitByPlayer = false;
    }
}
