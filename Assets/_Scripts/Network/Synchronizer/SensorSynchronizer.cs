using Fusion;
using UnityEngine;

public class SensorSynchronizer : NetworkBehaviour
{
    public Transform droneBody;

    [Header("드론 동기화 보정")]
    public Vector3 centerPositionOffset;

    [Header("이동 제한 구역 설정")]
    public Collider movementZone;

    [Header("빙의 불빛 설정")]
    public GameObject droneLightObject;
    [Networked] public NetworkBool netIsLightOn { get; set; }

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
}