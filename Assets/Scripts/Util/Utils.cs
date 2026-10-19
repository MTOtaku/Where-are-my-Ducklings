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
    }