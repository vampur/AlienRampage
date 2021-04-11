using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackCollider : MonoBehaviour
{
    Animator anim;
    bool bAttackReady;

    void Start()
    {
        anim = GetComponentInParent<Animator>();
    }

    void FixedUpdate()
    {
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            bAttackReady = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            return;
        if (bAttackReady && other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerController>().ApplyDamage(5);
            bAttackReady = false;
        }
    }
}
