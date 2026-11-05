using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Core.Web
{
    /// <summary>
    /// WebGL 빌드에서 호스트 페이지(JS)와 주고받는 유일한 창구.
    /// 에디터에서는 extern 호출이 불가능하므로, 같은 함수가 테스트 응답으로 대체 동작한다.
    /// 호스트 측 실제 함수명은 플랫폼 계약이라 Host_ 접두사로 가렸다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WebGLBridge : MonoBehaviour, IInitializable
    {
        #region Types

        [Serializable]
        public struct ScoreReport
        {
            public int Score;
            public int Point;
            public int PlayTimeSeconds;
            public string[] UsedItemCodes;
        }

        [Serializable]
        private struct StubResponse
        {
            public string FunctionName;
            public bool IsSuccess;
        }

        private sealed class PassThroughCodec : IPayloadCodec
        {
            public string Encode(string plain) => plain;

            public string Decode(string encoded) => encoded;
        }

        #endregion Types

        #region Variables

        [SerializeField] private bool _isWebBuildTarget;
        [SerializeField] private StubResponse[] _stubResponses;

        private IPayloadCodec _codec;

        #endregion Variables

        #region Properties

        public bool IsInitialized { get; private set; }

        public bool IsSessionDataReady { get; private set; }

        public string SessionData { get; private set; }

        // 에디터에서는 extern 심볼이 없어 즉시 크래시한다. 이 조건이 모든 호출의 전제다.
        private bool CanCallHost => !Application.isEditor && _isWebBuildTarget;

        #endregion Properties

        #region Public Methods

        public void Initialize()
        {
            _codec ??= new PassThroughCodec();
            IsInitialized = true;
        }

        public void SetCodec(IPayloadCodec codec)
        {
            _codec = codec ?? new PassThroughCodec();
        }

        public void SendScore(ScoreReport report, string gameData)
        {
            if (!CanCallHost) return;

            Host_RecordScore(
                report.Score,
                report.Point,
                report.PlayTimeSeconds,
                _codec.Encode(gameData),
                string.Join(",", report.UsedItemCodes ?? Array.Empty<string>()));
        }

        public void ReturnToHost()
        {
            if (!CanCallHost) return;

            Host_ReturnLanding();
        }

        // JS 가 UnityInstance.SendMessage 로 직접 호출하는 지점. 이름을 바꾸면 호스트 쪽도 같이 바뀌어야 한다.
        public void ReceiveSessionData(string encodedJson)
        {
            if (string.IsNullOrEmpty(encodedJson))
            {
                Debug.LogError($"[{nameof(WebGLBridge)}] 호스트가 빈 세션 데이터를 보냈다.", this);
                return;
            }

            SessionData = _codec.Decode(encodedJson);
            IsSessionDataReady = true;
        }

        // 요청과 응답이 프레임을 건너뛰기 때문에, 호출부는 결과를 기다릴 수단이 필요하다.
        public IEnumerator WaitForResult(Action request, Func<bool> isDone, Action onComplete = null)
        {
            request?.Invoke();
            yield return new WaitUntil(isDone);

            onComplete?.Invoke();
        }

        public bool GetStubResult(string functionName)
        {
            foreach (StubResponse response in _stubResponses)
            {
                if (response.FunctionName != functionName) continue;

                return response.IsSuccess;
            }

            return false;
        }

        #endregion Public Methods

        #region Host Imports

        [DllImport("__Internal")]
        private static extern void Host_RecordScore(
            int score, int point, int playTime, string gameData, string usedItemCodes);

        [DllImport("__Internal")]
        private static extern void Host_ReturnLanding();

        #endregion Host Imports
    }
}
