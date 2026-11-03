using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

// Estado global de pause. Qualquer sistema novo só precisa de UMA destas duas coisas:
//   - ler PauseManager.IsPaused, ou
//   - assinar PauseManager.PauseChanged (true = pausou, false = voltou)
//
// O que o pause faz:
//   1) Time.timeScale = 0: para física, Update/FixedUpdate com Time.deltaTime, Animators, partículas, shaders e Cinemachine
//   2) AudioListener.pause: pausa todos os AudioSource 
//   3) Rede de segurança: congela o que usa tempo NÃO escalado (Animator Unscaled Time, partículas e Timeline unscaled),
//      exceto o que estiver dentro de um objeto com IgnorePause
//   4) Avisa o resto do jogo pelo evento (o DuckInput bloqueia os controles, o FMOD pausa o áudio etc.)
public static class PauseManager {
    public static bool IsPaused { get; private set; }

    // true = pausou, false = voltou ao jogo
    public static event Action<bool> PauseChanged;

    static float timeScaleBeforePause = 1f;
    static readonly List<(Animator animator, float speed)> frozenAnimators = new List<(Animator, float)>();
    static readonly List<ParticleSystem> frozenParticles = new List<ParticleSystem>();
    static readonly List<PlayableDirector> frozenDirectors = new List<PlayableDirector>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() {
        IsPaused = false;
        PauseChanged = null;
        timeScaleBeforePause = 1f;
        frozenAnimators.Clear();
        frozenParticles.Clear();
        frozenDirectors.Clear();

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Trocou de cena com o jogo pausado (ex.: Sair para o menu): volta ao normal, senão a próxima cena nasce parada
    static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (IsPaused) Resume();
    }

    public static void Toggle() {
        if (IsPaused) Resume();
        else Pause();
    }

    public static void Pause() {
        if (IsPaused) return;

        // Guarda o timeScale atual (pode não ser 1 se houver câmera lenta/hitstop) para devolver igual
        timeScaleBeforePause = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        FreezeUnscaled();

        IsPaused = true;
        PauseChanged?.Invoke(true);
    }

    public static void Resume() {
        if (!IsPaused) return;

        IsPaused = false;
        Time.timeScale = timeScaleBeforePause;
        AudioListener.pause = false;
        UnfreezeUnscaled();

        PauseChanged?.Invoke(false);
    }

    // ---- Rede de segurança para o que ignora o timeScale ------------------------------------------

    static void FreezeUnscaled() {
        foreach (Animator a in UnityEngine.Object.FindObjectsByType<Animator>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
            if (a.updateMode != AnimatorUpdateMode.UnscaledTime) continue;
            if (a.GetComponentInParent<IgnorePause>() != null) continue;
            frozenAnimators.Add((a, a.speed));
            a.speed = 0f;
        }

        foreach (ParticleSystem ps in UnityEngine.Object.FindObjectsByType<ParticleSystem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
            if (!ps.main.useUnscaledTime || !ps.isPlaying) continue;
            if (ps.GetComponentInParent<IgnorePause>() != null) continue;
            ps.Pause(false);
            frozenParticles.Add(ps);
        }

        foreach (PlayableDirector d in UnityEngine.Object.FindObjectsByType<PlayableDirector>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
            if (d.timeUpdateMode != DirectorUpdateMode.UnscaledGameTime || d.state != PlayState.Playing) continue;
            if (d.GetComponentInParent<IgnorePause>() != null) continue;
            d.Pause();
            frozenDirectors.Add(d);
        }
    }

    static void UnfreezeUnscaled() {
        foreach (var f in frozenAnimators) if (f.animator != null) f.animator.speed = f.speed;
        foreach (var ps in frozenParticles) if (ps != null) ps.Play(false);
        foreach (var d in frozenDirectors) if (d != null) d.Resume();

        frozenAnimators.Clear();
        frozenParticles.Clear();
        frozenDirectors.Clear();
    }
}