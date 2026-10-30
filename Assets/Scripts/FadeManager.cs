using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{

    [SerializeField] private float animationDuration;
    [SerializeField] private Image fadeImage;
    private Material fadeMaterial;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fadeMaterial = fadeImage.material;
    }

    void OnEnable()
    {
        GameEvents.UI.OnFadeAnimation += StartFadeAnimation;
    }

    void OnDisable()
    {
        GameEvents.UI.OnFadeAnimation -= StartFadeAnimation;
    }

    private void StartFadeAnimation(bool isFadeIn, Action fadeAnimationFinish)
    {
        fadeImage.color = new Color(255, 255, 255, 1);
        fadeImage.raycastTarget = true;
        StartCoroutine(FadeAnimation(isFadeIn, fadeAnimationFinish));
    }

    private IEnumerator FadeAnimation(bool isFadeIn, Action fadeAnimationFinish)
    {
        var progress = 0.0f;
        var timer = 0.0f;
        while(progress < 1)
        {
            timer += Time.deltaTime;
            progress = Mathf.Clamp01(timer/ animationDuration);
            var initialValue = isFadeIn ? 0.01f : 1.0f;
            var finalValue = isFadeIn ? 1.0f: 0.01f;

            var scaleProgress = Mathf.Lerp(initialValue, finalValue, progress);
            var scaleName = "_scale";
            fadeMaterial.SetFloat(scaleName, scaleProgress);
            yield return null;
        }

        if (isFadeIn)
        {
            fadeImage.color = new Color(255, 255, 255, 0);
            fadeImage.raycastTarget = false;
        }

        fadeAnimationFinish?.Invoke();
    }
}
