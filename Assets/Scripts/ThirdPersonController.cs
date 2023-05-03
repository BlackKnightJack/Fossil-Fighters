using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    //Constants
    const float PADDING = 0.01f;

    //Public variables
    public Vector3 lookTarget;
    public float cameraSpeed;
    public float moveSpeed;
    public float zoomSpeed;

    //Component cache
    Animator anim;
    Camera mainCamera;
    CharacterController cc;

    //Input variables
    Vector2 cameraMoveVector;
    Vector2 playerMoveVector;

    //Camera variables
    Vector3 cameraOffset;
    float trueRadius;
    float currentRadius;

    //Misc variables
    int layerMask;

    // Start is called before the first frame update
    void Start()
    {
        //Cache components
        anim = GetComponent<Animator>();
        mainCamera = Camera.main;
        cc = GetComponent<CharacterController>();

        //Calculate camera offset
        cameraOffset = mainCamera.transform.position - CameraTarget();
        trueRadius = cameraOffset.magnitude;
        currentRadius = trueRadius;

        //Invert layer mask?
        layerMask = 1 << LayerMask.NameToLayer("Character");
        layerMask = ~layerMask;
    }

    // Update is called once per frame
    void Update()
    {
        //Assign movement vector
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            playerMoveVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }
        else
        {
            playerMoveVector = Vector2.zero;
        }

        //Assign camera vectors
        {
            cameraMoveVector = Vector2.zero;
            if (Input.mousePosition.x <= PADDING) { cameraMoveVector += Vector2.left; }
            if (Input.mousePosition.x >= Screen.width - PADDING) { cameraMoveVector += Vector2.right; }
            if (Input.mousePosition.y <= PADDING) { cameraMoveVector += Vector2.down; }
            if (Input.mousePosition.y >= Screen.height - PADDING) { cameraMoveVector += Vector2.up; }
        }
    }

    // FixedUpdate is called once per physics frame
    void FixedUpdate()
    {
        //Movement logic
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            //Get and face the angle of movement
            float facingAngle = Mathf.Atan2(playerMoveVector.x, playerMoveVector.y) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, facingAngle, transform.eulerAngles.z);

            //Move in that direction
            Vector3 moveVector = Vector3.ClampMagnitude(new Vector3(Mathf.Sin(Mathf.Deg2Rad * facingAngle), 0, Mathf.Cos(Mathf.Deg2Rad * facingAngle)) * playerMoveVector.magnitude, 1);
            cc.Move(moveVector * moveSpeed);
        }

        //Camera logic
        {
            //Calculate angles
            float angleXZ = Mathf.Atan2(cameraOffset.x, cameraOffset.z) * Mathf.Rad2Deg + cameraMoveVector.x * cameraSpeed;
            float angleY = Mathf.Clamp(Mathf.Asin(cameraOffset.y / currentRadius) * Mathf.Rad2Deg - cameraMoveVector.y * cameraSpeed, -89, 89);

            //Calculate vector components
            Vector3 vectorY = Vector3.up * Mathf.Sin(Mathf.Deg2Rad * angleY);
            Vector3 vectorXZ = new Vector3(Mathf.Sin(Mathf.Deg2Rad * angleXZ), 0, Mathf.Cos(Mathf.Deg2Rad * angleXZ)) * Mathf.Cos(Mathf.Deg2Rad * angleY);

            //Add vectors together
            Vector3 compositeVector = vectorXZ + vectorY;

            //Contract radius as necessary
            RaycastHit hit;
            if (Physics.Raycast(CameraTarget(), compositeVector, out hit, currentRadius, layerMask))
            {
                //currentRadius = hit.distance - PADDING;
                currentRadius = Mathf.MoveTowards(currentRadius, hit.distance - PADDING, zoomSpeed);
            }

            //Extend radius as necessary
            else if (currentRadius != trueRadius)
            {
                float destinationLength = trueRadius;
                if (Physics.Raycast(CameraTarget(), compositeVector, out hit, trueRadius, layerMask)) { destinationLength = hit.distance - PADDING; }
                currentRadius = Mathf.MoveTowards(currentRadius, destinationLength, zoomSpeed);
            }

            //Reassign camera offset
            cameraOffset = compositeVector * currentRadius;
        }
    }

    // LateUpdate is called once per frame after Update
    void LateUpdate()
    {
        mainCamera.transform.position = CameraTarget() + cameraOffset;
        mainCamera.transform.LookAt(CameraTarget());
    }

    //Functions
    Vector3 CameraTarget()
    {
        return transform.position + lookTarget;
    }

    bool MoveCamera()
    {
        bool WASDactive = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
        bool screenPush = Input.mousePosition.x <= PADDING || Input.mousePosition.x >= Screen.width - PADDING
            || Input.mousePosition.y <= PADDING || Input.mousePosition.y >= Screen.height - PADDING;

        return WASDactive || screenPush;
    }
}
