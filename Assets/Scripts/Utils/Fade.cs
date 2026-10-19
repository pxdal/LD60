using System.Collections;
using UnityEngine;

public static class Fade
{
    public static IEnumerator FadeObject(this GameObject gobj, float destinationOpacity, float duration, bool deleteWhenDone = false)
    {
        SpriteRenderer sr = gobj.GetComponentInChildren<SpriteRenderer>();

        float startOpacity = sr.color.a;
        
        float opacityPerSecond = (destinationOpacity - startOpacity) / duration;

        float startTime = Time.time;

        while(Time.time - startTime < duration)
        {
            float elapsed = Time.time - startTime;

            float total = opacityPerSecond * elapsed;

            sr.color = new(sr.color.r, sr.color.g, sr.color.b, startOpacity + total);

            yield return null;
        }

        sr.color = new(sr.color.r, sr.color.g, sr.color.b, destinationOpacity);

        if (deleteWhenDone)
        {
            GameObject.Destroy(gobj);
        }
    }

    public static IEnumerator FadeObjectAfter(this GameObject gobj, float destinationOpacity, float duration, float delay, bool deleteWhenDone = false)
    {
        yield return new WaitForSeconds(delay);

        yield return gobj.FadeObject(destinationOpacity, duration, deleteWhenDone);    
    }
}
