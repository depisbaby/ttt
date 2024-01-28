using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [SerializeField] GameObject aliveVersion;
    [SerializeField] List<MeshRenderer> teamMaterials;
    [SerializeField] Animator animator;

    private void Start()
    {
        
    }


    public void SetWalking(bool walk)
    {
        animator.SetBool("walking", walk);
    }

    public void SetTeamColot(Color color)
    {
        foreach (var c in teamMaterials)
        {
            c.material.SetColor("_BaseColor", color);
        }
    }



    public void HideForOwner()
    {
        aliveVersion.SetActive(false);
    }

    public void Kill(Vector3 dieDirection)
    {
        aliveVersion.SetActive(false);
    }

    public void Revive()
    {
        aliveVersion.SetActive(true);
    }
}
