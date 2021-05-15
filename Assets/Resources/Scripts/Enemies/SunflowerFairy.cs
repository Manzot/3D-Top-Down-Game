using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunflowerFairy : Enemy
{
    private float fVisionRadius = 2.2f;
    private float fVisionDistance = 4f;
    float fStunTime = 0.2f;
    float fTARGET_FOLLOW_RANGE = 120f;

    ProjectileThrower projectileThrower;

    private void Start()
    {
        base.Initialize();
        fAttackRange = 50f;
        projectileThrower = GetComponent<ProjectileThrower>();
    }

    private void Update()
    {
        base.Refresh();

        SetAnimations(); // Make function to set basic animations

        if (bIsAlive)
        {
           // if (bIsGrounded)
            {
                if (!bTargetFound)
                {
                    FindTarget(fVisionRadius, fVisionDistance);
                }
                else
                {
                   CheckTargetInRange(fAttackRange, fTARGET_FOLLOW_RANGE, 0.95f);
                }
            }
            CalculateInvulnerability(fStunTime);
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
          //  if (HelpUtils.Grounded(transform, 0.2f))
            {
                if (bTargetFound)
                {
                    if (bInAttackRange)
                    {
                        AttackMove();
                    }
                }
            }
        }
    }
    public void AttackMove()
    {
        if (fAttackWaitTimeCounter <= 0)
        {
            bCanFollow = false;
           // projectileThrower.InitializeProjectile();
            anim.SetTrigger("attack");
            StartCoroutine(HelpUtils.ChangeBoolAfter((bool b) => { fAttackWaitTimeCounter = fAttackWaitTime; bCanFollow = true; /* bCanRotate = true; */}, false, fAttackWaitTime)); //anim.GetCurrentAnimatorStateInfo(0).length));
            fAttackWaitTimeCounter = fAttackWaitTime;
          //  bCanRotate = true;
        }
    }

    void SetAnimations()
    {
        if (bIsAlive)
        {
            anim.SetBool("isMoving", moveScr.IsMoving());
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
    public void ThrowProjectile()
    {
        projectileThrower.InitializeProjectile();
    }
}
