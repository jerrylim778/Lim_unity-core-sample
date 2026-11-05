namespace Core.Web
{
    /// <summary>
    /// 전송 직전 본문을 가공하는 자리. 구현(암호화 방식)은 프로젝트마다 다르므로 밖에서 주입한다.
    /// </summary>
    public interface IPayloadCodec
    {
        string Encode(string plain);

        string Decode(string encoded);
    }
}
