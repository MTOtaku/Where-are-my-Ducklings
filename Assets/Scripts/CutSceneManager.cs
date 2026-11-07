using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Video;

public class CutSceneManager : MonoBehaviour
{
    [SerializeField] private VideoPlayer video;
    [SerializeField] private float waitingTimerForFadeout;
    private const string TITLE_SCREEN_SCENE = "Title_Screen";

    [SerializeField] private EventReference _cutSceneSound;

    private EventInstance cutSceneInstance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameEvents.UI.OnFadeAnimation?.Invoke(true, ShowVideo);
        cutSceneInstance = Utils.FmodSoundPlay(cutSceneInstance, _cutSceneSound);
    }

    void OnDisable()
    {
        cutSceneInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    private void ShowVideo(){
        StartCoroutine(CoroutineShowVideo());
    }
    IEnumerator CoroutineShowVideo()
    {
        yield return new WaitForSeconds(16.7f);
        video.Pause();
        yield return new WaitForSeconds(waitingTimerForFadeout);
        GameEvents.UI.OnFadeAnimation.Invoke(false, GoToMainMenu);
    }

    private void GoToMainMenu()
    {
        Utils.LoadScene(TITLE_SCREEN_SCENE);
    }
}
