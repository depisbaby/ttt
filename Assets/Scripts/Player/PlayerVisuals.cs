using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerVisuals : NetworkBehaviour
{
    public NetworkVariable<Quaternion> cameraRotation = new NetworkVariable<Quaternion>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Quaternion> orientation = new NetworkVariable<Quaternion>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField] GameObject visuals;
    [SerializeField] Animator animator;
    [SerializeField] Transform Tr_cameraRotation;
    [SerializeField] Transform Tr_orientation;

    //Two Bone
    [Header("Two Bone")]
    [SerializeField] TwoBoneIKConstraint rightHandTwoBoneIKConstraint;
    [SerializeField] TwoBoneIKConstraint leftHandTwoBoneIKConstraint;
    [SerializeField] Transform rightHandTip;
    [SerializeField] Transform leftHandTip;
    [SerializeField] Transform rightHandElbow;
    [SerializeField] Transform leftHandElbow;

    [Header("Predetermined hand transforms")]
    [SerializeField] Transform Ak_RightHandTip;
    [SerializeField] Transform Ak_LeftHandTip;
    [SerializeField] Transform Ak_RightHandElbow;
    [SerializeField] Transform Ak_LeftHandElbow;

    //Guns
    [Header("Gun models")]
    [SerializeField] GameObject mpxFull;


    private void Start()
    {
        EquipNothing();
    }

    private void Update()
    {
        Tr_orientation.rotation = orientation.Value;
        Tr_cameraRotation.rotation = cameraRotation.Value;
    }

    #region Animations

    public void SetWalking(bool walk)
    {
        animator.SetBool("walking", walk);
    }
    #endregion

    #region Update aiming

    

    #endregion

    #region Change appearance

    [ServerRpc] public void ChangeAppearanceServerRpc(FixedString128Bytes changeName)
    {
        ChangeAppearanceClientRpc(changeName);
    }
    [ClientRpc] public void ChangeAppearanceClientRpc(FixedString128Bytes changeName)
    {
        string _changeName = changeName.ToString();
        switch (_changeName)
        {

            case "A_mpx":
                AimMPX();
                break;

            case "mpx":
                EquipMPX();
                break;

            case "default":
                DefaultPose();
                break;

            default:
                EquipNothing();
                break;
        }
    }

    void DefaultPose()
    {
        rightHandTwoBoneIKConstraint.weight = 0;
        leftHandTwoBoneIKConstraint.weight = 0;
    }

    void EquipNothing()
    {
        rightHandTwoBoneIKConstraint.weight = 0;
        leftHandTwoBoneIKConstraint.weight = 0;

        DisableGunModels();
    }

    void AimMPX()
    {
        rightHandTwoBoneIKConstraint.weight = 1;
        leftHandTwoBoneIKConstraint.weight = 1;

        rightHandTip.localPosition = Ak_RightHandTip.localPosition;
        rightHandTip.localRotation = Ak_RightHandTip.localRotation;

        leftHandTip.localPosition = Ak_LeftHandTip.localPosition;
        leftHandTip.localRotation = Ak_LeftHandTip.localRotation;

        rightHandElbow.localPosition = Ak_RightHandElbow.localPosition;
        leftHandElbow.localPosition = Ak_LeftHandElbow.localPosition;

    }

    void EquipMPX()
    {
        mpxFull.SetActive(true);
    }

    void DisableGunModels()
    {
        mpxFull.SetActive(false);
    }

    #endregion

    #region Other
    public void HideForOwner()
    {
        visuals.SetActive(false);
    }
    #endregion
}
