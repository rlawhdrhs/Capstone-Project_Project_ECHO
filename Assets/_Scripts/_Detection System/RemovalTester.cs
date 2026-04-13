using UnityEngine;
using Fusion;

public class RemovalTester : NetworkBehaviour
{
    private PlayerDetectable targetPlayer;
    private bool _spacePressed;

    void Update()
    {
        if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.Space))
        {
            _spacePressed = true;
        }
    }
    public override void FixedUpdateNetwork()
    {
        if (Object == null || !Object.IsValid) return;

        if (!Object.HasInputAuthority) return;

        if (targetPlayer == null)
        {
            if (NetworkManager.Instance.InfiltratorObject != null)
            {
                targetPlayer = NetworkManager.Instance.InfiltratorObject.GetComponent<PlayerDetectable>();
            }
            else
            {
                return;
            }
        }

        if (_spacePressed)
        {
            _spacePressed = false;
            if (targetPlayer != null)
            {
                targetPlayer.RequestRemove();
            }
        }
    }
}