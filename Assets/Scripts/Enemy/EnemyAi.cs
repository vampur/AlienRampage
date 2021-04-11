using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAi : MonoBehaviour
{
    public GameObject gobjDest;
    NavMeshAgent nmAgent;
    Animator anim;

    public float flKillForce = 1f;

    float flDefaultSpeed;

    [HideInInspector] public bool bDead;
    bool bDeathHandled;

    void Start()
    {
        gobjDest = GameObject.FindGameObjectWithTag("Player");
        nmAgent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        flDefaultSpeed = nmAgent.speed;

        foreach(var rb in GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
        }
        foreach(var col in GetComponentsInChildren<Collider>())
        {
            if(col.tag != "Weapon")
                col.enabled = false;
        }
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Collider>().enabled = true;
    }

    void Update()
    {
        if(bDeathHandled)
            return;
        
        if(bDead)
        {
            nmAgent.enabled = false;
            anim.enabled = false;

            foreach (var rb in GetComponentsInChildren<Rigidbody>())
            {
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.isKinematic = false;
            }
            foreach (var col in GetComponentsInChildren<Collider>())
            {
                col.enabled = true;
            }

            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<Collider>().enabled = false;
            GameObject.FindObjectOfType<Spawner>().nEnemiesLeft--;
            bDeathHandled = true;
        }
        
        nmAgent.SetDestination(gobjDest.transform.position);
        Vector3 vecVel = nmAgent.velocity;
        vecVel.y = 0f;
        float flSpeedCur = vecVel.magnitude;
        anim.SetFloat("Forward", Mathf.Clamp01(flSpeedCur/nmAgent.speed), 0.1f, Time.deltaTime);
        if((gobjDest.transform.position - transform.position).magnitude < 1.75f)
        {
            //nmAgent.speed = 1f;
            Attack();
        }
        else
        {
            nmAgent.speed = flDefaultSpeed;
        }
    }

    void Attack()
    {
        anim.SetTrigger("Attack");
    }

    void OnCollisionEnter(Collision other)
    {
        if(LayerMask.LayerToName(other.gameObject.layer) == "TKable")
            if(other.impulse.magnitude > flKillForce)
                bDead = true;
    }
}

/*
    EnemyController ctrl;
    GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        ctrl = GetComponent<EnemyController>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        HandleMoveInput();
    }

    void HandleMoveInput()
    {
        float flMoveAmount = Random.value;
        
        Vector3 vecLookDir = Vector3.zero;

        float flYawDelta = 0f;
        if(Physics.Raycast(transform.position, transform.forward, 3f, ~LayerMask.GetMask("Player")))
        {
            RaycastHit hitLeft, hitRight;
            if (Physics.Raycast(transform.position, Quaternion.AngleAxis(-15f, transform.up) * transform.forward, out hitLeft, 3f, ~LayerMask.GetMask("Player")))
                flYawDelta -= (1 - (hitLeft.distance / 3f)) * 15;
            if (Physics.Raycast(transform.position, Quaternion.AngleAxis(15f, transform.up) * transform.forward, out hitRight, 3f, ~LayerMask.GetMask("Player")))
                flYawDelta += (1 - (hitRight.distance / 3f)) * 15;
            vecLookDir = Quaternion.AngleAxis(flYawDelta, transform.up) * transform.forward;
        }
        else
        {
            if((player.transform.position - transform.position).magnitude < 25f)
            {
                vecLookDir = player.transform.position - transform.position;
                flMoveAmount = 1f;
            }
            else
            {
                flYawDelta = Random.value * 30f - 15f;
                vecLookDir = Quaternion.AngleAxis(flYawDelta, transform.up) * transform.forward;
            }
        }

        vecLookDir.y = 0f;
        vecLookDir = vecLookDir.normalized;
        ctrl.flForwardMove = flMoveAmount;
        ctrl.vecLookDir = vecLookDir;
    }
*/