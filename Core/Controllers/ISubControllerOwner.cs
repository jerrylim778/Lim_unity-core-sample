namespace Core.Controllers
{
    /// <summary>
    /// 하위 컨트롤러를 소유하는 쪽의 계약.
    /// 소유자만 하위를 찾아 수명과 일시정지를 지시할 수 있다.
    /// </summary>
    public interface ISubControllerOwner
    {
        T GetSubController<T>() where T : SubControllerBase;

        bool TryGetSubController<T>(out T subController) where T : SubControllerBase;
    }
}
