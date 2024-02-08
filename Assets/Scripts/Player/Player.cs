using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking;
using Unity.Netcode;
using Unity.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;
using Unity.Netcode.Components;
using UnityEngine.UIElements;

public class Player : NetworkBehaviour
{
    public static Player localPlayer;
    public enum Team
    {
        Blue = 1,
        Red = 2,
    }

    public NetworkVariable<FixedString128Bytes> username = new NetworkVariable<FixedString128Bytes>("",NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> health = new NetworkVariable<int>(100,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<Quaternion> networkOriention = new NetworkVariable<Quaternion>(default,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> walking = new NetworkVariable<bool>(default,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [HideInInspector] public Team team;
    [HideInInspector] public Transform cameraTransform;

    [Header("PII")]
    public Transform headTransform;
    public GameObject visuals;
    public GameObject playerCollider;
    public PlayerVisuals playerVisuals;
    public GameObject flashBangPrefab;
    public AudioSource[] audioSources;
    public AudioClip[] audioClips;
    public LineRenderer lineRenderer;
    public GameObject bodyPrefab;
    public LayerMask interactMask;
    //

    //local
    public Gun gun;

    float flashAmount;
    bool frozen;
    float walkingTick;
    float weaponRaisedTick;
    bool weaponRaised;

    private async void Start()
    {
        //On everyone
        frozen = true;
        //On owner
        if (IsOwner)
        {
            #region Move
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;

            readyToJump = true;
            #endregion

            localPlayer = this;
            PlayerCam.Instance.BindToPlayer(this);
            SessionManager.Instance.playerPrefabDone = true;
            username.Value = SessionManager.Instance.localUsername;
            playerVisuals.HideForOwner();
        }

        //On server
        if (IsServer)
        {
            while(username.Value == "")
            {
                await Task.Yield();
            }

            LobbyMenu.Instance.OnPlayerJoined(this);
        }

    }

    private void OnDestroy()
    {
        LobbyMenu.Instance.OnPlayerLeft(this);
    }

    private void Update()
    {

        AllUpdate_FootSteps();
        playerVisuals.SetWalking(walking.Value);

        if (!IsOwner) return;

        if (frozen) return;

        if (MenuManager.Instance.actionBlockingWindowOpen) return;

        playerVisuals.orientation.Value = orientation.rotation;
        playerVisuals.cameraRotation.Value = cameraTransform.rotation;

        MyInput();

        Combat();
        Interact();

        #region Move
        // ground check
        grounded = Physics.Raycast(transform.position + new Vector3(0f,0.2f,0f), Vector3.down, 0.5f, whatIsGround);

        SpeedControl();

        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
        #endregion
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        if (MenuManager.Instance.actionBlockingWindowOpen) return;
        MovePlayer();
    }

    #region User Inputs

    void Combat()
    {
        if (Input.GetButtonDown("ThrowUtil"))
        {
            if (flashAmount == 0) return;

            flashAmount -= 1;
            ThrowFlashServerRpc(cameraTransform.position, cameraTransform.forward);
        }

        if (gun == null) return;

        ViewModelManager.Instance.HideViewModel(true);

        if (Input.GetButton("Fire2"))
        {
            weaponRaisedTick = Mathf.Clamp(weaponRaisedTick + Time.deltaTime * gun.raisingSpeed, 0f, 1f);

            if (weaponRaisedTick >= 1f) {
                ViewModelManager.Instance.HideViewModel(false);
            }

            if(weaponRaisedTick > 0f && weaponRaisedTick < 1f)
            {
                Hud.Instance.ShowRaiseWeapon();
            }
            else
            {
                Hud.Instance.ShowNoIcon();
            }

            if (weaponRaised == false)
            {
                weaponRaised = true;
                playerVisuals.ChangeAppearanceServerRpc("A_" + gun.itemName);
            }

        }
        else
        {
            if (weaponRaised == true)
            {
                weaponRaised = false;
                playerVisuals.ChangeAppearanceServerRpc("default");
            }

            Hud.Instance.ShowNoIcon();
            weaponRaisedTick = 0f;
        }

        if (Input.GetButton("Fire1") && weaponRaisedTick >= 1f)
        {
            gun.Shoot(cameraTransform, ViewModelManager.Instance.GetMuzzleTransform().position, this);
            
        }

    }

    void Interact()
    {
        if (Input.GetButtonDown("Interact"))
        {
            RaycastHit hit;
            if(Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, 2f, interactMask))
            {
                IInteractable i = hit.collider.transform.parent.gameObject.GetComponent<IInteractable>();
                if (i != null)
                {
                    //Debug.Log("gujisdfgiu");
                    i.Interact();
                }
            }
        }
    }

    #endregion

    #region Teleportation
    [ClientRpc] public void TeleportAndFreezeClientRpc(Vector3 position)
    {
        if (!IsOwner) return;
        ///Debug.Log(position);

        VoikkoNytVittuToimia(position);
        
        
    }

    public async void VoikkoNytVittuToimia(Vector3 position)
    {
        transform.position = position;
        while (true)
        {
            await Task.Delay(10);
            if ((position - transform.position).magnitude < 1f)
            {
                return;
            }

            //Debug.Log("Ei toimi");
            transform.position = position;
        }
    }
    #endregion

    #region Freeze
    [ClientRpc]
    public void FreezeClientRpc(bool freeze)
    {
        frozen = freeze;
        
    }
    #endregion

    #region Hit reg
    [ServerRpc(RequireOwnership = false)] public void SendHitServerRpc(int damage, ulong shooterId)
    {
        if (LobbyMenu.Instance.playersDict[shooterId].health.Value < 1)
        {
            return;
        }

        if(health.Value - damage < 0)
        {
            MatchManager.Instance.PlayerKilled(this, LobbyMenu.Instance.playersDict[shooterId]);
            KillClientRpc();
        }

        health.Value -= damage;
    }
    #endregion

    #region Kill/Revive
    [ClientRpc] public void KillClientRpc()
    {
        if(frozen) return;

        if(IsOwner)
        {
            rb.useGravity = false;
            Hud.Instance.ShowDeath(true);
            ViewModelManager.Instance.HideViewModel(true);
            SpawnBodyServerRpc(rb.velocity, orientation.rotation);
            InventoryMenu.Instance.UpdateInventory();
        }

        frozen = true;
        visuals.SetActive(false);
        playerCollider.SetActive(false);
    }

    [ClientRpc]
    public void ReviveClientRpc()
    {
        if (IsOwner)
        {
            rb.useGravity = true;
            Hud.Instance.ShowDeath(false);
            ViewModelManager.Instance.HideViewModel(false);

            playerVisuals.HideForOwner();
            flashAmount = 3;
        }

        frozen = false;
        visuals.SetActive(true);
        playerCollider.SetActive(true);
    }
    #endregion

    #region Flashing
    [ClientRpc] public void FlashPlayerClientRpc()
    {
        if(!IsOwner)
        {
            return;
        }

        Hud.Instance.Flash();
    }

    [ServerRpc(RequireOwnership = false)] public void ThrowFlashServerRpc(Vector3 position, Vector3 direction)
    {
        GameObject go = Instantiate(flashBangPrefab);
        go.GetComponent<NetworkObject>().Spawn();
        go.GetComponent<NetworkTransform>().Teleport(position, go.transform.rotation , go.transform.localScale);
        
        FlashBang flash = go.GetComponent<FlashBang>();
        flash.rb = go.AddComponent<Rigidbody>();
        flash.rb.mass = 0.1f;
        flash.rb.interpolation = UnityEngine.RigidbodyInterpolation.Interpolate;
        flash.rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        flash.Throw(position, direction);

    }
    #endregion

    #region Audio Sources
    [ServerRpc] public void PlayAudioClipServerRpc(ushort sourceId, ushort clipId, float volume)
    {
        PlayAudioClipClientRpc(sourceId, clipId, volume);
    }

    [ClientRpc] public void PlayAudioClipClientRpc(ushort sourceId, ushort clipId, float volume)
    {
        if (IsOwner) return;

        PlayAudio(sourceId, clipId, volume);
    }
    //
    public void PlayAudio(ushort sourceId, ushort clipId, float volume)
    {
        if(sourceId == 0) return;

        audioSources[sourceId].Stop();
        audioSources[sourceId].volume = volume;
        audioSources[sourceId].clip = audioClips[clipId];
        audioSources[sourceId].Play();
    }

    void AllUpdate_FootSteps()
    {
        if (walking.Value)
        {
            audioSources[0].volume = 0.3f;
        }
        else
        {
            audioSources[0].volume = 0f;
        }
    }

    #endregion

    #region Bullet traces
    [ServerRpc] public void CastTraceServerRpc(Vector3 start, Vector3 end)
    {
        CastTraceClientRpc(start, end);
    }
    [ClientRpc] public void CastTraceClientRpc(Vector3 start, Vector3 end)
    {
        if (IsOwner) return;
        CastTrace(start, end);
    }
    public async void CastTrace(Vector3 start, Vector3 end)
    {
        Vector3[] positions = { start, end };
        lineRenderer.SetPositions(positions);

        lineRenderer.enabled = true;

        await Task.Delay(10);

        lineRenderer.enabled = false;
    }
    #endregion

    #region Spawn dead body
    [ServerRpc]public void SpawnBodyServerRpc(Vector3 direction, Quaternion rotation)
    {
        GameObject go = Instantiate(bodyPrefab);
        go.GetComponent<NetworkObject>().Spawn();
        go.GetComponent<NetworkTransform>().Teleport(transform.position + new Vector3(0,1,0), rotation, transform.localScale);

        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.interpolation = UnityEngine.RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.velocity = direction;

        DespawnObject(go, 10 * 1000);

    }
    #endregion

    #region Equip gun
    public void SetEquippedGun(Gun _gun)
    {
        gun = _gun;
    }
    #endregion

    #region Util
    async void DespawnObject(GameObject go, int delay)
    {
        await Task.Delay((int)delay);

        go.GetComponent<NetworkObject>().Despawn();
        Destroy(go);
    }

    #endregion

    #region Movement
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [HideInInspector] public float walkSpeed;
    [HideInInspector] public float sprintSpeed;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    public Rigidbody rb;

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(horizontalInput != 0 || verticalInput != 0)
        {
            walking.Value = true;
        }
        else
        {
            walking.Value= false;
        }

        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {

        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
    }

    #endregion
}
