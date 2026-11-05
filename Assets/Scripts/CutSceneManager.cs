using System.Collections;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.Video;

public class CutSceneManager : MonoBehaviour
{
    [SerializeField] private VideoPlayer video;
    private const string TITLE_SCREEN_SCENE = "Title_Screen";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(ShowVideo());
    }

    IEnumerator ShowVideo()
    {
        Debug.Log(video.length- 1f);
        yield return new WaitForSeconds(18f);
        video.Pause();
        GameEvents.UI.OnFadeAnimation.Invoke(false, GoToMainMenu);
    }

    private void GoToMainMenu()
    {
        Utils.LoadScene(TITLE_SCREEN_SCENE);
    }
}
