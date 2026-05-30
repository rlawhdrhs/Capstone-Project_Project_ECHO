using Fusion;
using UnityEngine;

public class SoundEventNetworkManager : NetworkBehaviour
{
    public static SoundEventNetworkManager Instance;

    [Header("사운드 세팅")]
    [Tooltip("Meta XR Audio Source가 붙어있는 빈 스피커 프리팹")]
    public GameObject soundEmitterPrefab;

    [Tooltip("사운드 리스트")]
    public AudioClip[] audioClips;

    public override void Spawned()
    {
        if (Instance == null) Instance = this;
    }

    public void SendSoundEvent(Vector3 position, float intensity, int priority, int clipIndex)
    {
        Rpc_PlaySpatialSound(position, intensity, priority, clipIndex);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void Rpc_PlaySpatialSound(Vector3 position, float intensity, int priority, int clipIndex)
    {
        // 1. 우선순위(Priority) 로직 
        // if (priority < 0) return; 

        // 2. 스피커 프리팹 소환
        GameObject emitter = Instantiate(soundEmitterPrefab, position, Quaternion.identity);
        AudioSource audioSource = emitter.GetComponent<AudioSource>();

        // 3. 전달받은 데이터 세팅
        audioSource.clip = audioClips[clipIndex];
        audioSource.volume = intensity; // 강도를 볼륨에 적용

        // 4. 소리 재생 및 자동 파괴
        audioSource.Play();

        // 클립 길이 + 스피커 오브젝트 파괴
        Destroy(emitter, audioSource.clip.length + 0.1f);
    }
}