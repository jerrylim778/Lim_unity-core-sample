namespace Core.UI
{
    /// <summary>
    /// 뒤쪽을 가리는 딤 패널의 투명도를 팝업이 직접 정하고 싶을 때만 구현한다.
    /// </summary>
    public interface IDimProvider
    {
        float DimAlpha { get; }
    }
}
