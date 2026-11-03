using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

// O filhote. Três estados:
//  Lost      = esperando parado (ainda não recuperado, ou se perdeu da mamãe)
//  Returning = recuperado, mas longe da mamãe: caminha até ela depois de um Quack
//  Following = flutuando atrás da mamãe
public class Duckling : MonoBehaviour {
    public enum DucklingState { Lost, Returning, Following }

    // Todos os filhotes ativos da cena. O DuckQuack consulta esta lista (não precisa de colliders nem layers)
    public static readonly List<Duckling> All = new List<Duckling>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => All.Clear();

    [Header("Movimento")]
    [Tooltip("Velocidade máxima do filhote (m/s). Precisa ser MENOR que a do pato: é isso que faz o filhote ficar para trás e se perder.")]
    [SerializeField] float followSpeed = 4f;
    [Tooltip("Suavização do movimento (segundos). Menor = mais rígido, maior = mais 'flutuando'.")]
    [SerializeField] float smoothTime = 0.15f;

    [Header("Flutuação")]
    [Tooltip("Objeto filho com o desenho do filhote. Sobe e desce de leve enquanto flutua atrás da mamãe. Opcional.")]
    [SerializeField] Transform visual;
    [Tooltip("Quanto o filhote sobe e desce (m) enquanto flutua.")]
    [SerializeField] float bobAmplitude = 0.12f;
    [Tooltip("Velocidade da subida e descida.")]
    [SerializeField] float bobSpeed = 4f;

    [Header("Perdido")]
    [Tooltip("Layer do chão. Se o filhote se perder no ar, ele cai até o chão. Deixe vazio para ele ficar parado onde foi perdido.")]
    [SerializeField] LayerMask groundMask;
    [Tooltip("Distância (m) do centro do filhote até os pés, para ele pousar sem afundar no chão.")]
    [SerializeField] float groundOffset = 0.3f;
    [Tooltip("Aceleração da queda do filhote perdido (m/s²).")]
    [SerializeField] float fallGravity = 20f;

    [SerializeField] bool debugLogs;
    [SerializeField] private EventReference _quackSound;
    [SerializeField] private EventReference _foundDucklingSound;

    private EventInstance ducklingSoundInstance;
    
    public DucklingState State { get; private set; } = DucklingState.Lost;
    // Já foi recuperado alguma vez? (muda o que acontece quando ele ouve o Quack)
    public bool IsRecovered { get; private set; }

    public event Action<DucklingState> StateChanged;
    // O filhote ouviu o Quack e respondeu (use para som/animação do filhote)
    public event Action Responded;

    DuckQuack mother;
    Vector2 velocity;
    float fallSpeed;
    float bobTime;
    Vector3 visualBaseLocalPos;

    void Awake() {
        if (visual != null) visualBaseLocalPos = visual.localPosition;
        bobTime = UnityEngine.Random.value * Mathf.PI * 2f;   // cada filhote flutua em um ritmo diferente
    }

    void OnEnable() => All.Add(this);

    void OnDisable() {
        All.Remove(this);
        if (mother != null) mother.RemoveFollower(this);
    }

    // ---- Chamados pelo DuckQuack -----------------------------------------------------------

    public void StartFollowing(DuckQuack newMother) {
        ducklingSoundInstance = Utils.Fmod3DSoundPlay(ducklingSoundInstance, _foundDucklingSound, gameObject);
        mother = newMother;
        IsRecovered = true;
        velocity = Vector2.zero;
        mother.AddFollower(this);
        SetState(DucklingState.Following);
    }

    public void StartReturning(DuckQuack newMother) {
        if (State == DucklingState.Returning) return;
        mother = newMother;
        velocity = Vector2.zero;
        SetState(DucklingState.Returning);
    }

    // Fica perdido onde está (a mamãe se afastou demais)
    public void StopFollowing() {
        if (mother != null) mother.RemoveFollower(this);
        mother = null;
        fallSpeed = 0f;
        SetState(DucklingState.Lost);
    }

    public void Respond()
    {
        Debug.Log("Patinho respondendo");
        ducklingSoundInstance = Utils.Fmod3DSoundPlay(ducklingSoundInstance, _quackSound, gameObject);
        Responded?.Invoke();
    }

    // ----------------------------------------------------------------------------------------

    void Update() {
        // A mamãe foi destruída (troca de cena, por exemplo): fica perdido
        if (State != DucklingState.Lost && mother == null) SetState(DucklingState.Lost);

        switch (State) {
            case DucklingState.Following: UpdateFollowing(); break;
            case DucklingState.Returning: UpdateReturning(); break;
            default: UpdateLost(); break;
        }

        UpdateBob();
    }

    void UpdateFollowing() {
        Vector2 motherPos = mother.transform.position;

        // Passou do raio Search: o filhote não acompanha mais
        if (Vector2.Distance(transform.position, motherPos) > mother.SearchRadius) {
            StopFollowing();
            return;
        }

        Vector2 target = mother.GetFollowPosition(this);
        Vector2 pos = Vector2.SmoothDamp(transform.position, target, ref velocity, smoothTime, followSpeed, Time.deltaTime);
        SetPosition2D(pos);
    }

    void UpdateReturning() {
        Vector2 motherPos = mother.transform.position;
        float dist = Vector2.Distance(transform.position, motherPos);

        // A mamãe se afastou além do Search enquanto ele voltava: desiste
        if (dist > mother.SearchRadius) {
            StopFollowing();
            return;
        }

        // Entrou na área Follow: retoma o acompanhamento normal
        if (dist <= mother.FollowRadius) {
            StartFollowing(mother);
            return;
        }

        // Caminha direto até a mamãe, na velocidade do filhote
        SetPosition2D(Vector2.MoveTowards(transform.position, motherPos, followSpeed * Time.deltaTime));
    }

    void UpdateLost() {
        if (groundMask.value == 0) return;

        Vector2 pos = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.down, 200f, groundMask);
        if (hit.collider == null) return;

        float gap = hit.distance - groundOffset;
        if (gap <= 0.01f) { fallSpeed = 0f; return; }

        fallSpeed += fallGravity * Time.deltaTime;
        float step = Mathf.Min(gap, fallSpeed * Time.deltaTime);
        SetPosition2D(pos + Vector2.down * step);
    }

    void UpdateBob() {
        if (visual == null) return;

        bobTime += Time.deltaTime * bobSpeed;
        float offset = State == DucklingState.Lost ? 0f : Mathf.Sin(bobTime) * bobAmplitude;
        visual.localPosition = visualBaseLocalPos + Vector3.up * offset;
    }

    // Mexe só em X e Y, preservando o Z (ordem de desenho)
    void SetPosition2D(Vector2 p) => transform.position = new Vector3(p.x, p.y, transform.position.z);

    void SetState(DucklingState newState) {
        if (State == newState) return;
        State = newState;
        if (debugLogs) Debug.Log($"{name}: {newState}", this);
        StateChanged?.Invoke(newState);
    }
}