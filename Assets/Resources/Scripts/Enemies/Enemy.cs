using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType { METALON = 0, PLANT = 1 }
public class Enemy : MonoBehaviour, IHittable
{
    float fInvulnerableCounter;

    //const float fDISTANCE_TO_GROUND = 0.1f;
    const float fDISTANCE_TO_COLIS = 1.6f;
    const float fVISION_RANGE = 5f;

    protected const float fROTATE_SPEED = 240f;
    protected float fAttackRange = 4f;
    protected float fFollowRange = 80f;

    public float fMaxHitPoints;
    protected float fCurrentHitPoints;
    public int iCollisionDamage;
    protected Rigidbody rbody;
    protected Animator anim;
    protected static PlayerController targetPlayer;

    protected bool bIsAlive;

    protected bool bIsAttacking;
    protected bool bIsInvulnerable;
    protected bool bIsHit;
    protected bool bTargetFound;
    protected bool bInAttackRange;
    protected bool bCanFollow;
    protected bool bIsGrounded;
    public float fAttackWaitTime = 1f;
    protected float fAttackWaitTimeCounter;
    public float fOnCollisionKnockBackForce = 5f;
    public Vector3 tHeadOffset = new Vector3(0, 0.5f, 0);
    protected Vector3 startPosition;
    // Material Dissolve Variables
    private Material rndMaterial;
    float fMatDissolveAlpha = -0.8f;

    protected Movement moveScr;
    Collider maxTravelAreaCol;

    public void Initialize()
    {
        rbody = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        moveScr = GetComponent<Movement>();
        fCurrentHitPoints = fMaxHitPoints;
        if (!targetPlayer)
            targetPlayer = PlayerController.Instance;
        fAttackWaitTimeCounter = 0;
        bIsAlive = true;

        startPosition = transform.position;
        // Just Randomiaing the stats a bit

        rndMaterial = GetComponentInChildren<Renderer>().material;
    }
    public void Refresh()
    {
       // bIsGrounded = Grounded(transform, 0.4f);
        if (!bIsAlive)
        {
            DissolveOnDeath(0.6f);
        }
    }
    public void FixedRefresh()
    {

    }

