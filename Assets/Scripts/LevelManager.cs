using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private BoxCollider2D levelLimits;
    [SerializeField] private EventReference _ambienceSound;
    private EventInstance ambienceSoundInstance;
    
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
        GameManager.Instance.LevelCurrentTime = gameTime;
        BackgroundSoundManager.Instance.InstanciaMusica.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

        ambienceSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        Utils.LoadScene(victory_scene);
    }

    private IEnumerator InitializeGame()
    {
        yield return null;
        GameEvents.UI.OnFadeAnimation.Invoke(true, () => Debug.Log(""));
    }
}
