using Fusion;
using UnityEngine;

public class SensorSynchronizer : NetworkBehaviour
{
    public Transform droneBody;

    [Header("드론 동기화 보정")]
    public Vector3 centerPositionOffset;

    [Header("이동 제한 구역 설정")]
    public GameObject localBoundaryWall;

    [Header("빙의 불빛 설정")]
    public GameObject droneLightObject;
    [Networked] public NetworkBool netIsLightOn { get; set; }

    [Header("전기 이펙트 프리패브 (여기에 연결)")]
    public GameObject electricShockwavePrefab;

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            if (droneBody != null)
            {
                Vector3 headForward = data.headRotation * Vector3.forward;
                headForward.y = 0f;

                if (headForward.sqrMagnitude > 0.01f)
                {
                    droneBody.rotation = Quaternion.LookRotation(headForward);
                }
            }

            Vector3 alignedPosition = new Vector3(data.headPosition.x, data.rootPosition.y, data.headPosition.z);
            transform.position = alignedPosition;

            transform.position += transform.TransformDirection(centerPositionOffset);
        }
    }

    public override void Render()
    {
        base.Render();
        if (droneLightObject != null)
        {
            droneLightObject.SetActive(netIsLightOn);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_PlayShockwaveVFX_Global(Vector3 spawnPosition, float radius)
    {
        // 추격자 본인은 이미 로컬에서 생성했으므로 중복 생성 방지
        if (Object.HasInputAuthority) return;

        if (electricShockwavePrefab == null) return;

        // 잠입자 화면에서 이펙트 생성
        GameObject fxObj = Instantiate(electricShockwavePrefab, spawnPosition, Quaternion.identity);
        float diameter = radius * 2f;
        fxObj.transform.localScale = new Vector3(diameter, diameter, diameter);

        ParticleSystem[] subParticles = fxObj.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in subParticles)
        {
            ps.Clear();
            ps.Play();
        }
        Destroy(fxObj, 3.0f);
    }
}