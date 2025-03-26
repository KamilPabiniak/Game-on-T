using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common.Scripts
{
    public class GameOverController : MonoBehaviour
    {
        [Header("Scene Settings")]
        [Tooltip("Nazwa sceny gry (Game)")]
        [SerializeField] private string gameSceneName = "Game";
        [Tooltip("Nazwa sceny UI")]
        [SerializeField] private string uiSceneName = "UI";
        
        
        public void Restart()
        {
            RestartScenes();
        }
        
        private void RestartScenes()
        {
            SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
            SceneManager.LoadScene(uiSceneName, LoadSceneMode.Additive);
        }
    }
}
