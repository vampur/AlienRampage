using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    // components
    Rigidbody rb;
    Animator anim;

    public float flMoveSpeed = 2f;
    public float flTurnSpeed = 5f;

    //[HideInInspector]
    public float flForwardMove;
    //[HideInInspector]
    public Vector3 vecLookDir;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleAnims();
    }

    void FixedUpdate()
    {
        HandleMove();
    }

    void HandleAnims()
    {
        anim.SetFloat("Forward", flForwardMove, 0.1f, Time.deltaTime);
    }

    void HandleMove()
    {
        vecLookDir.y = 0f;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(vecLookDir), flTurnSpeed * Time.fixedDeltaTime);

        Vector3 vecVelTarget = transform.forward * flForwardMove * flMoveSpeed;
        Vector3 vecVelDelta = vecVelTarget - rb.velocity;
        vecVelDelta /= 3f;
        vecVelDelta.y = 0;
        rb.velocity += vecVelDelta;
    }
}
