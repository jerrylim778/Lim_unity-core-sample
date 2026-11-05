using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Core.UI;

namespace Example
{
    /// <summary>
    /// PopupBase 사용 예시. 연출의 끝을 콜백으로 넘겨야 스택이 다음 단계로 간다.
    /// </summary>
    public sealed class PausePopup : PopupBase, IDimProvider
    {
        #region Constants

        private const float FADE_DURATION = 0.2f;
        private const float DIM_ALPHA = 0.7f;

        #endregion Constants

        #region Variables

        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Button _resumeButton;

        private Action _onCloseRequested;

        #endregion Variables

        #region Properties

        public float DimAlpha => DIM_ALPHA;

        #endregion Properties

        #region Unity Methods

        private void OnEnable()
        {
            _resumeButton.onClick.AddListener(OnResumeClick);
        }

        private void OnDisable()
        {
            _resumeButton.onClick.RemoveListener(OnResumeClick);
        }

        #endregion Unity Methods

        #region Public Methods

        public override void Initialize()
        {
            _group.alpha = 0f;
            _group.interactable = false;
        }

        public override void Show(Action onComplete = null)
        {
            StopAllCoroutines();
            StartCoroutine(Fade(1f, () =>
            {
                _group.interactable = true;
                onComplete?.Invoke();
            }));
        }

        public override void Hide(Action onComplete = null)
        {
            _group.interactable = false;
            StopAllCoroutines();
            StartCoroutine(Fade(0f, onComplete));
        }

        public void BindCloseRequest(Action onCloseRequested)
        {
            _onCloseRequested = onCloseRequested;
        }

        #endregion Public Methods

        #region Event Handlers

        private void OnResumeClick()
        {
            _onCloseRequested?.Invoke();
        }

        #endregion Event Handlers

        #region Help Methods

        private IEnumerator Fade(float target, Action onComplete)
        {
            float start = _group.alpha;
            float elapsed = 0f;

            while (elapsed < FADE_DURATION)
            {
                elapsed += Time.unscaledDeltaTime;
                _group.alpha = Mathf.Lerp(start, target, elapsed / FADE_DURATION);
                yield return null;
            }

            _group.alpha = target;
            onComplete?.Invoke();
        }

        #endregion Help Methods
    }
}
