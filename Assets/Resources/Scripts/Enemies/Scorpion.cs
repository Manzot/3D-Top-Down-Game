﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scorpion : Enemy, IHittable
{
    const float fTARGET_FOLLOW_RANGE = 60f;
    const float fPUSHBACKFORCE = 3f;
    private bool bRotateAnims = true;
    float fINVULNERABILITY_TIME = 0.1f; // this is extra time after the animation

    void Start()
    {
        base.Initialize();
        fAttackRange = 2.5f;
    }

    // Update is called once per frame
    private void Update()
    {
        base.Refresh();

        SetAnimations();

        if (bIsAlive)
        {
            if(HelperFunctions.Grounded(transform, 0.2f))
            {
                if (!bTargetFound)
                {
                    FindingTarget();
                }
                else
                {
                    CheckTargetInRange(fAttackRange, fTARGET_FOLLOW_RANGE);
                }
            }
            CalculateInvulnerability(fINVULNERABILITY_TIME);
        }
    }
    private void FixedUpdate()
    {
        base.FixedRefresh();
        if (bIsAlive)
        {
            if (HelperFunctions.Grounded(transform, 0.2f))
            {
                if (!bTargetFound)
                {
                    MovingRandomly();
                }
                else
                {
                    if (!bCanAttack)
                    {
                        FollowTarget();
                    }
                    else
                    {
                        AttackMove();
                    }
                }
            }
        }
    }
    public void AttackMove()
    {
        if(fAttackWaitTimeCounter <= 0)
        {
            bCanFollow = false;
            anim.SetTrigger("StabAttack");
            StartCoroutine(HelperFunctions.ChangeBoolAfter((bool b) => { fAttackWaitTimeCounter = fAttackWaitTime; bCanFollow = true; bCanRotate = true; }, false, anim.GetCurrentAnimatorStateInfo(0).length));
            fAttackWaitTimeCounter = fAttackWaitTime;
            bCanRotate = true;
        }
    }
    public void SetAnimations()
    {
        if (bIsAlive)
        {
            anim.SetFloat("isWalking", rbody.velocity.sqrMagnitude);

            if (bIsHit)
            {
                anim.SetTrigger("isHit");
                bIsHit = false;
            }

            if (bCanRotate)
            {
                if (bRotateAnims)
                {
                    anim.SetTrigger("tRotating");

                    bRotateAnims = false;

                    if (bCanFollow)
                        bCanRotate = false;
                }
            }
            else
            {
                bRotateAnims = true;
            }
        }
        else
        {
            anim.SetTrigger("isDead");
        }

    }
    public void TakeDamage(int _damage)
    {
        if (!bIsInvulnerable)
        {
            Knockback(targetPlayer.transform.position, fPUSHBACKFORCE);
            IsInvulnerable(true);
            bIsHit = true;
            bTargetFound = true;
            bCanFollow = false;
            fCurrentHitPoints -= _damage;
        }
        if (fCurrentHitPoints <= 0)
        {
            Die();
        }
    }
    
   
}
