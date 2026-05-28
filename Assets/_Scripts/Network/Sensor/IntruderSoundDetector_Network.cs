using System.Collections.Generic;
using UnityEngine;

public class IntruderSoundDetector_Network : MonoBehaviour
{
    public static List<IntruderSoundDetector_Network> Detectors = new List<IntruderSoundDetector_Network>();

    [Header("센서 감지 범위")]
    public float detectRange = 15f;

    [Header("UI 큐브 제어")]
    public CubeColorChanger targetCube;

    public bool IsSoundDetected { get; private set; }

    private void OnEnable()
    {
        Detectors.Add(this);
    }

    // 오브젝트가 파괴되거나 꺼질 때 리스트에서 자동 제거
    private void OnDisable()
    {
        Detectors.Remove(this);
    }

    void Update()
    {
        // 매 프레임마다 소리 감지 여부를 확인합니다.
        bool isDetected = CheckForSoundsInManager();

        IsSoundDetected = isDetected;

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

            if (distance <= detectRange)
            {
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