using System;

namespace Core.UI
{
    /// <summary>
    /// 여는 연출과 닫는 연출의 "끝"을 스택에 알려주는 계약.
    /// 콜백 없이 즉시 반환하면 스택이 연출 도중에 다음 팝업을 올려버린다.
    /// </summary>
    public interface IPopupLifecycle
    {
        void Show(Action onComplete = null);

        void Hide(Action onComplete = null);
    }
}
