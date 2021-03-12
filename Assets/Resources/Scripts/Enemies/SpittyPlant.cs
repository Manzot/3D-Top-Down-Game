﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpittyPlant : Enemy
{

    float fINVULNERABILITY_TIME = 0.7f;
    float fTARGET_FOLLOW_RANGE = 120f;

    ProjectileThrower projectileThrower;

    private void Start()
    {
        base.Initialize();
        fAttackRange = 80f;
        projectileThrower = GetComponent<ProjectileThrower>();
    }

    private void Update()
    {
        base.Refresh();

        //SetAnimations(); // Make function to set basic animations

        if (bIsAlive)
        {
           // if (bIsGrounded)
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
            if (HelpUtils.Grounded(transform, 0.2f))
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
            projectileThrower.InitializeProjectile();
            // anim.SetTrigger("StabAttack"); set attack animation here
            StartCoroutine(HelpUtils.ChangeBoolAfter((bool b) => { fAttackWaitTimeCounter = fAttackWaitTime; bCanFollow = true; /* bCanRotate = true; */}, false, fAttackWaitTime)); //anim.GetCurrentAnimatorStateInfo(0).length));
            fAttackWaitTimeCounter = fAttackWaitTime;
          //  bCanRotate = true;
        }
    }

}
