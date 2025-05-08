using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public static class FadeEffect
{
    public static IEnumerator FadeImage (Image target, float duration, Color color)
    {
        if (!target)
           yield break;

        float alpha = target.color.a;

        for (float t = 0.0f; t < 1.0f; t+= Time.deltaTime / duration)
        {
            if (!target)
               yield break;
            
            Color newColor = new Color(color.r, color.g, color.b, Mathf.SmoothStep(alpha, color.a, t));
            target.color = newColor;
            yield return null;
        }
        target.color = color;
    }

    public static IEnumerator FadeText (Text target, float duration, Color color)
    {
        if (!target)
           yield break;
        
        float alpha = target.color.a;

        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
        {
            if (!target)
               yield break;
            
            Color newColor = new Color(color.r, color.g, color.b, Mathf.SmoothStep(alpha, color.a, t));
            target.color = newColor;
            yield return null;
        }
        target.color = color;
    }

    public static IEnumerator FadeSprite (SpriteRenderer target, float duration, Color color)
    {
        if (!target)
           yield break;
        
        float alpha = target.material.color.a;
        float t = 0f;

        while (t < 1.0f)
        {
            if (!target)
               yield break;

            Color newColor = new Color(color.r, color.g, color.b, Mathf.SmoothStep(alpha, color.a, t));
            target.material.color = newColor;

            t += Time.deltaTime / duration;

            yield return null;
        }
        Color finalColor = new Color(color.r, color.g, color.b, Mathf.SmoothStep(alpha, color.a, t));
        if (target != null)
        {
            target.material.color = finalColor;
        }
    }

    public static IEnumerator FadeCanvasGroup (CanvasGroup target, float duration, float targetAlpha, bool unscaled = true)
    {
        if (!target)
           yield break;
        
        float currentAlpha = targetAlpha;
        float t = 0f;

        while (t < 1.0f)
        {
            if (!target)
               yield break;
            float newAlpha = Mathf.SmoothStep(currentAlpha, targetAlpha, t);
            targetAlpha = newAlpha;

            if (unscaled)
            {
                t += Time.unscaledDeltaTime / duration;
            } 
            else 
            {
                t += Time.deltaTime / duration;
            }

            yield return null;
        }
        target.alpha = targetAlpha;
    }
}
