using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scorpion : Enemy
{
    //private bool bRotateAnims = true;
    // Material Dissolve Variables
   
    float fStunTime = 1f;
    float fInvulnerableTime = 0.4f;

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
            if (!bIsStun)
            {
                if (!bTargetFound)
                {
                    FindTarget();
                }
                else
                {
                    CheckTargetInRange(fAttackRange, fFollowRange, 100);
                    CheckWalkingArea(startPosition);
                }
            }

            CalculateInvulnerability(fInvulnerableTime);
            CalculateStun(fStunTime);
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
            if (!bIsStun)
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
