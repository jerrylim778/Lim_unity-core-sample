using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.UI
{
    /// <summary>
    /// 팝업을 스택으로 쌓고 닫는 단일 창구.
    /// 열기(Open)와 보이기(Show)를 분리한 이유는, 생성 직후 값을 채운 뒤에 연출을 시작해야 하기 때문이다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PopupStack : MonoBehaviour
    {
        #region Variables

        [SerializeField] private Canvas _canvas;
        [SerializeField] private RectTransform _dimPanel;
        [SerializeField] private RectTransform _defaultLayerRoot;
        [SerializeField] private RectTransform _requiredLayerRoot;
        [SerializeField] private RectTransform _specialLayerRoot;
        [SerializeField] private PopupBase[] _prefabs;

        private readonly Stack<PopupBase> _stack = new();
        private readonly List<PopupBase> _updateBuffer = new();

        private CanvasGroup _dimGroup;

        #endregion Variables

        #region Properties

        public int Count => _stack.Count;

        #endregion Properties

        #region Unity Methods

        private void Awake()
        {
            _dimGroup = _dimPanel.GetComponent<CanvasGroup>();
            _dimPanel.gameObject.SetActive(false);
        }

        private void Update()
        {
            // 갱신 도중 팝업이 열리거나 닫히면 스택이 바뀐다. 그래서 복사본을 돈다.
            // AddRange(Stack) 는 열거자를 박싱해 매 프레임 할당이 생긴다. 구조체 열거자로 직접 옮긴다.
            _updateBuffer.Clear();
            foreach (PopupBase popup in _stack) _updateBuffer.Add(popup);

            foreach (PopupBase popup in _updateBuffer)
            {
                if (popup != null && popup.IsUpdating) popup.ProcessUpdate();
            }
        }

        #endregion Unity Methods

        #region Public Methods

        public T Open<T>() where T : PopupBase
        {
            if (!TryGetPrefab(out T prefab)) return null;

            T popup = Instantiate(prefab, GetLayerRoot(prefab.Layer));
            popup.name = prefab.name;
            Stretch(popup);

            _stack.Push(popup);
            popup.Initialize();
            RefreshDim();
            return popup;
        }

        public void Show<T>(Action onComplete = null) where T : PopupBase
        {
            if (!TryPeek(out T popup))
            {
                Debug.LogError($"[{nameof(PopupStack)}] 맨 위 팝업이 아니다: {typeof(T).Name}", this);
                return;
            }

            popup.Show(onComplete);
        }

        public void Close<T>(Action onComplete = null) where T : PopupBase
        {
            if (!TryPeek(out T popup))
            {
                Debug.LogError($"[{nameof(PopupStack)}] 맨 위 팝업이 아니다: {typeof(T).Name}", this);
                return;
            }

            _stack.Pop();
            popup.Hide(() =>
            {
                Destroy(popup.gameObject);
                RefreshDim();
                onComplete?.Invoke();
            });
        }

        public bool TryPeek<T>(out T popup) where T : PopupBase
        {
            popup = null;
            if (_stack.Count == 0) return false;

            popup = _stack.Peek() as T;
            return popup != null;
        }

        public bool TryFind<T>(out T popup) where T : PopupBase
        {
            foreach (PopupBase item in _stack)
            {
                if (item is not T match) continue;

                popup = match;
                return true;
            }

            popup = null;
            return false;
        }

        #endregion Public Methods

        #region Help Methods

        private bool TryGetPrefab<T>(out T prefab) where T : PopupBase
        {
            foreach (PopupBase item in _prefabs)
            {
                if (item is not T match) continue;

                prefab = match;
                return true;
            }

            Debug.LogError($"[{nameof(PopupStack)}] 등록되지 않은 프리팹: {typeof(T).Name}", this);
            prefab = null;
            return false;
        }

        private RectTransform GetLayerRoot(PopupLayer layer)
        {
            return layer switch
            {
                PopupLayer.Required => _requiredLayerRoot,
                PopupLayer.Special => _specialLayerRoot,
                _ => _defaultLayerRoot,
            };
        }

        // 캔버스 해상도가 기기마다 달라도 팝업이 같은 비율을 쓰도록 부모 Rect 를 그대로 따라간다.
        private void Stretch(PopupBase popup)
        {
            var target = (RectTransform)popup.transform;
            var reference = (RectTransform)_canvas.transform;

            target.anchorMin = Vector2.zero;
            target.anchorMax = Vector2.one;
            target.pivot = reference.pivot;
            target.offsetMin = Vector2.zero;
            target.offsetMax = Vector2.zero;
        }

        private void RefreshDim()
        {
            if (!TryPeekDimTarget(out PopupBase top))
            {
                _dimPanel.gameObject.SetActive(false);
                return;
            }

            _dimPanel.gameObject.SetActive(true);
            _dimPanel.SetParent(top.transform.parent, false);

            // 딤을 맨 위 팝업 바로 뒤에 둔다. 딤이 이미 팝업보다 앞 번호면 빼는 순간 팝업 번호가 하나 당겨진다.
            int topIndex = top.transform.GetSiblingIndex();
            _dimPanel.SetSiblingIndex(_dimPanel.GetSiblingIndex() < topIndex ? topIndex - 1 : topIndex);
            _dimGroup.alpha = top is IDimProvider provider ? provider.DimAlpha : 1f;
        }

        private bool TryPeekDimTarget(out PopupBase top)
        {
            top = null;

            foreach (PopupBase popup in _stack)
            {
                if (popup.Layer != PopupLayer.Default) continue;

                top = popup;
                return true;
            }

            return false;
        }

        #endregion Help Methods
    }
}
