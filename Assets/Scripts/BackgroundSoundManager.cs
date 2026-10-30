using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class BackgroundSoundManager : Singleton<BackgroundSoundManager>
{
    [SerializeField] private EventReference _backgroundMusic;
    private EventInstance instanciaMusica;
    public EventInstance InstanciaMusica => instanciaMusica;

    void OnEnable()
    {
        GameEvents.AudioSystem.StartGame += StartGame;
    }

    void OnDisable()
    {
        GameEvents.AudioSystem.StartGame -= StartGame;
    }

    public void InicializarMusica()
    {
        instanciaMusica = Utils.FmodSoundPlay(instanciaMusica, _backgroundMusic);
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
