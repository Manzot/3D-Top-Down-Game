using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovementType {IDLE, PATROLLING, MOVE_RANDOM }
public class Movement : MonoBehaviour
{
    const float fDISTANCE_TO_GROUND = 0.2f;
    const float fDISTANCE_TO_COLIS = 1f;
    const float fROTATE_SPEED = 240f;

    Rigidbody rbody;
    Animator anim;
    public MovementType movementType;
    public float fSpeed;
    public float fWalkTime;
    public float fWaitTime;

    private bool bCanMove;
    private bool bIsMoving;
    private Vector3 randomPosition;
    private Vector3 moveVector;
    private bool bOutOfWalkingArea;

    private bool bCanRotate;
    // Patrolling
    private Vector3 lastDirection;

    public Transform[] tPatrolPoints;
    public bool bReverseDirection; // it is to enable or disable reverse direction
    public bool bRandomizePoints;

    private bool bDirReversing; // it is actually reversing direction if the npc reaches the end point
    private int iPatrolPos = 0;

    private bool bActive;

    // Walk Area Variables
    public float fMaxWalkingDistance = 60;
    Vector3 startPosition;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rbody = GetComponent<Rigidbody>();
        startPosition = transform.position;
        bActive = true;

        fWaitTime += Random.Range(-0.5f, 1f);
        fWalkTime += Random.Range(-0.5f, 1f);
        fSpeed += Random.Range(-0.5f, 1f);
    }
    private void OnEnable()
    {
        //TODO: Later make this for initialize enemies on enabling and also randomize move and wait time at bit from the given value
        bCanMove = true;
        StartCoroutine(HelpUtils.WaitForSeconds(delegate { bCanMove = false; }, 1f));
        bActive = true;
    }

    private void FixedUpdate()
    {
        if (bActive)
        {
            switch (movementType)
            {
                case MovementType.MOVE_RANDOM:
                    MoveRandomly();
                    break;
                case MovementType.PATROLLING:
                    Patrol();
                    break;
            }
        }
    }

    public void Idle()
    {
        rbody.velocity = Vector3.zero;
    }
    public void MoveRandomly()
    {
        if (bOutOfWalkingArea)
            rbody.MovePosition(transform.position + transform.forward * fSpeed * Time.fixedDeltaTime);

        if (!bCanMove)
        {
            StopAllCoroutines();
            StartCoroutine(GetRandomDirection());
            bCanMove = true;
        }
        else
        {
            if (bIsMoving)
            {
                if (HelpUtils.CheckAheadForColi(transform, fDISTANCE_TO_COLIS))
                {
                    bIsMoving = false;
                    StartCoroutine(HelpUtils.WaitForSeconds(delegate { bCanMove = false; }, fWaitTime / 2)); ;
                }
                
                rbody.MovePosition(transform.position + transform.forward * fSpeed * Time.fixedDeltaTime);
            }
            else
            {
                if (bCanRotate)
                {
                   LookTowards(randomPosition, fROTATE_SPEED);
                }
            }
        }
    }// TODO: Add Moving Area
    public void Patrol()
    {
        if (bIsMoving)
        {
            lastDirection = (tPatrolPoints[iPatrolPos].position - transform.position).normalized;
            lastDirection.y = 0;

            if ((transform.position - tPatrolPoints[iPatrolPos].position).sqrMagnitude <= 1f)
            {
                if (!bRandomizePoints)
                {
                    if (bDirReversing)
                        iPatrolPos--;
                    else
                        iPatrolPos++;

                    if (iPatrolPos >= tPatrolPoints.Length)
                    {
                        if (bReverseDirection)
                        {
                            iPatrolPos = tPatrolPoints.Length - 2;
                            bDirReversing = true;
                        }
                        else
                            iPatrolPos = 0;
                    }
                    else if (iPatrolPos <= 0)
                    {
                        iPatrolPos = 0;
                        if (bDirReversing)
                        {
                            bDirReversing = false;
                        }
                    }
                    if (fWaitTime > 0.3f)
                    {
                        bIsMoving = false;
                        StartCoroutine(HelpUtils.ChangeBoolAfter((bool b) => { bIsMoving = b; }, true, fWaitTime));
                    }
                    else
                        transform.LookAt(tPatrolPoints[iPatrolPos].position);
                }
                else
                {
                    iPatrolPos = Random.Range(0, tPatrolPoints.Length);
                    if (fWaitTime > 0.3f)
                    {
                        bIsMoving = false;
                        StartCoroutine(HelpUtils.ChangeBoolAfter((bool b) => { bIsMoving = b; }, true, fWaitTime));
                    }
                    else
                        transform.LookAt(tPatrolPoints[iPatrolPos].position);
                }
               
            }
            moveVector = new Vector3(lastDirection.x, 0, lastDirection.z);
            rbody.MovePosition(transform.position + moveVector * fSpeed * Time.fixedDeltaTime);
        }
        else
        {
            HelpUtils.RotateTowards(transform, tPatrolPoints[iPatrolPos].position, fROTATE_SPEED);
        }
    }

    IEnumerator GetRandomDirection()
    {
        randomPosition = transform.position + new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));// Random.insideUnitSphere * 100;
        CheckWalkingArea();
        if ((randomPosition - startPosition).sqrMagnitude <= fMaxWalkingDistance) // if the random vector is out of walking area then reassign the vector
        {
            bCanMove = true;
            bIsMoving = true;
        }
        else
        {
            randomPosition = startPosition;
        }
        yield return new WaitForSeconds(fWalkTime);
        bIsMoving = false;
        yield return new WaitForSeconds(fWaitTime / 3);
        bCanRotate = true;
        yield return new WaitForSeconds(fWaitTime / 3);
        bCanRotate = false;
        yield return new WaitForSeconds(fWaitTime / 3);
        bCanMove = false;
    }
    public void FollowTarget(Vector3 _target)
    {
        bIsMoving = true;
        LookTowards(_target, fROTATE_SPEED);
        Seek(_target);
    }
    public void Seek(Vector3 _target)
    {
        Vector3 _movePos = (_target - transform.position).normalized;
        rbody.MovePosition(transform.position + _movePos * fSpeed * Time.fixedDeltaTime);
    }
    public void LookTowards(Vector3 _target, float _rotationSpeed)
    {
        Vector3 _directionToPlayer = (_target - transform.position).normalized;
        _directionToPlayer.y = 0;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(_directionToPlayer), _rotationSpeed * Time.fixedDeltaTime);
    }
    public void CheckWalkingArea()
    {
      //  if (movementType != MovementType.PATROLLING)
        {
            if ((transform.position - startPosition).sqrMagnitude > fMaxWalkingDistance)
            {
                bOutOfWalkingArea = true;
                randomPosition = startPosition;
            }
            else
                bOutOfWalkingArea = false;
        }
    }
    public bool IsMoving()
    {
        return bIsMoving;
    }
    public void SetIsMoving(bool _bToSet)
    {
        bIsMoving = _bToSet;
    }
    public void SetMovementActive(bool _bToSet)
    {
        bActive = _bToSet;
    }
}
