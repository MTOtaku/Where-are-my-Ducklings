using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{

    [SerializeField] private float animationDuration;
    [SerializeField] private GameObject bg;
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
        if(isFadeIn)
            yield return new WaitForSeconds(0.2f);
        
        bg.SetActive(false);
        while(progress < 1)
        {
            timer += Time.unscaledDeltaTime;
            progress = Mathf.Clamp01(timer/ animationDuration);
            var initialValue = isFadeIn ? 0.01f : 5f;
            var finalValue = isFadeIn ? 5f: 0.01f;

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
        bg.SetActive(!isFadeIn);
        fadeAnimationFinish?.Invoke();
    }
}
