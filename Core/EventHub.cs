using System;

namespace Core
{
    /// <summary>
    /// 서로를 직접 참조하면 안 되는 모듈 사이의 신호만 모아 둔 허브.
    /// 여기 올리는 기준은 "구독자가 둘 이상이거나, 호출부와 수신부의 수명이 다른 신호"다.
    /// </summary>
    public sealed class EventHub
    {
        #region Events

        public event Action<bool> OnCursorVisibilityChanged;

        public event Action<bool, bool> OnPlayStateChanged;

        public event Action OnSceneUnloading;

        #endregion Events

        #region Public Methods

        public void RaiseCursorVisibilityChanged(bool isVisible)
        {
            OnCursorVisibilityChanged?.Invoke(isVisible);
        }

        public void RaisePlayStateChanged(bool isPlaying, bool isUpdateHeld)
        {
            OnPlayStateChanged?.Invoke(isPlaying, isUpdateHeld);
        }

        public void RaiseSceneUnloading()
        {
            OnSceneUnloading?.Invoke();
        }

        public void Clear()
        {
            OnCursorVisibilityChanged = null;
            OnPlayStateChanged = null;
            OnSceneUnloading = null;
        }

        #endregion Public Methods
    }
}
