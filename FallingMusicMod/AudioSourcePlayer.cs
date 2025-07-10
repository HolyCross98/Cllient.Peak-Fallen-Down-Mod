using BepInEx;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking; // We need this for the new method

namespace FallingMusicMod
{
    public class AudioSourcePlayer : MonoBehaviour
    {
        private AudioSource _audioSource;

        public float fadeDuration = 2.0f;
        private float _originalVolume;
        private bool _isPaused = false;

        void Awake()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.loop = true;

            _originalVolume = 0.10f;
            _audioSource.volume = _originalVolume;

            // The audio loading is now a coroutine
            StartCoroutine(LoadAudio());
        }

        // LoadAudio is now an IEnumerator because UnityWebRequest is an asynchronous operation
        private IEnumerator LoadAudio()
        {
            // IMPORTANT: This path now points to an .ogg file. 
            // OGG is a highly recommended format for mods.
            // Please convert your sound to "Fallen Down.ogg" and place it in your folder.
            string audioPath = Path.Combine(Paths.PluginPath, "holycross98-FallenDownMod", "Fallen Down.ogg");

            // We must add "file://" to the path for UnityWebRequest to load a local file
            string fullPath = "file://" + audioPath;

            Debug.Log("Attempting to load audio from: " + fullPath);

            // Let Unity's native audio loader handle the file.
            // AudioType.OGGVORBIS is used here. If you use a WAV, change it to AudioType.WAV.
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(fullPath, AudioType.OGGVORBIS);

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogError("UnityWebRequest Error: " + request.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                if (clip != null)
                {
                    _audioSource.clip = clip;
                    Debug.Log("Successfully loaded audio clip!");
                }
                else
                {
                    Debug.LogError("Failed to get AudioClip from request.");
                }
            }
        }

        public void PlayOrResume()
        {
            StopAllCoroutines();
            _audioSource.volume = _originalVolume;

            if (_isPaused)
            {
                _audioSource.UnPause();
            }
            else if (!_audioSource.isPlaying)
            {
                _audioSource.Play();
            }
            _isPaused = false;
        }

        public void InitiateFadeOut()
        {
            if (_audioSource.isPlaying)
            {
                StartCoroutine(FadeOutAndPause());
            }
        }

        private IEnumerator FadeOutAndPause()
        {
            float startVolume = _audioSource.volume;
            while (_audioSource.volume > 0)
            {
                _audioSource.volume -= startVolume * Time.deltaTime / fadeDuration;
                yield return null;
            }

            _audioSource.Pause();
            _isPaused = true;
            _audioSource.volume = _originalVolume;
        }
    }
}