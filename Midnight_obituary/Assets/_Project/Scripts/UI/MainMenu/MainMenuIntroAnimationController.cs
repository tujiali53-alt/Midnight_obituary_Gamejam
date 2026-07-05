using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace ObituaryTomorrow.UI
{
    public sealed class MainMenuIntroAnimationController : MonoBehaviour
    {
        [SerializeField] private VideoClip introVideo;
        [SerializeField] private Image backgroundImage;

        private VideoPlayer videoPlayer;
        private RawImage videoDisplay;
        private RenderTexture renderTexture;
        private Coroutine playRoutine;

        public event Action OnAnimationComplete;

        private void Awake()
        {
            if (backgroundImage == null)
            {
                backgroundImage = GetComponent<Image>();
            }

            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                playRoutine = null;
            }

            if (videoPlayer != null)
            {
                videoPlayer.Stop();
            }
        }

        public void Play()
        {
            if (introVideo == null)
            {
                Debug.LogWarning("MainMenuIntroAnimationController: comic_1 video is missing.");
                OnAnimationComplete?.Invoke();
                return;
            }

            if (playRoutine != null)
            {
                return;
            }

            gameObject.SetActive(true);
            playRoutine = StartCoroutine(PlayRoutine());
        }

        private IEnumerator PlayRoutine()
        {
            EnsureVideoSetup();

            videoPlayer.clip = introVideo;
            videoPlayer.Prepare();

            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }

            if (renderTexture != null
                && (renderTexture.width != introVideo.width || renderTexture.height != introVideo.height))
            {
                renderTexture.Release();
                renderTexture.width = Mathf.Max(16, (int)introVideo.width);
                renderTexture.height = Mathf.Max(16, (int)introVideo.height);
                renderTexture.Create();
            }

            if (videoDisplay != null)
            {
                videoDisplay.gameObject.SetActive(true);
            }

            videoPlayer.Play();

            while (videoPlayer.isPlaying)
            {
                yield return null;
            }

            if (videoPlayer != null)
            {
                videoPlayer.Stop();
            }

            gameObject.SetActive(false);
            playRoutine = null;
            OnAnimationComplete?.Invoke();
        }

        private void EnsureVideoSetup()
        {
            if (videoPlayer != null)
            {
                return;
            }

            videoPlayer = GetComponent<VideoPlayer>();
            if (videoPlayer == null)
            {
                videoPlayer = gameObject.AddComponent<VideoPlayer>();
            }

            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            videoPlayer.skipOnDrop = true;

            int width = introVideo != null ? Mathf.Max(16, (int)introVideo.width) : 1920;
            int height = introVideo != null ? Mathf.Max(16, (int)introVideo.height) : 1080;
            renderTexture = new RenderTexture(width, height, 0);
            videoPlayer.targetTexture = renderTexture;

            if (videoDisplay == null)
            {
                GameObject displayObject = new GameObject(
                    "VideoDisplay",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(RawImage));
                displayObject.transform.SetParent(transform, false);

                RectTransform rectTransform = displayObject.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;

                videoDisplay = displayObject.GetComponent<RawImage>();
                videoDisplay.raycastTarget = true;
            }

            videoDisplay.texture = renderTexture;
        }
    }
}
