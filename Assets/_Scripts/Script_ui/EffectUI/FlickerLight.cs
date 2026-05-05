using System.Collections;
using UnityEngine;

public class FlickerLight : MonoBehaviour
{
    public Light targetLight;

    [Header("Flicker Setting")]
    public float normalIntensity = 100f;
    public float offIntensity = 0f;
    public float flickerInterval = 0.12f;
    public float pauseTime = 2f;

    void Start()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light>();

        StartCoroutine(FlickerRoutine());
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            // ±ôºý±ôºý 3¹ø
            for (int i = 0; i < 3; i++)
            {
                targetLight.intensity = offIntensity;
                yield return new WaitForSeconds(flickerInterval);

                targetLight.intensity = normalIntensity;
                yield return new WaitForSeconds(flickerInterval);
            }

            // Àá±ñ Á¤»ó À¯Áö
            yield return new WaitForSeconds(pauseTime);
        }
    }
}