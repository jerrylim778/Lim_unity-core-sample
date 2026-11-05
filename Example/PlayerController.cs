using UnityEngine;
using Core;
using Core.Controllers;
using Core.UI;

namespace Example
{
    /// <summary>
    /// 줄기 전체를 한 화면에서 보여 주는 예시.
    /// 상위 컨트롤러가 하위(조이스틱)를 만들고, 초기화하고, 일시정지시키고, 해제한다.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : ControllerBase, IInitializable
    {
        #region Variables

        [SerializeField] private Canvas _uiCanvas;
        [SerializeField] private PopupStack _popupStack;
        [SerializeField] private float _moveSpeed = 5f;

        private VirtualJoystickController _joystick;
        private CharacterController _body;

        #endregion Variables

        #region Properties

        public bool IsInitialized { get; private set; }

        #endregion Properties

        #region Unity Methods

        private void OnEnable()
        {
            AppInstance app = AppInstance.Instance;
            if (app == null) return;

            app.Events.OnPlayStateChanged += OnPlayStateChanged;
        }

        private void OnDisable()
        {
            // 구독과 해제는 항상 같은 쌍으로 둔다. 씬 전환 때 죽은 객체가 호출되는 사고가 여기서 막힌다.
            // 앱 종료 시에는 AppInstance 가 먼저 사라질 수 있다.
            AppInstance app = AppInstance.Instance;
            if (app == null) return;

            app.Events.OnPlayStateChanged -= OnPlayStateChanged;
        }

        private void Update()
        {
            if (!IsRunning) return;

            Vector2 input = _joystick.Direction;
            if (input == Vector2.zero) return;

            var move = new Vector3(input.x, 0f, input.y);
            _body.Move(move * (_moveSpeed * Time.deltaTime));
        }

        #endregion Unity Methods

        #region Public Methods

        // AppInstance 의 부팅 배열에 이 컴포넌트를 넣으면 부팅 순서대로 여기가 호출된다.
        void IInitializable.Initialize() => Initialize();

        public override object[] Initialize(params object[] args)
        {
            _body = GetComponent<CharacterController>();

            _joystick = GetOrCreateSubController<VirtualJoystickController>();
            object[] rest = _joystick.Initialize(this, _uiCanvas);
            if (rest == null) return null;

            IsInitialized = true;
            SetRunning(true);
            return rest;
        }

        public override void Finish()
        {
            _joystick.Release();
            base.Finish();
        }

        // 멈추는 주체는 팝업이 아니라 이쪽이다. 팝업은 "닫아 달라"고만 말한다.
        public void OpenPause()
        {
            SetRunning(false);

            PausePopup popup = _popupStack.Open<PausePopup>();
            popup.BindCloseRequest(ClosePause);
            _popupStack.Show<PausePopup>();
        }

        #endregion Public Methods

        #region Event Handlers

        private void ClosePause()
        {
            _popupStack.Close<PausePopup>(() => SetRunning(true));
        }

        private void OnPlayStateChanged(bool isPlaying, bool isUpdateHeld)
        {
            if (!IsInitialized) return;

            SetRunning(isPlaying);
            _joystick.SetRunning(isPlaying && !isUpdateHeld);
        }

        #endregion Event Handlers
    }
}
