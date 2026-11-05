namespace Core
{
    /// <summary>
    /// AppInstance가 부팅 순서대로 호출하는 초기화 계약.
    /// 생성(Awake)과 초기화(Start)를 분리해 참조 수집이 끝난 뒤에만 초기화가 돌게 한다.
    /// </summary>
    public interface IInitializable
    {
        bool IsInitialized { get; }

        void Initialize();
    }
}
