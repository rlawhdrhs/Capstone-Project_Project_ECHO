using System.Collections;
using UnityEngine;

public class SequentialActivator : MonoBehaviour
{
    [Header("시작 대기 시간")]
    public float startDelay = 5f;

    [Header("활성화 간격")]
    public float interval = 1f;

    void Start()
    {
        StartCoroutine(ActivateChildren());
    }

    IEnumerator ActivateChildren()
    {
        // 시작 전 대기
        yield return new WaitForSeconds(startDelay);

        // 처음에는 전부 비활성화
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        // 하나씩 활성화
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);

            yield return new WaitForSeconds(interval);
        }
    }
}