using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ObituaryTomorrow.UI
{
    public sealed class MainMenuTipController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panelTip;
        [SerializeField] private Text tipText;

        [Header("Timing (seconds)")]
        [SerializeField] private float fadeInSeconds = 1f;
        [SerializeField] private float displaySeconds = 3f;
        [SerializeField] private float fadeOutSeconds = 1f;

        private Coroutine tipRoutine;
        private Color tipTextBaseColor = Color.white;

        private void Awake()
        {
            ResolveReferences();
            CacheTipTextColor();
        }

        private void Start()
        {
            PlayTipSequence();
        }

        private void OnDisable()
        {
            if (tipRoutine != null)
            {
                StopCoroutine(tipRoutine);
                tipRoutine = null;
            }
        }

        public void PlayTipSequence()
        {
            ResolveReferences();

            if (panelTip == null || tipText == null)
            {
                Debug.LogWarning("MainMenuTipController: Panel_Tip or Tip_Text is missing.");
                return;
            }

            if (tipRoutine != null)
            {
                StopCoroutine(tipRoutine);
            }

            panelTip.SetActive(true);
            SetTipTextAlpha(0f);
            tipRoutine = StartCoroutine(TipSequenceRoutine());
        }

        private IEnumerator TipSequenceRoutine()
        {
            yield return FadeTipText(0f, 1f, fadeInSeconds);
            yield return new WaitForSeconds(Mathf.Max(0f, displaySeconds));
            yield return FadeTipText(1f, 0f, fadeOutSeconds);

            panelTip.SetActive(false);
            SetTipTextAlpha(1f);
            tipRoutine = null;
        }

        private IEnumerator FadeTipText(float fromAlpha, float toAlpha, float duration)
        {
            if (duration <= 0f)
            {
                SetTipTextAlpha(toAlpha);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                SetTipTextAlpha(Mathf.Lerp(fromAlpha, toAlpha, t));
                yield return null;
            }

            SetTipTextAlpha(toAlpha);
        }

        private void ResolveReferences()
        {
            if (panelTip == null)
            {
                panelTip = gameObject;
            }

            if (tipText == null)
            {
                tipText = GetComponentInChildren<Text>(true);
            }
        }

        private void CacheTipTextColor()
        {
            if (tipText != null)
            {
                tipTextBaseColor = tipText.color;
            }
        }

        private void SetTipTextAlpha(float alpha)
        {
            if (tipText == null)
            {
                return;
            }

            Color color = tipTextBaseColor;
            color.a = alpha;
            tipText.color = color;
        }
    }
}
