using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashBang : Throwable
{
    [SerializeField] LayerMask flashBlocking;
    public override void Activate()
    {
        foreach (var item in LobbyMenu.Instance.players)
        {
            if (!Physics.Linecast(transform.position, item.headTransform.position, flashBlocking))
            {
                item.FlashPlayerClientRpc();
            }
        }

        no.Despawn();
        Destroy(gameObject);
    }
}
