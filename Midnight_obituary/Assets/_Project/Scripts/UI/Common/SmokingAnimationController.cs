using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace ObituaryTomorrow.UI
{
    /// <summary>
    /// Plays a smoking animation: sequence frame sprites → MP4 video with fade in/out.
    /// Attach to the smoking animation panel root.
    /// </summary>
    public sealed class SmokingAnimationController : MonoBehaviour
    {
        [Header("Display")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Image spriteDisplay;
        [SerializeField] private RawImage videoDisplay;
        [SerializeField] private VideoPlayer videoPlayer;

        [Header("Sequence Frames")]
        [SerializeField] private Sprite[] sequenceFrames;
        [SerializeField] private float frameDuration = 0.12f;

        [Header("Video")]
        [SerializeField] private float videoFadeInDuration = 0.3f;
        [SerializeField] private float videoFadeOutDuration = 0.5f;
        [SerializeField] private float videoDurationSeconds = 3f;

        public event Action OnAnimationComplete;

        private Coroutine playRoutine;

        private void Awake()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
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
            if (playRoutine != null)
            {
                return;
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            playRoutine = StartCoroutine(PlayRoutine());
        }

        private IEnumerator PlayRoutine()
        {
            bool hasFrames = sequenceFrames != null && sequenceFrames.Length > 0;

            if (hasFrames)
            {
                if (spriteDisplay != null)
                {
                    spriteDisplay.gameObject.SetActive(true);
                    spriteDisplay.color = Color.white;
                }

                if (videoDisplay != null)
                {
                    videoDisplay.gameObject.SetActive(false);
                }

                foreach (Sprite frame in sequenceFrames)
                {
                    if (frame == null)
                    {
                        continue;
                    }

                    if (spriteDisplay != null)
                    {
                        spriteDisplay.sprite = frame;
                    }

                    yield return new WaitForSeconds(frameDuration);
                }
            }

            // ── switch to video ──
            if (spriteDisplay != null)
            {
                spriteDisplay.gameObject.SetActive(false);
            }

            if (videoPlayer != null && videoDisplay != null)
            {
                videoDisplay.gameObject.SetActive(true);

                // fade in
                Color c = videoDisplay.color;
                c.a = 0f;
                videoDisplay.color = c;

                float elapsed = 0f;
                while (elapsed < videoFadeInDuration)
                {
                    elapsed += Time.deltaTime;
                    c.a = Mathf.Clamp01(elapsed / videoFadeInDuration);
                    videoDisplay.color = c;
                    yield return null;
                }

                c.a = 1f;
                videoDisplay.color = c;

                videoPlayer.Play();

                // wait for video
                float videoElapsed = 0f;
                while (videoElapsed < videoDurationSeconds && videoPlayer.isPlaying)
                {
                    videoElapsed += Time.deltaTime;
                    yield return null;
                }

                if (videoPlayer.isPlaying)
                {
                    videoPlayer.Pause();
                }

                // fade out
                elapsed = 0f;
                while (elapsed < videoFadeOutDuration)
                {
                    elapsed += Time.deltaTime;
                    c.a = 1f - Mathf.Clamp01(elapsed / videoFadeOutDuration);
                    videoDisplay.color = c;
                    yield return null;
                }

                c.a = 0f;
                videoDisplay.color = c;
                videoPlayer.Stop();
            }

            // ── done ──
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            playRoutine = null;
            OnAnimationComplete?.Invoke();
        }
    }
}
