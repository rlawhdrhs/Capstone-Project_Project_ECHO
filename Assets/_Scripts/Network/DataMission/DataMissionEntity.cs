using Fusion;
using UnityEngine;

public class DataMissionEntity : NetworkBehaviour
{
    private GameObject _myMinimapObject;

    public override void Spawned()
    {
        DataMissionSpawner spawner = FindAnyObjectByType<DataMissionSpawner>();
        if (spawner == null) return;

        foreach (Transform point in spawner.spawnPoints)
        {
            if (point != null && Vector3.Distance(transform.position, point.position) < 0.5f)
            {
                // [구조 파악] point(Spawn Point)의 부모가 '미션 부모 오브젝트'임
                Transform missionParent = point.parent;
                if (missionParent != null)
                {
                    Transform minimapTransform = missionParent.Find("GreenSphere");

                    if (minimapTransform == null)
                    {
                        foreach (Transform child in missionParent)
                        {
                            if (child != point && (child.name.Contains("GreenSphere")))
                            {
                                minimapTransform = child;
                                break;
                            }
                        }
                    }

                    // 찾은 미니맵 구체를 활성화합니다.
                    if (minimapTransform != null)
                    {
                        _myMinimapObject = minimapTransform.gameObject;
                        _myMinimapObject.SetActive(true);
                        Debug.Log($"<color=lime>[미니맵 동기화] {missionParent.name} 위치의 미니맵 표시 활성화!</color>");
                    }
                }
                break;
            }
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasStateAuthority)
    {
        // 미션이 완료되어 네트워크상에서 사라질 때(Despawn), 켜두었던 미니맵 구체를 다시 꺼줍니다.
        if (_myMinimapObject != null)
        {
            _myMinimapObject.SetActive(false);
        }
    }
}