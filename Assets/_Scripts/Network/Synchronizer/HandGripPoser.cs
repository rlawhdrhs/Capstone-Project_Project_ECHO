using UnityEngine;

public class HandGripPoser : MonoBehaviour
{
    [System.Serializable]
    public struct BonePose
    {
        public Transform boneTransform; // 손가락 관절 오브젝트
        [HideInInspector] public Quaternion localRotation; // 저장될 회전값
    }

    [Header("손가락 관절 목록 (마디들을 다 넣어주세요)")]
    public BonePose[] fingerBones;

    [Header("포즈 활성화")]
    public bool applyGrip = true;

    // ★ 인스펙터 컴포넌트 이름을 우클릭하면 나오는 대박 편리한 기능입니다.
    [ContextMenu("현재 손 모양을 그랩 포즈로 저장")]
    public void SaveCurrentPose()
    {
        if (fingerBones == null || fingerBones.Length == 0)
        {
            Debug.Log("fingerBones 배열에 손가락 관절 오브젝트들을 먼저 등록해주세요!");
            return;
        }

        for (int i = 0; i < fingerBones.Length; i++)
        {
            if (fingerBones[i].boneTransform != null)
            {
                // 현재 에디터 씬 뷰에서 구부려놓은 각도를 그대로 저장합니다.
                fingerBones[i].localRotation = fingerBones[i].boneTransform.localRotation;
            }
        }
        Debug.Log("<color=lime>👍 현재 손가락 관절 각도가 스크립트에 성공적으로 박제되었습니다!</color>");
    }

    void LateUpdate()
    {
        // 일반 애니메이션이 손가락을 마음대로 움직이지 못하게 
        // 유니티 렌더링 직전(LateUpdate)에 우리가 구운 각도로 강제 고정합니다.
        if (applyGrip && fingerBones != null)
        {
            for (int i = 0; i < fingerBones.Length; i++)
            {
                if (fingerBones[i].boneTransform != null)
                {
                    fingerBones[i].boneTransform.localRotation = fingerBones[i].localRotation;
                }
            }
        }
    }
}