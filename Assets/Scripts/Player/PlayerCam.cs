using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public static PlayerCam Instance;
    private void Awake()
    {
        Instance = this;
    }

    public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation;
    float yRotation;

    [SerializeField]MoveCamera moveCamera;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (orientation == null) return;
        if (MenuManager.Instance.actionBlockingWindowOpen) return;

        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * sensX * 1000;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensY * 1000;

        //Recoil is added in Gun.cs!!!!!!!!!!!!!!!!!!

        transform.rotation = transform.rotation * Quaternion.Euler(-mouseY, 0, 0);
        moveCamera.transform.rotation = moveCamera.transform.rotation * Quaternion.Euler(0, mouseX, 0);
        orientation.rotation = moveCamera.transform.rotation;
    }

    public void BindToPlayer(Player player)
    {
        player.cameraTransform = transform;
        moveCamera.cameraPosition = player.headTransform;
        orientation = player.orientation;
    }

    
}
