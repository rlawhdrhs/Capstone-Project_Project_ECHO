using Fusion;
using UnityEngine;

public class SensorSynchronizer : NetworkBehaviour
{
    public Transform droneBody; // 드론 본체
    public float speed = 3.0f;

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            if (droneBody != null)
            {
                droneBody.rotation = data.headRotation;
            }

            // 1. 내 시선(고개 방향)을 기준으로 앞뒤/좌우 벡터를 구함
            Vector3 forward = data.headRotation * Vector3.forward;
            Vector3 right = data.headRotation * Vector3.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            Vector3 moveDir = (forward * data.moveZ) + (right * data.moveX);

            transform.position += moveDir * speed * Runner.DeltaTime;
        }
    }
}