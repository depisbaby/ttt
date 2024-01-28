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

        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX * 1000;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY * 1000;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void BindToPlayer(Player player)
    {
        player.cameraTransform = transform;
        moveCamera.cameraPosition = player.headTransform;
        orientation = player.orientation;
    }
}
