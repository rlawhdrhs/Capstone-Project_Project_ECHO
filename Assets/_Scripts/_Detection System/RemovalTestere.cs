using UnityEngine;

public class RemovalTester : MonoBehaviour
{
    public PlayerDetectable targetPlayer;

    void Update()
    {
        if (targetPlayer == null) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            targetPlayer.TryRemove();
        }
    }
}