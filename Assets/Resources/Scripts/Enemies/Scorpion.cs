using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scorpion : Enemy
{
    //private bool bRotateAnims = true;
    // Material Dissolve Variables
   
    float fSTUN_TIME = 0f; // this is extra time after the animation

    void Start()
    {
        base.Initialize();
        fAttackRange = 2.5f;
        fFollowRange = 120f;
    }

    // Update is called once per frame
    private void Update()
    {
        base.Refresh();

        SetAnimations();

        if (bIsAlive)
        {
            if (!bIsInvulnerable)
            {
                if (!bTargetFound)
                {
                    FindTarget();
                }
                else
                {
                    CheckTargetInRange(fAttackRange, fFollowRange);
                    CheckWalkingArea(startPosition);
                }
            }
            else
                CalculateInvulnerability(fSTUN_TIME);
        }
        else
        {
             DissolveOnDeath(0.6f);
        }
    }
    private void FixedUpdate()
    {
        base.FixedRefresh();

        if (bIsAlive)
        {
            if (!bIsInvulnerable)
            {
                if (bTargetFound)
                {
                    if (!bInAttackRange)
                    {
                        if (bCanFollow && !bIsAttacking)
                        {
                            moveScr.FollowTarget(targetPlayer.transform.position);
                        }
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
            bIsAttacking = true;
            anim.SetTrigger("StabAttack");
           // bCanRotate = false;
            StartCoroutine(HelpUtils.ChangeBoolAfter((bool b) => { bIsAttacking = false; fAttackWaitTimeCounter = fAttackWaitTime; bCanFollow = true; /*bCanRotate = true;*/ }, false, fAttackWaitTime));
            fAttackWaitTimeCounter = fAttackWaitTime;
            //bCanRotate = true;
        }
    }
    public void SetAnimations()
    {
        if (bIsAlive)
        {
            //float f = bIsMoving ? 1 : 0;
            anim.SetBool("isMoving", moveScr.IsMoving());
            anim.SetBool("canAttack", bInAttackRange);

            if (bIsHit)
            {
                anim.SetTrigger("isHit");
                bIsHit = false;
            }
        }
        else
        {
            anim.SetTrigger("isDead");
        }

    }
  
}
