using FMODUnity;
using UnityEngine;

// Pausa o áudio do FMOD junto com o jogo. Coloque UM na cena (ou no objeto do PauseController).
public class FmodPauseHook : MonoBehaviour {
    [Tooltip("Bus do FMOD que é pausado. 'bus:/' = tudo. Para o menu de pause ter música/sons, " +
             "use um bus só de gameplay (ex.: bus:/Gameplay) e deixe o do menu fora.")]
    [SerializeField] string busPath = "bus:/";

    void OnEnable() {
        PauseManager.PauseChanged += OnPauseChanged;
        if (PauseManager.IsPaused) OnPauseChanged(true);
    }

    void OnDisable() {
        PauseManager.PauseChanged -= OnPauseChanged;
        OnPauseChanged(false);      // nunca deixa o áudio preso em pausa
    }

    void OnPauseChanged(bool paused) {
        if (!RuntimeManager.IsInitialized) return;

        try {
            RuntimeManager.GetBus(busPath).setPaused(paused);
        }
        catch (BusNotFoundException) {
            Debug.LogWarning($"FmodPauseHook: bus '{busPath}' não existe no projeto FMOD.", this);
        }
    }
}