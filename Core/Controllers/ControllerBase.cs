using UnityEngine;

namespace Core.Controllers
{
    /// <summary>
    /// 모든 동작 모듈의 최상위 추상 클래스.
    /// 상위가 하위의 수명을 쥐는 Composite 구조를 만들기 위해, 생성/초기화/정지를 전부 여기서 규정한다.
    /// </summary>
    public abstract class ControllerBase : MonoBehaviour, ISubControllerOwner
    {
        #region Variables

        [SerializeField] protected bool _isTestMode;

        private bool _isRunning;

        #endregion Variables

        #region Properties

        public virtual bool IsRunning => _isRunning;

        public bool IsFinished { get; protected set; }

        public bool IsTestMode => _isTestMode;

        #endregion Properties

        #region Abstract Methods

        // 반환값 = 내가 쓰고 남은 인자. 하위 컨트롤러가 이어서 쓴다.
        public abstract object[] Initialize(params object[] args);

        #endregion Abstract Methods

        #region Public Methods

        public virtual void Finish()
        {
            if (this is SubControllerBase) return;

            IsFinished = true;
            SetRunning(false);
        }

        public T GetSubController<T>() where T : SubControllerBase
        {
            return TryGetSubController(out T subController) ? subController : null;
        }

        public bool TryGetSubController<T>(out T subController) where T : SubControllerBase
        {
            subController = GetComponentInChildren<T>(true);
            return subController != null;
        }

        public virtual void SetRunning(bool isRunning)
        {
            if (isRunning) IsFinished = false;

            _isRunning = isRunning;
        }

        #endregion Public Methods

        #region Help Methods

        protected void SetRunningInternal(bool isRunning)
        {
            _isRunning = isRunning;
        }

        protected static bool TryTakeArgument<T>(object[] args, out T value)
        {
            value = default;
            if (args == null) return false;

            foreach (object arg in args)
            {
                if (arg is not T match) continue;

                value = match;
                return true;
            }

            return false;
        }

        protected T GetOrCreateSubController<T>() where T : SubControllerBase
        {
            T existing = GetComponentInChildren<T>(true);
            if (existing != null) return existing;

            var created = new GameObject(typeof(T).Name).AddComponent<T>();
            created.transform.SetParent(transform, false);
            return created;
        }

        #endregion Help Methods
    }
}
