using Common.Scripts;
using UnityEngine;
using UnityEngine.Audio;

namespace TetrisGame.GameLogic
{
    public class AudioMixerManager : MonoBehaviour
    {
        [SerializeField] private AudioMixer audioMixer;

        private void OnEnable()
        {
            GameEvent.OnGameOver += SetEffect;
        }

        private void OnDisable()
        {
            GameEvent.OnGameOver -= SetEffect;
            audioMixer.SetFloat("Cutoff", 22000f);
        }

        private void SetEffect()
        {
            audioMixer.SetFloat("Cutoff", 400f);
        }

        public void SetMusicVolume(float level)
        {
            audioMixer.SetFloat("Volume", Mathf.Log10(level) * 20f);
        }
    }
}
