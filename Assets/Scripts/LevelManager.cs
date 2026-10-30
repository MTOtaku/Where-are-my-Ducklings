using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private BoxCollider2D levelLimits;
    [SerializeField] private EventReference _ambienceSound;
    private EventInstance ambienceSoundInstance;

    [SerializeField] private EventReference _winSound;
    private EventInstance winSoundInstance;
    
    [SerializeField] private string victory_scene;
    void Start()
    {
        GameEvents.GameSystem.OnStartLevel?.Invoke(levelLimits);
        ambienceSoundInstance = Utils.Fmod3DSoundPlay(ambienceSoundInstance, _ambienceSound, gameObject);
        StartCoroutine(InitializeGame());
        GameEvents.GameSystem.OnLevelCompleted += LevelCompleted;
    }

    private void LevelCompleted(float gameTime)
    {
        ambienceSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        if(Utils.IsMusicPlaying(winSoundInstance))
            winSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        winSoundInstance = Utils.FmodSoundPlay(winSoundInstance, _winSound);
        Utils.LoadScene(victory_scene);
    }

    private IEnumerator InitializeGame()
    {
        yield return null;
        GameEvents.UI.OnFadeAnimation.Invoke(true, () => Debug.Log(""));
    }
}
