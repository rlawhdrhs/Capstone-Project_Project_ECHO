using System;
using System.Collections;
using UnityEngine;

public class FindPulse : MonoBehaviour
{
    public enum PulseResult
    {
        NoSignal,
        Nearby,
        Close,
        VeryClose
    }

    [Header("Target")]
    [SerializeField] private Transform intruder;

    [Header("Pulse Range")]
    [SerializeField] private float maxRange = 20f;
    [SerializeField] private float closeRange = 10f;
    [SerializeField] private float veryCloseRange = 5f;

    [Header("Cooldown")]
    [SerializeField] private float cooldown = 10f;
    [SerializeField] private bool startReady = true;

    [Header("Input Test")]
    [SerializeField] private bool useKeyboardInput = true;
    [SerializeField] private KeyCode pulseKey = KeyCode.C;

    [Header("Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pulseUseClip;
    [SerializeField] private AudioClip noSignalClip;
    [SerializeField] private AudioClip nearbyClip;
    [SerializeField] private AudioClip closeClip;
    [SerializeField] private AudioClip veryCloseClip;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = true;

    public event Action<PulseResult, float> OnPulseUsed;
    public event Action<float> OnCooldownChanged;

    private bool canUsePulse;
    private float currentCooldown;

    private void Awake()
    {
        canUsePulse = startReady;

        if (!startReady)
        {
            currentCooldown = cooldown;
            StartCoroutine(CooldownRoutine());
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (!useKeyboardInput)
        {
            return;
        }

        if (Input.GetKeyDown(pulseKey))
        {
            UsePulse();
        }
    }

    public void UsePulse()
    {
        if (!canUsePulse)
        {
            if (showDebugLog)
            {
                Debug.Log($"[ProximityPulse] Pulse is on cooldown. Remaining: {currentCooldown:F1}s");
            }

            return;
        }

        if (intruder == null)
        {
            Debug.LogWarning("[ProximityPulse] Intruder Transform is missing.");
            return;
        }

        float distance = Vector3.Distance(transform.position, intruder.position);
        PulseResult result = GetPulseResult(distance);

        PlayPulseSound(result);

        if (showDebugLog)
        {
            Debug.Log($"[ProximityPulse] Result: {result} / Distance: {distance:F2}m");
        }

        OnPulseUsed?.Invoke(result, distance);

        canUsePulse = false;
        currentCooldown = cooldown;
        StartCoroutine(CooldownRoutine());
    }

    private PulseResult GetPulseResult(float distance)
    {
        if (distance > maxRange)
        {
            return PulseResult.NoSignal;
        }

        if (distance <= veryCloseRange)
        {
            return PulseResult.VeryClose;
        }

        if (distance <= closeRange)
        {
            return PulseResult.Close;
        }

        return PulseResult.Nearby;
    }

    private void PlayPulseSound(PulseResult result)
    {
        if (audioSource == null)
        {
            return;
        }

        if (pulseUseClip != null)
        {
            audioSource.PlayOneShot(pulseUseClip);
        }

        AudioClip resultClip = null;

        switch (result)
        {
            case PulseResult.NoSignal:
                resultClip = noSignalClip;
                break;

            case PulseResult.Nearby:
                resultClip = nearbyClip;
                break;

            case PulseResult.Close:
                resultClip = closeClip;
                break;

            case PulseResult.VeryClose:
                resultClip = veryCloseClip;
                break;
        }

        if (resultClip != null)
        {
            audioSource.PlayOneShot(resultClip);
        }
    }

    private IEnumerator CooldownRoutine()
    {
        while (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
            OnCooldownChanged?.Invoke(currentCooldown);

            yield return null;
        }

        currentCooldown = 0f;
        canUsePulse = true;
        OnCooldownChanged?.Invoke(currentCooldown);

        if (showDebugLog)
        {
            Debug.Log("[ProximityPulse] Pulse is ready.");
        }
    }

    public bool CanUsePulse()
    {
        return canUsePulse;
    }

    public float GetCurrentCooldown()
    {
        return currentCooldown;
    }

    public float GetCooldownRatio()
    {
        if (cooldown <= 0f)
        {
            return 0f;
        }

        return currentCooldown / cooldown;
    }
}