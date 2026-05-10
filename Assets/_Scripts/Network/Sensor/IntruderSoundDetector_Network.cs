using UnityEngine;

public class IntruderSoundDetector_Network : MonoBehaviour
{
    [Header("센서 감지 범위")]
    public float detectRange = 15f;

    [Header("UI 큐브 제어")]
    public CubeColorChanger targetCube;

    void Update()
    {
        // 매 프레임마다 소리 감지 여부를 확인합니다.
        bool isDetected = CheckForSoundsInManager();

        // 감지 여부에 따라 UI 큐브 색상을 변경합니다.
        UpdateCubeColor(isDetected);
    }

    private bool CheckForSoundsInManager()
    {
        // SoundManager가 아직 씬에 없다면 false 반환
        if (SoundManager.Instance == null) return false;

        // SoundManager가 관리 중인 현재 발생한 모든 소리 목록을 검사합니다.
        foreach (var soundEvent in SoundManager.Instance.soundEvents)
        {
            // 센서(로봇) 위치와 소리가 발생한 위치 사이의 거리 계산
            float distance = Vector3.Distance(transform.position, soundEvent.position);

            // 소리가 센서의 감지 범위 안에서 발생했다면
            // (참고: soundEvent.detectionRadius를 활용하고 싶다면 distance <= soundEvent.detectionRadius 로 변경해도 됩니다)
            if (distance <= detectRange)
            {
                // 콘솔창에서 어떤 소리를 감지했는지 확인용
                Debug.Log($"[센서 감지] 소리 종류: {soundEvent.soundType}, 위치: {soundEvent.position}");

                // 하나라도 감지 범위 내에 있으면 즉시 true를 반환하고 루프를 종료합니다.
                return true;
            }
        }

        // 범위 내에 소리가 하나도 없으면 false 반환
        return false;
    }

    private void UpdateCubeColor(bool isDetected)
    {
        if (targetCube != null)
        {
            if (isDetected)
            {
                targetCube.ChangeToDetectedColor();
            }
            else
            {
                targetCube.ChangeToNormalColor();
            }
        }
    }

    // 에디터에서 센서의 감지 범위를 시각적으로 확인하기 위한 기즈모
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}