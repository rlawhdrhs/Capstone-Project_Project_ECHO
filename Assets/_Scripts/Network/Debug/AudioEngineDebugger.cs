using UnityEngine;

public class AudioDSPFixer : MonoBehaviour
{
    void Awake()
    {
        // 1. 현재 유니티의 오디오 설정을 그대로 가져옵니다.
        AudioConfiguration config = AudioSettings.GetConfiguration();

        // 2. 오디오 하드웨어 드라이버와 DSP(디지털 신호 처리) 그래프를 강제로 재부팅합니다.
        // 이 함수가 실행되면 꼬여있던 오디오 줄기가 메타 퀘스트 3 헤드셋과 다시 강제 연결됩니다.
        AudioSettings.Reset(config);

        Debug.Log("🔄 [오디오 픽서] 유니티 오디오 드라이버 및 DSP 그래프 강제 재부팅 완료!");
    }
}