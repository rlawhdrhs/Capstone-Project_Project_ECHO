using System.Collections;
using UnityEngine;

public class SplitDoorOpen : MonoBehaviour
{
    [Header("Door Parts")]
    public Transform upperDoor;
    public Transform lowerDoor;

    [Header("Move Settings")]
    public float moveDistance = 2f;
    public float moveDuration = 2f;

    private Vector3 upperStartPos;
    private Vector3 lowerStartPos;

    private Vector3 upperTargetPos;
    private Vector3 lowerTargetPos;

    void Start()
    {
        upperStartPos = upperDoor.position;
        lowerStartPos = lowerDoor.position;

        // 위 문은 위로 +2
        upperTargetPos = new Vector3(
            upperStartPos.x,
            upperStartPos.y + moveDistance,
            upperStartPos.z
        );

        // 아래 문은 아래로 -2
        lowerTargetPos = new Vector3(
            lowerStartPos.x,
            lowerStartPos.y - moveDistance,
            lowerStartPos.z
        );

        StartCoroutine(OpenDoor());
    }

    IEnumerator OpenDoor()
    {
        float time = 0f;

        while (time < moveDuration)
        {
            time += Time.deltaTime;

            float t = time / moveDuration;

            upperDoor.position =
                Vector3.Lerp(
                    upperStartPos,
                    upperTargetPos,
                    t
                );

            lowerDoor.position =
                Vector3.Lerp(
                    lowerStartPos,
                    lowerTargetPos,
                    t
                );

            yield return null;
        }

        upperDoor.position = upperTargetPos;
        lowerDoor.position = lowerTargetPos;
    }
}