    private void OnCollisionEnter(Collision _collision)
    {
        if (bIsAlive)
        {
            if (_collision.collider)
            {
                if (_collision.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    if (_collision.collider.GetComponent<IHittable>() != null)
                    {
                        bTargetFound = true;
                        moveScr.SetMovementActive(false);
                        _collision.collider.GetComponent<IHittable>().ApplyKnockback(transform.position, fOnCollisionKnockBackForce);
                        _collision.collider.GetComponent<IHittable>().ApplyDamage(iCollisionDamage);
                    }
                }
            }
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Movements and Target Checks
    public bool Grounded(Transform _transform, float _distanceToGround)
    {
        return Physics.Raycast(_transform.position + new Vector3(0, 0.2f, 0), Vector3.down, _distanceToGround);
    }
    public void FindingTarget()
    {
        RaycastHit hit;
        //Debug.DrawRay(transform.position + new Vector3(0, 0.3f, 0), transform.forward * fVISION_RANGE, Color.red);
        if (Physics.Raycast(transform.position + new Vector3(0, 0.3f, 0), transform.forward, out hit, fVISION_RANGE))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                bTargetFound = true;
                bCanFollow = true;
                moveScr.SetMovementActive(false);
                //bFollowingPath = false;
            }
        }
    }
    public void CalculateInvulnerability(float _fStunTime)
    {
        if (bIsInvulnerable)
        {
            fInvulnerableCounter += Time.deltaTime;
            if (fInvulnerableCounter >= anim.GetCurrentAnimatorStateInfo(0).length + _fStunTime)
            {
                bIsInvulnerable = false;
                bIsHit = false;
                bCanFollow = true;
                bInAttackRange = false;
                fInvulnerableCounter = 0;
            }
        }
    }
    public void CheckTargetInRange(float _fAttackRange, float _fTargetFollowRange)
    {
        fAttackWaitTimeCounter -= Time.deltaTime;
        // Out of Range
        if ((transform.position - targetPlayer.transform.position).sqrMagnitude >= _fTargetFollowRange)
        {
            if (!bIsInvulnerable && bTargetFound)
            {
                ResetBools();
            }
        }
        // In Attack Range
        else if ((transform.position - targetPlayer.transform.position).sqrMagnitude <= _fAttackRange)
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
        // In Non Attack Range
        else
        {
            bCanFollow = true;
            bInAttackRange = false;
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Damages and Health Effects
    public void ApplyKnockback(Vector3 _sourcePosition, float _pushForce)
    {
        if (!bIsInvulnerable)
        {
            Vector3 pushForce = transform.position - _sourcePosition;
            pushForce.y = 0;
            //transform.forward = -pushForce.normalized;
            rbody.AddForce(pushForce.normalized * _pushForce - Physics.gravity * 0.2f, ForceMode.Impulse);
        }
    }
    public void ApplyDamage(float _damage)
    {
        if (!bIsInvulnerable)
        {
            // Knockback(targetPlayer.transform.position, fPUSHBACKFORCE);
            bIsInvulnerable = true;
            bIsHit = true;
           // bTargetFound = true;
            bCanFollow = false;
            fCurrentHitPoints -= _damage;
        }
        if (fCurrentHitPoints <= 0)
        {
            Die();
        }
    }
    public float GetCurrentHealth()
    {
        return fCurrentHitPoints;
    }
    public void Die()
    {
        bIsAlive = false;
        StartCoroutine(HelpUtils.WaitForSeconds(OnDeathStuff, 1f));
        //Destroy(gameObject, 4f);
    }
    public void DestroyEnemy()
    {
        Destroy(gameObject, 5f);
    }
    public void Revive(Vector3 _position)
    {
        bIsAlive = true;
        fCurrentHitPoints = fMaxHitPoints;
        transform.position = _position;
    }
    void OnDeathStuff()
    {
        rbody.isKinematic = true;
        Collider[] _colliders = GetComponents<Collider>();
        for (int i = 0; i < _colliders.Length; i++)
        {
            _colliders[i].enabled = false;
        }
        StartCoroutine(HelpUtils.WaitForSeconds(delegate { gameObject.SetActive(false); }, 4f));
    }// MAKING IT KINEMETIC ON DEATH, AND REMOVING COLLIDERS
    void DissolveOnDeath(float _fDissolveSpeed)
    {
        fMatDissolveAlpha += _fDissolveSpeed * Time.deltaTime;
        rndMaterial.SetFloat("_alpha", fMatDissolveAlpha);
    }
    public bool IsInvulnerable()
    {
        return bIsInvulnerable;
    }
    public bool IsEnemyDead()
    {
        return !bIsAlive;
    }
    public void SetBoundary(Collider _boundaryCol)
    {
        maxTravelAreaCol = _boundaryCol;
    }
    public bool CanFollow()
    {
        return bCanFollow;
    }
    public void ResetBools()
    {
        StopAllCoroutines();
        bInAttackRange = false;
        bCanFollow = false;
        bTargetFound = false;
        if(moveScr)
            moveScr.SetMovementActive(true);
    }
    public void CheckWalkingArea(Vector3 _targetPosition) // Walk Area Check 
    {
        if (maxTravelAreaCol)
        {
            if (!maxTravelAreaCol.bounds.Contains(transform.position))
            {
                ResetBools();

                    HelpUtils.RotateTowards(transform, startPosition, fROTATE_SPEED);
                   // moveVector = new Vector3(_targetPos.x, 0, _targetPos.z);
                    rbody.MovePosition(transform.position + transform.forward * 2 * Time.fixedDeltaTime);
            }
        }
    }
}
