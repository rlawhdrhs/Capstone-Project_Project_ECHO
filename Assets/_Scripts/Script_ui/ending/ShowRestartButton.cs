using UnityEngine;

public class ShowRestartButton : MonoBehaviour
{
    public GameObject restartButton;
    public float delay = 5f;

    void Start()
    {
        restartButton.SetActive(false); // Ã³À½¿£ ¼û±è
        Invoke("ShowButton", delay);
    }

    void ShowButton()
    {
        restartButton.SetActive(true);
    }
}