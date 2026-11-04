using UnityEngine;

// Marcador: Animators, partículas e Timeline deste objeto (e dos filhos) NÃO são congelados pela rede de segurança do pause.
// Use no painel do menu de pause e no Fade_Panel, que precisam continuar animando com o jogo parado
// (e configure o Animator deles como Unscaled Time).
public class IgnorePause : MonoBehaviour { }