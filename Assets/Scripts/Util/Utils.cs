using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Utils
{
    private static AsyncOperation _currentLoadingSceneOperation;
    public static AsyncOperation CurrentLoadingSceneOperation => _currentLoadingSceneOperation;

    public static void LoadScene(string newPath)
    {
        SceneManager.LoadScene(newPath);
    }

    public static void LoadSceneAsync(string newPath)
    {
        _currentLoadingSceneOperation = SceneManager.LoadSceneAsync(newPath);
    }

    public static void FinishLoadSceneAsync()
    {
        _currentLoadingSceneOperation = null;
    }

    public static bool IsMusicPlaying(EventInstance instance)
    {
        PLAYBACK_STATE state;
        instance.getPlaybackState(out state);
        
        // Retorna verdadeiro se o estado não for STOPPED
        return state != PLAYBACK_STATE.STOPPED;
    }

    public static EventInstance FmodSoundPlay(EventInstance soundInstance, EventReference soundReference)
    {
        soundInstance = RuntimeManager.CreateInstance(soundReference);
        soundInstance.start();
        return soundInstance;
    }
    public static EventInstance Fmod3DSoundPlay(EventInstance soundInstance, EventReference soundReference, GameObject soundObject)
    {
        soundInstance = RuntimeManager.CreateInstance(soundReference);
        RuntimeManager.AttachInstanceToGameObject(soundInstance, soundObject);

        soundInstance.start();
        return soundInstance;
    }
}