using UnityEngine;

public class EndingManager : MonoBehaviour
{
    public EndingType endingType;

    public GameObject chaserWin;
    public GameObject chaserLose;
    public GameObject intruderWin;
    public GameObject intruderLose;

    void Start()
    {
        ShowEnding();
    }

    void ShowEnding()
    {
        // 전부 끄기
        chaserWin.SetActive(false);
        chaserLose.SetActive(false);
        intruderWin.SetActive(false);
        intruderLose.SetActive(false);

        // 하나만 켜기
        switch (endingType)
        {
            case EndingType.ChaserWin:
                chaserWin.SetActive(true);
                break;

            case EndingType.ChaserLose:
                chaserLose.SetActive(true);
                break;

            case EndingType.IntruderWin:
                intruderWin.SetActive(true);
                break;

            case EndingType.IntruderLose:
                intruderLose.SetActive(true);
                break;
        }
    }
}