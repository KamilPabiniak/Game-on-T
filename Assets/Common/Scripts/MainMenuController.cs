using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common.Scripts
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField] private string gameSceneName = "Game";
        [SerializeField] private string uiSceneName = "UI";
        
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 1f;
        
        public void PlayGame()
        {
            if (fadeCanvasGroup != null)
            {
                StartCoroutine(FadeOutAndLoadScenes());
            }
            else
            {
                LoadScenes();
            }
        }
        
        private IEnumerator FadeOutAndLoadScenes()
        {
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                yield return null;
            }
            LoadScenes();
        }
        
        private void LoadScenes()
        {
            SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
            SceneManager.LoadScene(uiSceneName, LoadSceneMode.Additive);
        }
    }
}
