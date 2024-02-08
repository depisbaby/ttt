using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class Gun : Item
{
    // Start is called before the first frame update
    [Header("Gun")]
    public int cyclingRate;
    public int ammoRemaining;
    bool cycling;
    public float recoilAmount;
    public float recoilRecovery;
    public LayerMask hittable;
    public int damage;
    public AnimationCurve climbX;
    public float randomDivergence;
    public float raisingSpeed;

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
        player.PlayAudio(1, 0, 0.3f);
        player.PlayAudioClipServerRpc(1,0, 0.3f);

        cycling = true;

        Vector3 recoiledDirection = GetRecoil(cameraTransform);
        RaycastHit hit;
        if(Physics.Raycast(cameraTransform.position, recoiledDirection, out hit,Mathf.Infinity,hittable))
        {
            player.CastTrace(muzzlePosition, hit.point);
            player.CastTraceServerRpc(muzzlePosition, hit.point);
            PlayerCollider _collider = hit.collider.gameObject.GetComponent<PlayerCollider>();
            if(_collider != null) {

                _collider.player.SendHitServerRpc((int)(damage * _collider.damageMultiplier), NetworkManager.Singleton.LocalClientId);
            }
            
        }
        cameraTransform.rotation = cameraTransform.rotation * Quaternion.Euler(-recoil, 0 ,0);
        recoil += recoilAmount;
        sprayModifier = 0f;

        Cycle();
    }

    Vector3 GetRecoil(Transform cameraTransform)
    {
        Vector3 _randomDivergence = new Vector3(UnityEngine.Random.Range(-randomDivergence, randomDivergence), UnityEngine.Random.Range(-randomDivergence, randomDivergence), UnityEngine.Random.Range(-randomDivergence, randomDivergence));
        
        float velocityModifier = Mathf.Clamp(Player.localPlayer.rb.velocity.magnitude,0, 5f) * 0.01f;
        Vector3 runningInaccuracy = new Vector3(UnityEngine.Random.Range(-velocityModifier, velocityModifier), UnityEngine.Random.Range(-velocityModifier, velocityModifier), UnityEngine.Random.Range(-velocityModifier, velocityModifier));

        return (cameraTransform.right * climbX.Evaluate(recoil * 0.5f) + cameraTransform.forward) + _randomDivergence + runningInaccuracy;
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

    public override void Equip()
    {
        Player.localPlayer.SetEquippedGun(this);
    }

}
