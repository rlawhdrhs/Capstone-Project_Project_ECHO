using UnityEngine;

public class BoxClickSubtitle : MonoBehaviour
{
    [Header("Subtitle UI")]
    public GameObject subtitlePanel;

    private bool isOpen = false;

    void Start()
    {
        if (subtitlePanel != null)
            subtitlePanel.SetActive(false);
    }

    void OnMouseDown()
    {
        if (subtitlePanel == null) return;

        isOpen = !isOpen;
        subtitlePanel.SetActive(isOpen);
    }
}