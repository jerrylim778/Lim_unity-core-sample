namespace Core.UI
{
    /// <summary>
    /// 팝업이 쌓이는 층. 층이 다르면 부모 Transform 과 딤 처리 규칙이 다르다.
    /// </summary>
    public enum PopupLayer
    {
        Default = 0,
        Required = 1,
        Special = 2,
    }
}
