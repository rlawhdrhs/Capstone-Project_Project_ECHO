using UnityEngine;

public class SoundSensor : MonoBehaviour
{
    private SoundListener listener;

    public int sensorId;
    public SimpleMovement movement;
    public Transform cameraPoint;

    public LaserDetector laserDetector;
    public RemovalTester removalTester;

    public MonoBehaviour[] chaserOnlyComponents;



    private void Awake()
    {
        listener = GetComponent<SoundListener>();

        if (movement == null)
            movement = GetComponent<SimpleMovement>();
    }

    private void Start()
    {
        if (SensorManager.Instance != null)
            SensorManager.Instance.RegisterSensor(this);

        SetControlled(false);
    }

    private void OnEnable()
    {
        if (listener != null)
            listener.OnSoundDetected += HandleSoundDetected;
    }

    private void OnDisable()
    {
        if (listener != null)
            listener.OnSoundDetected -= HandleSoundDetected;
    }

    private void HandleSoundDetected(SoundData sound, float intensity)
    {
        if (SensorManager.Instance != null)
            SensorManager.Instance.ReportDetection(this, sound, intensity);
    }

    public void SetControlled(bool value)
    {
        if (movement != null)
            movement.enabled = value;

        if (laserDetector != null)
            laserDetector.SetLaserActiveByControl(value);

        if (removalTester != null)
            removalTester.enabled = value;
    }
}