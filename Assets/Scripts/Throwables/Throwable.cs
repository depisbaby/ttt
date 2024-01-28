using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Throwable : NetworkBehaviour
{
    public Rigidbody rb;
    [SerializeField] float fuseTime;
    [SerializeField] protected NetworkObject no;
    public void Throw(Vector3 position, Vector3 direction)
    {
        transform.position = position;
        rb.AddForce(direction.normalized * 2f, ForceMode.Impulse);
        rb.AddTorque(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)));

        Invoke("Activate", fuseTime);
    }

    public virtual void Activate()
    {

    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
