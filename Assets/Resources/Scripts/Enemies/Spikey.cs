using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Spikey : Enemy
{
    const float fCLOSE_RANGE = 5f;

    private float fVisionRadius = 2.2f;
    private float fVisionDistance = 4f;
    public float fTackleSpeed = 6f;
    float fSpinAttackMaxTime = 3f;
    float fSTUN_TIME = 2f; // this is extra time after the animation
    private bool bSpeedingUp;
    private bool bIsHidden;

    public VisualEffect vfxEffect;
    private void Start()
    {
        base.Initialize();
        fAttackRange = 65f;
        fFollowRange = 100f;
        bIsInvulnerable = true;
        vfxEffect.Stop();
    }
    public void Update()
    {
        base.Refresh();
        SetAnimations();

        if (bIsAlive)
        {
            if (bIsInvulnerable)
            {
                if (!bTargetFound)
                    FindTarget(fVisionRadius, fVisionDistance);
                else
                {
                    if (!bIsAttacking)
                    {
                        CheckTargetInRange(fAttackRange, fFollowRange);
                        CheckWalkingArea(startPosition);

                        if(!bIsHidden && bInAttackRange)
                            AttackMove();

                        if (!bIsHidden)
                            fAttackWaitTimeCounter -= Time.deltaTime;
                    }
                    else
                    {
                        CollisionCheck();
                    }
                }
            }
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
            if (bTargetFound && bIsInvulnerable && !bIsHidden)
            {
                if (!bInAttackRange)
                {
                    if (bCanFollow && !bIsAttacking)
                    {
                        moveScr.FollowTarget(targetPlayer.transform.position);
                    }
                } 
                if (bIsAttacking)
                {
                    Tackle(attackDirection, fTackleSpeed);
                }
            }
        }
    }

    Vector3 attackDirection;
    public void SpinningAttack()
    {
        rotateAngle = transform.forward;
        StartCoroutine(HelpUtils.WaitForSeconds(delegate { vfxEffect.Play(); rotateAngle = attackDirection; }, 0.6f));
        anim.SetBool("spinAttack", true);
        attackDirection = (targetPlayer.transform.position - transform.position).normalized;      
    }

    public void AttackMove()
    {
        if (fAttackWaitTimeCounter < 0)
        {
            bCanFollow = false;
            bIsAttacking = true;
            SpinningAttack();
           // StopAllCoroutines();
            StartCoroutine(StopAttackAfter(fSpinAttackMaxTime));
            fAttackWaitTimeCounter = fAttackWaitTime;
        }
    }
    public override void CheckTargetInRange(float _fAttackRange, float _fTargetFollowRange, float _fRotateSpeed = 160, float _fAccuracy = 0.99f)
    {
        float _fDistance = (transform.position - targetPlayer.transform.position).sqrMagnitude;

        if (_fDistance >= _fTargetFollowRange)
        {
            if (bTargetFound)
            {
                ResetBools();
            }
        }
        else if (_fDistance <= _fAttackRange && _fDistance >= fCLOSE_RANGE)
        {
            if (bIsHidden)
            {
                bIsHidden = false;
            }
            bCanFollow = false;
            moveScr.SetIsMoving(false);
           // if (!bIsInvulnerable)
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
                        HelpUtils.RotateTowards(transform, targetPlayer.transform.position, _fRotateSpeed);
                    }
                }
            }
        }
        else if(_fDistance < fCLOSE_RANGE) // when too close to player
        {
            if (!bIsAttacking)
            {
                fAttackWaitTimeCounter = fAttackWaitTime;
                anim.SetTrigger("hide");
                moveScr.LookTowards(targetPlayer.transform.position, fROTATE_SPEED);
                bIsHidden = true;
                bInAttackRange = false;
                bIsAttacking = false;
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
            anim.SetBool("hide", bIsHidden);
        }
        else
        {
            anim.SetTrigger("isDead");
        }
    }
    Vector3 rotateAngle = new Vector3(0,0,0);
    public void Tackle(Vector3 _direction, float _fTackleSpeed)
    {
        rotateAngle = Quaternion.AngleAxis(120, Vector3.up) * rotateAngle;

        if (bSpeedingUp)
            rbody.MovePosition(transform.position + _direction * _fTackleSpeed * Time.fixedDeltaTime);
        else
            rbody.MovePosition(transform.position + _direction * 2 * Time.fixedDeltaTime);

        //vfxEffect.gameObject.SetActive(true);
        
    }
    public void CollisionCheck()
    {
        if (HelpUtils.CheckAheadForColi(transform.position + moveScr.tHeadOffset, rotateAngle, 0.85f, LayerMask.GetMask("Default", "Ground", "Weapon", "Player", "Tree"), true))
        {
            if(bSpeedingUp)
                KnockedUpsideDown();
        }
    }
    public void KnockedUpsideDown()
    {
        vfxEffect.Stop();
        anim.SetBool("getKnockedUp", true);
        bIsInvulnerable = false;
        bInAttackRange = false;
        StartCoroutine(DoKnockupReset(fSTUN_TIME));
        ApplyKnockback(transform.position + transform.forward, 4f);
    }
    public override void ApplyKnockback(Vector3 _sourcePosition, float _pushForce)
    {
        if (!bTargetFound)
        {
            bTargetFound = true;
            moveScr.SetMovementActive(false);
        }

        Vector3 pushForce = transform.position - _sourcePosition;
        pushForce.y = 0;
        rbody.AddForce(pushForce.normalized * (_pushForce / 2) - Physics.gravity * 0.2f, ForceMode.Impulse);
    }
    public override void ApplyDamage(float _damage)
    {
        if (!bIsInvulnerable && !bIsHit)
        {
            fCurrentHitPoints -= _damage;
        }
        if (fCurrentHitPoints <= 0)
        {
            Die();
        }
    }
    IEnumerator DoKnockupReset(float _fTimeToWait)
    {
        bIsHit = false;
        yield return new WaitForSeconds(_fTimeToWait);
        bIsAttacking = false;
        bSpeedingUp = false;
        bIsHit = true;
        bIsHidden = true;

        anim.SetBool("spinAttack", bIsAttacking);
        anim.SetBool("getKnockedUp", false); // sida ho jve

        yield return new WaitForSeconds(_fTimeToWait);
        bIsInvulnerable = true;
        bIsHidden = false;
    }
    IEnumerator StopAttackAfter(float _fTimeToWait)
    {
        bInAttackRange = false;
        yield return new WaitForSeconds(_fTimeToWait);
        vfxEffect.Stop();
        bIsAttacking = false;
        bSpeedingUp = false;
        anim.SetBool("spinAttack", bIsAttacking);
    }
    public void SetSpeedUpBool(int _bToSet)
    {
        bSpeedingUp = _bToSet == 0 ? false : true;
    }

    void DissolveOnDeath(float _fDissolveSpeed)
    {
        fMatDissolveAlpha += _fDissolveSpeed * Time.deltaTime;
        rndrMaterial.SetFloat("_alpha", fMatDissolveAlpha);
    }

}
