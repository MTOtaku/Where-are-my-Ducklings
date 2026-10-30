using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class BackgroundSoundManager : MonoBehaviour
{
    [SerializeField] private EventReference _backgroundMusic;
    private EventInstance instanciaMusica;

    void Start()
    {
        instanciaMusica = Utils.FmodSoundPlay(instanciaMusica, _backgroundMusic);
    }

    void OnEnable()
    {
        GameEvents.AudioSystem.StartGame += StartGame;
    }

    void OnDisable()
    {
        GameEvents.AudioSystem.StartGame -= StartGame;
    }

    bool IsMusicPlaying(EventInstance instance)
    {
        PLAYBACK_STATE state;
        instance.getPlaybackState(out state);
        
        // Retorna verdadeiro se o estado não for STOPPED
        return state != PLAYBACK_STATE.STOPPED;
    }

    private void StartGame()
    {
        ChangeParameter(instanciaMusica, "menugameplay", 100);
    }

    void ChangeParameter(EventInstance instance, string parameterName, int newValue)
    {
        instance.setParameterByName(parameterName, newValue);
    }
}
