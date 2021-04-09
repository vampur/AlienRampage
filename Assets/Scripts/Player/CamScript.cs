using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamScript : MonoBehaviour
{
    public static CamScript GetCamScript;
    void Awake()
    {
        if(!GetCamScript)
            GetCamScript = this;
        else
            Debug.LogWarning("Erawr CamScript already exists...");
    }

    public float flMouseSensitivity = 5.0f;
    public float flFollowSpeed = 10.0f;
    public float flClampAngle = 89.0f;

    public bool bMouseSmoothing = false;

    public Transform trnsfTarget;
    Transform trnfPivot;
    Transform trnfCamera;

    float flTiltAmount;
    float flLookAngle;

    float flTurnSmoothing = 9.0f;

    // Start is called before the first frame update
    void Start()
    {
        trnfCamera = Camera.main.transform;
        trnfPivot = trnfCamera.parent;

        flLookAngle = transform.rotation.eulerAngles.y;

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float flDeltaTime = Time.deltaTime;
        float flMouseX = Input.GetAxis("Mouse X");
        float flMouseY = Input.GetAxis("Mouse Y");

        HandleRotations(flMouseX, flMouseY, flDeltaTime);

        float flSpeed = flFollowSpeed * flDeltaTime;
        Vector3 vecTargetPos = Vector3.Lerp(transform.position, trnsfTarget.position, flSpeed);

        transform.position = vecTargetPos;
    }

    void LateUpdate()
    {
        Vector3 vecTargetOffset = new Vector3();
        RaycastHit hit;
        if(Physics.Raycast(trnsfTarget.position, (trnfCamera.position - trnsfTarget.position).normalized, out hit, (trnfCamera.position - transform.position).magnitude, ~LayerMask.GetMask("Player")))
        {
            vecTargetOffset = hit.point - trnfCamera.position;
        }
        transform.position += vecTargetOffset;
    }

    void HandleRotations(float flMouseX, float flMouseY, float flDeltaTime)
    {
        flTiltAmount -= flMouseY * flMouseSensitivity;
        flTiltAmount = Mathf.Clamp(flTiltAmount, -flClampAngle, flClampAngle);

        trnfPivot.localRotation = Quaternion.Euler(flTiltAmount, 0.0f, 0.0f);

        flLookAngle += flMouseX * flMouseSensitivity;

        if(bMouseSmoothing)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0.0f, flLookAngle, 0.0f), flDeltaTime * flTurnSmoothing);
        else
            transform.rotation = Quaternion.Euler(0.0f, flLookAngle, 0.0f);

    }
}
