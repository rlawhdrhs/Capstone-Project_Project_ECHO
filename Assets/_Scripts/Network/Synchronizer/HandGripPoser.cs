using UnityEngine;

public class HandGripPoser : MonoBehaviour
{
    [System.Serializable]
    public struct BonePose
    {
        public Transform boneTransform; // 손가락 관절 오브젝트
        [HideInInspector] public Quaternion savedRotation; // ★ 런타임에 자동 저장될 회전값
    }

    [Header("손가락 관절 목록 (마디들을 다 넣어주세요)")]
    public BonePose[] fingerBones;

    [Header("포즈 활성화")]
    public bool applyGrip = true;

    void Awake()
    {
        // ★ 핵심: 게임이 켜지자마자, 애니메이터가 손을 펴기 전 타이밍에
        // 유저분이 장인정신으로 구부려놓은 프리팹 고유의 손 모양을 자동으로 기억(캐시)합니다.
        if (fingerBones != null)
        {
            for (int i = 0; i < fingerBones.Length; i++)
            {
                if (fingerBones[i].boneTransform != null)
                {
                    fingerBones[i].savedRotation = fingerBones[i].boneTransform.localRotation;
                }
            }
            Debug.Log("<color=lime>👍 프리팹에 구부려진 손가락 각도를 자동으로 런타임 메모리에 로드했습니다.</color>");
        }
    }

    void LateUpdate()
    {
        // 유니티 애니메이터가 손가락을 펴려고 방해해도, 
        // Awake 때 기억해둔 이쁜 손 모양 각도로 매 프레임 강제 고정합니다.
        if (applyGrip && fingerBones != null)
        {
            for (int i = 0; i < fingerBones.Length; i++)
            {
                if (fingerBones[i].boneTransform != null)
                {
                    fingerBones[i].boneTransform.localRotation = fingerBones[i].savedRotation;
                }
            }
        }
    }
}