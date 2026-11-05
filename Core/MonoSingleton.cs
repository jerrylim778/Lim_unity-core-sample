using UnityEngine;

namespace Core
{
    /// <summary>
    /// 씬에 하나만 존재하는 MonoBehaviour의 공통 진입점.
    /// 중복 인스턴스는 자기 자신을 파괴하고, 최초 1개만 소유권(IsOwner)을 갖는다.
    /// </summary>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        #region Variables

        [SerializeField] private bool _isPersistent;

        private static T s_instance;
        private bool _isOwner;

        #endregion Variables

        #region Properties

        public static T Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = FindFirstObjectByType<T>();
                }

                return s_instance;
            }
        }

        protected bool IsOwner => _isOwner;

        #endregion Properties

        #region Unity Methods

        protected virtual void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_instance = this as T;
            _isOwner = true;

            if (_isPersistent)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (s_instance == this)
            {
                s_instance = null;
            }
        }

        #endregion Unity Methods
    }
}
