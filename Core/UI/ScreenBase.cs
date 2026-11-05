using UnityEngine;

namespace Core.UI
{
    /// <summary>
    /// 화면 단위의 공통 수명주기. 생성과 초기화를 분리하고, 갱신 여부를 소유자가 끈다.
    /// </summary>
    public abstract class ScreenBase : MonoBehaviour
    {
        #region Variables

        private bool _isUpdating;

        #endregion Variables

        #region Properties

        public bool IsUpdating => _isUpdating;

        #endregion Properties

        #region Abstract Methods

        public abstract void Initialize();

        #endregion Abstract Methods

        #region Public Methods

        public virtual void ProcessUpdate()
        {
        }

        public virtual void SetUpdating(bool isUpdating)
        {
            _isUpdating = isUpdating;
        }

        #endregion Public Methods
    }
}
