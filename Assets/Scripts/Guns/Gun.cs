using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class Gun : MonoBehaviour
{
    // Start is called before the first frame update
    public int cyclingRate;
    public int ammoRemaining;
    bool cycling;
    public float recoilAmount;
    public float recoilRecovery;
    public LayerMask hittable;
    public int damage;
    public AnimationCurve climbX;

    float recoil;
    float sprayModifier;
    float sway;

    public void Update()
    {
        recoil = Mathf.Lerp(recoil,0f, Time.deltaTime * (recoilRecovery*sprayModifier));
        sprayModifier = Mathf.Clamp(sprayModifier + Time.deltaTime,0f,1f);
    }

    public void Shoot(Transform cameraTransform, Vector3 muzzlePosition, Player player)
    {
        if (cycling) return;
        if (ammoRemaining == 0) return;

        ViewModelManager.Instance.PlayShootAnimation(recoilAmount, recoilRecovery);
        player.PlayAudio(0, 0.3f);
        player.PlayAudioClipServerRpc(0, 0.3f);

        cycling = true;

        RaycastHit hit;
        if(Physics.Raycast(cameraTransform.position, GetRecoil(cameraTransform), out hit,Mathf.Infinity,hittable))
        {
            player.CastTrace(muzzlePosition, hit.point);
            player.CastTraceServerRpc(muzzlePosition, hit.point);
            PlayerCollider _collider = hit.collider.gameObject.GetComponent<PlayerCollider>();
            if(_collider != null) {

                _collider.player.SendHitServerRpc((int)(damage * _collider.damageMultiplier), NetworkManager.Singleton.LocalClientId);
            }
            
        }
        recoil += recoilAmount;
        sprayModifier = 0f;

        Cycle();
    }

    Vector3 GetRecoil(Transform cameraTransform)
    {
        return (cameraTransform.right * climbX.Evaluate(recoil*0.5f) + cameraTransform.up * (recoil * 0.3f) + cameraTransform.forward);
    }

    public void SetSway(float _sway)
    {
        sway = _sway;
    }

    async void Cycle()
    {
        await Task.Delay(cyclingRate);
        cycling = false;
    }

}
