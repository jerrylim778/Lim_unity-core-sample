using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 앱의 단일 진입점. 매니저를 "모으는 단계"와 "초기화하는 단계"를 분리한다.
    /// 부팅 순서는 코드가 아니라 _bootSequence 배열 순서가 결정한다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AppInstance : MonoSingleton<AppInstance>
    {
        #region Variables

        [SerializeField] private MonoBehaviour[] _bootSequence;

        private readonly List<IInitializable> _bootTargets = new();

        #endregion Variables

        #region Properties

        // Awake 보다 먼저 다른 오브젝트의 OnEnable 이 구독할 수 있으므로, 생성 시점에 이미 존재해야 한다.
        public EventHub Events { get; } = new();

        public bool IsBootCompleted { get; private set; }

        #endregion Properties

        #region Unity Methods

        protected override void Awake()
        {
            base.Awake();
            if (!IsOwner) return;

            CollectBootTargets();
        }

        private void Start()
        {
            if (!IsOwner) return;

            Boot();
        }

        #endregion Unity Methods

        #region Public Methods

        public T Find<T>() where T : class
        {
            foreach (IInitializable target in _bootTargets)
            {
                if (target is T match) return match;
            }

            return null;
        }

        #endregion Public Methods

        #region Help Methods

        private void CollectBootTargets()
        {
            _bootTargets.Clear();

            foreach (MonoBehaviour behaviour in _bootSequence)
            {
                if (behaviour == null) continue;

                if (behaviour is not IInitializable target)
                {
                    Debug.LogError($"[{nameof(AppInstance)}] {nameof(IInitializable)} 이 아닌 부팅 대상: {behaviour.GetType().Name}", behaviour);
                    continue;
                }

                _bootTargets.Add(target);
            }
        }

        private void Boot()
        {
            foreach (IInitializable target in _bootTargets)
            {
                if (target.IsInitialized) continue;

                target.Initialize();
            }

            IsBootCompleted = true;
        }

        #endregion Help Methods
    }
}
