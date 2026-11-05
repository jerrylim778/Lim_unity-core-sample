using UnityEngine;

namespace Core.Controllers
{
    /// <summary>
    /// 상위 컨트롤러가 수명을 쥐는 하위 모듈.
    /// 적용(Initialize) · 일시정지(SetRunning) · 해제(Release)를 상위에서 지시받는다.
    /// 자기 상태가 켜져 있어도 상위가 멈춰 있으면 돌지 않는다 (IsRunning 이 상위를 따른다).
    /// </summary>
    public abstract class SubControllerBase : ControllerBase
    {
        #region Variables

        [SerializeField] private bool _isSelfUpdating;

        private ControllerBase _owner;

        #endregion Variables

        #region Properties

        public ControllerBase Owner => _owner;

        public bool IsInitialized { get; private set; }

        // 소유자 없이는 돌지 않고, 소유자가 멈추면 같이 멈춘다. 중첩된 하위도 위로 따라 올라가며 같은 규칙을 탄다.
        public override bool IsRunning => base.IsRunning && _owner != null && _owner.IsRunning;

        #endregion Properties

        #region Unity Methods

        protected virtual void Update()
        {
            if (!_isSelfUpdating || !IsRunning) return;

            ProcessUpdate(Time.deltaTime);
        }

        #endregion Unity Methods

        #region Public Methods

        // 첫 번째 ControllerBase 인자를 소유자로 삼고, 나머지는 그대로 돌려준다.
        public override object[] Initialize(params object[] args)
        {
            if (!TryTakeArgument(args, out ControllerBase owner))
            {
                Debug.LogError($"[{GetType().Name}] 소유자({nameof(ControllerBase)}) 없이 초기화할 수 없다.", this);
                return null;
            }

            _owner = owner;
            IsInitialized = true;
            return Exclude(args, owner);
        }

        public override void SetRunning(bool isRunning)
        {
            SetRunningInternal(isRunning);
        }

        public virtual void Release()
        {
            SetRunningInternal(false);
            IsInitialized = false;
            _owner = null;
        }

        public virtual void ProcessUpdate(float deltaTime)
        {
        }

        public T FindOwnerOfType<T>() where T : ControllerBase
        {
            ControllerBase current = _owner;

            while (current != null)
            {
                if (current is T match) return match;
                if (current is not SubControllerBase sub) break;

                current = sub.Owner;
            }

            return null;
        }

        #endregion Public Methods

        #region Help Methods

        private static object[] Exclude(object[] args, object removed)
        {
            var result = new object[args.Length - 1];
            int index = 0;

            foreach (object arg in args)
            {
                if (ReferenceEquals(arg, removed)) continue;

                result[index++] = arg;
            }

            return result;
        }

        #endregion Help Methods
    }
}
