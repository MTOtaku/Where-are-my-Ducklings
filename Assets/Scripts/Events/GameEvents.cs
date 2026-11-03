using System;
using UnityEngine;

public static class GameEvents
    {
        public static class Localization
        {
            public static Action OnChangeLocalization;
        }

        public static class GameSystem {
            public static Action<BoxCollider2D> OnStartLevel;
            public static Action<float> OnLevelCompleted; // vitória ativada: recebe o tempo final 
        }

        public static class UI
        {
            public static Action<bool, Action> OnFadeAnimation;
        }

        public static class AudioSystem
        {
            public static Action StartGame;
        }
    }
