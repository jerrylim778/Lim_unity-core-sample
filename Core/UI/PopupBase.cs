using System;
using UnityEngine;

namespace Core.UI
{
    /// <summary>
    /// 스택에 쌓이는 화면. 여는 연출과 닫는 연출의 끝을 콜백으로 알려야 스택이 다음 단계로 간다.
    /// </summary>
    public abstract class PopupBase : ScreenBase, IPopupLifecycle
    {
        #region Variables

        [SerializeField] private PopupLayer _layer = PopupLayer.Default;

        #endregion Variables

        #region Properties

        public PopupLayer Layer => _layer;

        #endregion Properties

        #region Abstract Methods

        public abstract void Show(Action onComplete = null);

        public abstract void Hide(Action onComplete = null);

        #endregion Abstract Methods
    }
}
