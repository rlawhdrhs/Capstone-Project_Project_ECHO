using Fusion;
using UnityEngine;

public class IntruderTensionSound : NetworkBehaviour
{
    [Header("사운드 설정")]
    [Tooltip("잠입자 본인에게만 재생할 긴장감 루프 사운드 에셋")]
    public AudioClip tensionLoopClip;
    [Range(0f, 1f)] public float maxTensionVolume = 1f;
    [Tooltip("볼륨이 커지고 작아지는 변화 속도")]
    public float fadeSpeed = 2f;

    [Header("거리 설정 (미터 단위)")]
    public float maxDistance = 7f;
    public float minDistance = 3f;

    private AudioSource _audioSource;
    private bool _isLocalInfiltrator = false;
    private float _targetVolume = 0f;

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            _isLocalInfiltrator = true;

            if (tensionLoopClip != null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.clip = tensionLoopClip;
                _audioSource.loop = true;
                _audioSource.spatialBlend = 0f;
                _audioSource.playOnAwake = false;
                _audioSource.volume = 0f;
            }
        }
    }

    private void Update()
    {
        if (!_isLocalInfiltrator || Object == null) return;

        HandleDistanceAudio();
    }

    private void HandleDistanceAudio()
    {
        if (_audioSource == null || tensionLoopClip == null) return;

        SensorSynchronizer[] chasers = FindObjectsByType<SensorSynchronizer>(FindObjectsSortMode.None);

        float closestDistance = float.MaxValue;
        bool chaserFound = false;

        foreach (var chaser in chasers)
        {
            if (chaser != null)
            {
                if (chaser.Object.InputAuthority == PlayerRef.None)
                {
                    continue;
                }

                float dist = Vector3.Distance(transform.position, chaser.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    chaserFound = true;
                }
            }
        }

        // 범위 안에 추격자가 있을 때
        if (chaserFound && closestDistance <= maxDistance)
        {
            // 소리가 꺼져있었다면 루프 재생 시작
            if (!_audioSource.isPlaying)
            {
                _audioSource.Play();
            }

            // 거리에 따른 비율 계산
            float distanceRatio = (closestDistance - minDistance) / (maxDistance - minDistance);
            float lerpRatio = Mathf.Clamp01(1f - distanceRatio);

            _targetVolume = lerpRatio * maxTensionVolume;
        }
        else
        {
            _targetVolume = 0f;

            if (_audioSource.isPlaying && _audioSource.volume <= 0.01f)
            {
                _audioSource.Stop();
            }
        }

        _audioSource.volume = Mathf.MoveTowards(_audioSource.volume, _targetVolume, Time.deltaTime * fadeSpeed);
    }

    private void OnDestroy()
    {
        // 오브젝트가 파괴되거나 씬이 바뀔 때 안전하게 정지
        if (_audioSource != null)
        {
            _audioSource.Stop();
        }
    }
}