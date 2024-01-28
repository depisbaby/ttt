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

    public NetworkVariable<FixedString128Bytes> username = new NetworkVariable<FixedString128Bytes>("",
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> health = new NetworkVariable<int>(100,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<Quaternion> networkOriention = new NetworkVariable<Quaternion>(default,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public Transform headTransform;
    public Transform cameraTransform;
    public GameObject visuals;
    public GameObject playerCollider;
    public PlayerVisuals playerVisuals;
    public GameObject flashBangPrefab;
    public AudioSource audioSource;
    public AudioClip[] audioClips;
    public LineRenderer lineRenderer;
    public GameObject bodyPrefab;

    //local
    [SerializeField]Gun gun;

    float flashAmount;

    bool frozen;
    float walkingTick;

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

            PlayerCam.Instance.BindToPlayer(this);
            SessionManager.Instance.playerPrefabDone = true;
            username.Value = SessionManager.Instance.localUsername;
            
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
        playerVisuals.transform.rotation = networkOriention.Value;

        if (!IsOwner) return;

        if (frozen) return;

        networkOriention.Value = orientation.rotation;

        if (Input.GetButton("Fire1"))
        {
            if(gun != null)
            {
                gun.Shoot(cameraTransform, ViewModelManager.Instance.muzzle.position, this);
            }
        }

        if (Input.GetButtonDown("ThrowUtil"))
        {
            if (flashAmount == 0) return;

            PlayAudio(2, 0.5f);
            flashAmount -= 1;
            ThrowFlashServerRpc(cameraTransform.position, cameraTransform.forward);
        }

        if(walkingTick > 0)
        {
            walkingTick -= Time.deltaTime;
        }
        else
        {
            if((horizontalInput != 0 || verticalInput != 0) && !Input.GetButton("Fire1") && grounded)
            {
                PlayAudio(3, 0.1f);
                PlayAudioClipServerRpc(3, 0.1f);
            }
            walkingTick = 0.40f;
        }

        

        #region Move
        // ground check
        grounded = Physics.Raycast(transform.position + new Vector3(0f,0.2f,0f), Vector3.down, 0.5f, whatIsGround);

        MyInput();
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
        MovePlayer();
    }

    [ClientRpc] public void TeleportAndFreezeClientRpc(Vector3 position)
    {
        if (!IsOwner) return;
        Debug.Log(position);

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

            Debug.Log("Ei toimi");
            transform.position = position;
        }
    }

    [ClientRpc]
    public void UnfreezeClientRpc()
    {
        frozen = false;
        
    }

    [ServerRpc(RequireOwnership = false)] public void SendHitServerRpc(int damage, ulong shooterId)
    {
        if(health.Value - damage < 0)
        {
            MatchManager.Instance.PlayerKilled(this, LobbyMenu.Instance.playersDict[shooterId]);
            KillClientRpc();
        }

        health.Value -= damage;
    }

    [ClientRpc] public void KillClientRpc()
    {
        if(frozen) return;

        if(IsOwner)
        {
            rb.useGravity = false;
            Hud.Instance.ShowDeath(true);
            playerVisuals.Kill(rb.velocity);
            ViewModelManager.Instance.HideViewModel(true);
            SpawnBodyServerRpc(rb.velocity, orientation.rotation);
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
            playerVisuals.Revive();
            ViewModelManager.Instance.HideViewModel(false);

            playerVisuals.HideForOwner();
            flashAmount = 3;
        }

        frozen = false;
        visuals.SetActive(true);
        playerCollider.SetActive(true);
    }

    [ClientRpc] public void FlashPlayerClientRpc()
    {
        if(!IsOwner)
        {
            return;
        }

        PlayAudio(1, 0.5f);
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

    [ServerRpc] public void PlayAudioClipServerRpc(int id, float volume)
    {
        PlayAudioClipClientRpc(id, volume);
    }

    [ClientRpc] public void PlayAudioClipClientRpc(int id, float volume)
    {
        if (IsOwner) return;

        PlayAudio(id, volume);
    }
    //
    public void PlayAudio(int id, float volume)
    {

        audioSource.Stop();
        audioSource.volume = volume;
        audioSource.clip = audioClips[id];
        audioSource.Play();
    }

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

    async void DespawnObject(GameObject go, int delay)
    {
        await Task.Delay((int)delay);

        go.GetComponent<NetworkObject>().Despawn();
        Destroy(go);
    }

    [ClientRpc]public void SetColorClientRpc(bool blue)
    {
        if(blue)
        {
            playerVisuals.SetTeamColot(Color.blue);
        }
        else
        {
            playerVisuals.SetTeamColot(Color.red);
        }
    }

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

    Rigidbody rb;

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(horizontalInput != 0 || verticalInput != 0)
        {
            SetWalkingServerRpc(true);
            
        }
        else
        {
            SetWalkingServerRpc(false);
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

    [ServerRpc] void SetWalkingServerRpc(bool walking)
    {
        SetWalkingClientRpc(walking);
    }

    [ClientRpc] void SetWalkingClientRpc(bool walking)
    {
        playerVisuals.SetWalking(walking);
    }
    #endregion
}
