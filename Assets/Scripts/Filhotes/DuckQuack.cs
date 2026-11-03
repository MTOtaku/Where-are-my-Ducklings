using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

// Fica no Player: escuta o botão de Quack (W), identifica os filhotes pelas duas áreas
// (Search e Follow) e mantém a fila de filhotes que seguem a mamãe.
public class DuckQuack : MonoBehaviour {
    [SerializeField] DuckInput input;

    [Header("Áreas")]
    [Tooltip("Area Search: raio (m) em que o Quack é ouvido e em que o filhote continua seguindo. Se o filhote passar disso, ele se perde.")]
    [SerializeField] float searchRadius = 10f;
    [Tooltip("Area Follow: raio (m) em que o filhote passa a seguir a mamãe ao ouvir o Quack. Deve ser MENOR que a Search.")]
    [SerializeField] float followRadius = 4f;

    [Header("Quack")]
    [Tooltip("Tempo mínimo (segundos) entre dois Quacks. Evita spam do botão.")]
    [SerializeField] float quackCooldown = 0.5f;

    [Header("Fila de filhotes")]
    [Tooltip("Distância (m) entre cada filhote da fila, medida ao longo do caminho que a mamãe percorreu.")]
    [SerializeField] float followSpacing = 1f;
    [Tooltip("Distância mínima (m) entre dois pontos gravados do caminho da mamãe.")]
    [SerializeField] float trailStep = 0.1f;
    [Tooltip("Quantidade máxima de pontos do caminho guardados (com passo de 0.1 m, 300 pontos = ~30 m de trilha).")]
    [SerializeField] int maxTrailPoints = 300;
    [SerializeField] private EventReference _quackSound;

    private EventInstance duckSoundInstance;

    [SerializeField] bool debugLogs;

    // O pato deu um Quack da pra usar para o som e a animação da mamãe
    public event Action Quacked;
    // Um filhote ainda não recuperado ouviu o Quack: recebe o filhote e a direção (da mamãe até ele) para o indicador
    public event Action<Duckling, Vector2> DucklingResponded;
    public event Action<Duckling> DucklingJoined;
    public event Action<Duckling> DucklingLeft;

    public float SearchRadius => searchRadius;
    public float FollowRadius => followRadius;
    public int FollowerCount => followers.Count;

    readonly List<Duckling> followers = new List<Duckling>();   // [0] = o mais perto da mamãe
    readonly List<Vector2> trail = new List<Vector2>();         // caminho da mamãe, [último] = o mais novo
    DuckFacing facing;
    float lastQuackTime = -Mathf.Infinity;

    void Awake() {
        facing = GetComponent<DuckFacing>();
        if (input == null) input = GetComponent<DuckInput>();
        if (input == null) input = FindFirstObjectByType<DuckInput>();
    }

    void OnEnable() {
        if (input != null) input.QuackPressed += OnQuackPressed;
    }

    void OnDisable() {
        if (input != null) input.QuackPressed -= OnQuackPressed;
    }

    void OnValidate() {
        followSpacing = Mathf.Max(0.1f, followSpacing);
        trailStep = Mathf.Max(0.02f, trailStep);
        maxTrailPoints = Mathf.Max(10, maxTrailPoints);

        if (followRadius >= searchRadius)
            Debug.LogWarning("DuckQuack: o raio Follow deve ser MENOR que o raio Search.", this);
    }

    void Update() {
        // Grava o caminho da mamãe: os filhotes seguem esse caminho
        Vector2 pos = transform.position;
        if (trail.Count == 0 || Vector2.Distance(trail[trail.Count - 1], pos) >= trailStep) {
            trail.Add(pos);
            if (trail.Count > maxTrailPoints) trail.RemoveAt(0);
        }
    }

    void OnQuackPressed() {
        if (Time.time < lastQuackTime + quackCooldown) return;
        lastQuackTime = Time.time;

        Quacked?.Invoke();
        if(!Utils.IsMusicPlaying(duckSoundInstance))
            duckSoundInstance = Utils.FmodSoundPlay(duckSoundInstance, _quackSound);
        Vector2 origin = transform.position;

        for (int i = Duckling.All.Count - 1; i >= 0; i--) {
            Duckling d = Duckling.All[i];
            if (d.State == Duckling.DucklingState.Following) continue;

            Vector2 toDuckling = (Vector2)d.transform.position - origin;
            float dist = toDuckling.magnitude;

            if (dist <= followRadius) {
                // Dentro da Area Follow: passa a seguir (ou volta a seguir, se já tinha sido recuperado)
                d.StartFollowing(this);
            }
            else if (dist <= searchRadius) {
                if (d.IsRecovered) {
                    // Já recuperado, dentro da Search e fora da Follow: caminha até a mamãe
                    d.StartReturning(this);
                }
                else {
                    // Ainda não recuperado: só responde ao chamado (indicador sonoro com a direção dele)
                    d.Respond();
                    Vector2 dir = toDuckling.normalized;
                    if (debugLogs) Debug.Log($"{d.name} respondeu ao Quack. Direção: {dir}", d);
                    DucklingResponded?.Invoke(d, dir);
                }
            }
        }
    }

    // ---- Chamados pelos Filhotes ----------------------------------------------------------

    public void AddFollower(Duckling d) {
        if (followers.Contains(d)) return;
        followers.Add(d);
        if (debugLogs) Debug.Log($"{d.name} passou a seguir a mamãe ({followers.Count} na fila)", d);
        DucklingJoined?.Invoke(d);
    }

    public void RemoveFollower(Duckling d) {
        if (!followers.Remove(d)) return;
        if (debugLogs) Debug.Log($"{d.name} se perdeu da mamãe ({followers.Count} na fila)", d);
        DucklingLeft?.Invoke(d);
    }

    // Posição em que o filhote deve ficar: a (posição na fila + 1) × followSpacing metros ATRÁS, ao longo do caminho da mamãe
    public Vector2 GetFollowPosition(Duckling d) {
        int index = followers.IndexOf(d);
        if (index < 0) index = followers.Count;
        return GetPointBehind((index + 1) * followSpacing);
    }

    Vector2 GetPointBehind(float distance) {
        Vector2 prev = transform.position;
        float remaining = distance;

        for (int i = trail.Count - 1; i >= 0; i--) {
            Vector2 p = trail[i];
            float segment = Vector2.Distance(prev, p);

            if (segment >= remaining)
                return prev + (p - prev) * (remaining / segment);

            remaining -= segment;
            prev = p;
        }

        // Caminho ainda curto (a mamãe quase não andou): estende para trás, no lado oposto ao que ela olha
        float side = facing != null ? facing.Facing : 1f;
        return prev + Vector2.left * side * remaining;
    }

    // Círculos na Scene view (com o Player selecionado): amarelo = Search, verde = Follow
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, followRadius);
    }
}