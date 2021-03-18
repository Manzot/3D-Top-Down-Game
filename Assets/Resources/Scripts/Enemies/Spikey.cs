using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikey : Enemy
{
    const float fCLOSE_RANGE = 7f;

    public float fVisionRadius;
    public float fVisionDistance;
    public float fTackleSpeed = 6f;
    float fSpinAttackMaxTime = 3f;
    float fSTUN_TIME = 0f; // this is extra time after the animation
    
    private void Start()
    {
        base.Initialize();
        fAttackRange = 20f;
        fFollowRange = 120f;
    }
    public void Update()
    {
        base.Refresh();

        SetAnimations();

        if (bIsAlive)
        {
            if (!bTargetFound)
            {
                FindingTarget(fVisionRadius, fVisionDistance);
            }
            else
            {
                if (!bIsAttacking)
                {
                    CheckTargetInRange(fAttackRange, fFollowRange);
                    CheckWalkingArea(startPosition);
                }
            }
            CalculateInvulnerability(fSTUN_TIME);
        }
        else
        {
          //  DissolveOnDeath(0.6f);
        }
        
        Debug.Log(bTargetFound);
    }
    private void FixedUpdate()
    {
        base.FixedRefresh();
        if (bIsAlive)
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
                    if (!bIsAttacking)
                    {
                        AttackMove();
                    }
                    else
                        Tackle(attackPos, fTackleSpeed);
                }
            }
        }
    }
    // Add function to make a random chance to hide under shell and look around;
    // if too close to player either stab attack or hide undear shell
    // if in between noraml attack range and very close range, then try to maintain some distancne to start spinning attack
    // if at normal attack range, then use spinning attack


    public void HideUnderShell()
    {
        moveScr.SetMovementActive(false);
        // set anim to hide and seek
    }
    // Move Towards Player at fast speed
    // Upon hitting get knocked up for few seconds
    // Upon not hitting, stop spinning after few seconds
    Vector3 attackPos;
    public void SpinningAttack()
    {
        StopAllCoroutines();
        anim.SetBool("spinAttack", true);
        attackPos = (targetPlayer.transform.position - transform.position).normalized;
    }

    public void AttackMove()
    {
        if (fAttackWaitTimeCounter <= 0)
        {
            bCanFollow = false;
            bIsAttacking = true;
            SpinningAttack();
            StartCoroutine(HelpUtils.WaitForSeconds(delegate {
                anim.SetBool("spinAttack", false);
                bIsAttacking = false; 
                bCanFollow = true; 
                fAttackWaitTimeCounter = fAttackWaitTime; 
            },
            fSpinAttackMaxTime));

            fAttackWaitTimeCounter = fAttackWaitTime;
        }
    }
    public override void CheckTargetInRange(float _fAttackRange, float _fTargetFollowRange)
    {
        fAttackWaitTimeCounter -= Time.deltaTime;
        float _fDistance = (transform.position - targetPlayer.transform.position).sqrMagnitude;

        if (_fDistance >= _fTargetFollowRange)
        {
            if (!bIsInvulnerable && bTargetFound)
            {
                ResetBools();
            }
        }
        else if (_fDistance <= _fAttackRange && _fDistance >= fCLOSE_RANGE)
        {
            bCanFollow = false;
            moveScr.SetIsMoving(false);
            if (!bIsInvulnerable)
            {
                if (fAttackWaitTimeCounter <= 0)
                {
                    Vector3 dir = (targetPlayer.transform.position - transform.position).normalized;
                    float dot = Vector3.Dot(dir, transform.forward);
                    if (dot > 0.99f)  // if the enemy is facing the target
                    {
                        bInAttackRange = true;
                    }
                    else
                    {
                        bInAttackRange = false;
                        HelpUtils.RotateTowards(transform, targetPlayer.transform.position, fROTATE_SPEED / 3f);
                    }
                }
            }
        }
        else if(_fDistance < fCLOSE_RANGE)
        {
            if (!bIsAttacking)
            {
                // Shell Itself
            }
        }
        else
        {
            bCanFollow = true;
            bInAttackRange = false;
        }
    }
    public void SetAnimations()
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
    public void Tackle(Vector3 _direction, float _fTackleSpeed)
    {
        rbody.MovePosition(transform.position + _direction * _fTackleSpeed * Time.fixedDeltaTime);
    }
}
