using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.Controllers
{
    /// <summary>
    /// 추상 계층이 실제로 쓰이는 예시. 화면 아무 곳이나 눌러 생기는 플로팅 조이스틱.
    /// 입력 값만 들고 있고, 그 값을 누가 어떻게 쓰는지는 모른다.
    /// </summary>
    public sealed class VirtualJoystickController : SubControllerBase,
        IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        #region Constants

        private const float DEFAULT_LEVER_RANGE = 1f;

        #endregion Constants

        #region Variables

        [SerializeField] private RectTransform _touchArea;
        [SerializeField] private RectTransform _background;
        [SerializeField] private RectTransform _lever;

        [SerializeField] private bool _isBackgroundAlwaysVisible;
        [SerializeField, Range(0f, 1f)] private float _deadZone;
        [SerializeField, Range(0f, 1f)] private float _leverRange = DEFAULT_LEVER_RANGE;

        private Canvas _canvas;
        private Camera _canvasCamera;
        private Vector2 _backgroundHomePosition;
        private bool _isDragging;

        #endregion Variables

        #region Properties

        public Vector2 Direction { get; private set; }

        #endregion Properties

        #region Public Methods

        public override object[] Initialize(params object[] args)
        {
            // 코드로 빈 오브젝트에 붙으면 참조가 비어 있다. 조이스틱은 UI 계층이 필요하므로 프리팹으로 배치해야 한다.
            if (_touchArea == null || _background == null || _lever == null)
            {
                Debug.LogError($"[{nameof(VirtualJoystickController)}] RectTransform 참조가 비어 있다.", this);
                return null;
            }

            object[] rest = base.Initialize(args);
            if (rest == null) return null;

            if (!TryTakeArgument(rest, out Canvas canvas))
            {
                Debug.LogError($"[{nameof(VirtualJoystickController)}] {nameof(Canvas)} 인자가 없다.", this);
                return null;
            }

            _canvas = canvas;
            // ScreenSpaceOverlay 는 worldCamera 가 null 이어야 좌표 변환이 맞는다. 여기서 틀리면 레버가 어긋난다.
            _canvasCamera = _canvas.renderMode == RenderMode.ScreenSpaceCamera ? _canvas.worldCamera : null;

            var center = new Vector2(0.5f, 0.5f);
            _background.pivot = center;
            _lever.pivot = center;
            _lever.anchorMin = center;
            _lever.anchorMax = center;
            _backgroundHomePosition = _background.anchoredPosition;

            ResetLever();
            SetRunning(true);
            return rest;
        }

        public override void SetRunning(bool isRunning)
        {
            base.SetRunning(isRunning);

            if (_touchArea.TryGetComponent(out Image raycastTarget))
            {
                raycastTarget.raycastTarget = isRunning;
            }

            if (!isRunning) ResetLever();
        }

        public override void Release()
        {
            if (IsInitialized) ResetLever();
            base.Release();
        }

        #endregion Public Methods

        #region Event Handlers

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!IsRunning) return;

            _background.gameObject.SetActive(true);
            _background.localPosition = ScreenToTouchAreaPoint(eventData.position);
            _isDragging = true;

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsRunning || !_isDragging)
            {
                ResetLever();
                return;
            }

            Vector2 origin = RectTransformUtility.WorldToScreenPoint(_canvasCamera, _background.position);
            Vector2 radius = _background.sizeDelta * 0.5f;
            Vector2 raw = (eventData.position - origin) / (radius * _canvas.scaleFactor);

            Direction = ApplyDeadZone(raw);
            _lever.anchoredPosition = Direction * radius * _leverRange;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // 멈춘 동안 손을 떼도 레버는 놓는다. 여기서 막으면 재개 직후 마지막 방향으로 저절로 움직인다.
            if (!_isDragging) return;

            ResetLever();
        }

        #endregion Event Handlers

        #region Help Methods

        private Vector2 ApplyDeadZone(Vector2 raw)
        {
            if (raw.magnitude <= _deadZone) return Vector2.zero;

            return raw.magnitude > 1f ? raw.normalized : raw;
        }

        private void ResetLever()
        {
            Direction = Vector2.zero;
            _isDragging = false;
            _lever.anchoredPosition = Vector2.zero;
            _background.anchoredPosition = _backgroundHomePosition;
            _background.gameObject.SetActive(_isBackgroundAlwaysVisible);
        }

        // 배경은 터치 영역의 바로 아래 자식이다. 터치 영역의 로컬 좌표를 그대로 쓰면 앵커와 무관하게 맞는다.
        // (sizeDelta 로 계산하면 화면 전체로 늘린 터치 영역에서 크기가 0 이 되어 배경이 엉뚱한 곳에 생긴다.)
        private Vector2 ScreenToTouchAreaPoint(Vector2 screenPosition)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _touchArea, screenPosition, _canvasCamera, out Vector2 localPoint)
                ? localPoint
                : Vector2.zero;
        }

        #endregion Help Methods
    }
}
