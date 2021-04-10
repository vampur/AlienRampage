using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAi : MonoBehaviour
{
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
        Vector3 vecLookDir = transform.forward + new Vector3(Random.value * 30f - 15, 0f, Random.value * 30f - 15).normalized;
        vecLookDir.y = 0f;
        Vector3 vecTargetPos = player.transform.position;
        if((vecTargetPos - transform.position).magnitude < 25f)
        {
            vecLookDir = vecTargetPos - transform.position;
            vecLookDir.y = 0f;
            flMoveAmount = 1f;
        }

        vecLookDir = vecLookDir.normalized;
        ctrl.flForwardMove = flMoveAmount;
        ctrl.vecLookDir = vecLookDir;
    }
}
