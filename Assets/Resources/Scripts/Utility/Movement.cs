using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovementType {IDLE, PATROLLING, MOVE_RANDOM, CIRCULAR_MOTION }
public class Movement : MonoBehaviour
{
    const float fDISTANCE_TO_GROUND = 0.2f;
    const float fDISTANCE_TO_COLIS = 1f;
    const float fROTATE_SPEED = 240f;

    Rigidbody rbody;
    public MovementType movementType;
    public float fSpeed;
    public float fWalkTime;
    public float fWaitTime;
    public float fRandomizeDirAfter;

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

    public float fRotateSpeed = 60f;

    private bool bDirReversing; // it is actually reversing direction if the npc reaches the end point
    private int iPatrolPos = 0;

    private bool bActive;

    // Walk Area Variables
    public float fMaxWalkingDistance = 60;
    Vector3 startPosition;
    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody>();
        startPosition = transform.position;
       
        bActive = true;
        SetKinemetic();

        fWaitTime += Random.Range(-0.5f, 1f);
        fWalkTime += Random.Range(-0.5f, 1f);
        fSpeed += Random.Range(-0.5f, 1f);
        fChangeDirectionTime = Random.Range(1f, fRandomizeDirAfter);
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
                case MovementType.CIRCULAR_MOTION:
                    CircularMotion(bReverseDirection);
                    break;
            }
        }
    }

    public void SetKinemetic()
    {
        if (movementType == MovementType.IDLE)
            rbody.isKinematic = true;
        else
        {
            rbody.isKinematic = false;
        }
    }
    public void Idle()
    {
        rbody.velocity = Vector3.zero;
    }
    public void MoveRandomly()
    {
        if (CheckWalkingArea())
        {
            FollowTarget(startPosition);
        }
        else
        {
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
        }
       
    }// TODO: Add Moving Area

    private float fWaitCounter = 0;
    private float fWalkCounter = 0;
    private float fChangeDirectionCounter = 0;
    private float fChangeDirectionTime = 0;
    public void CircularMotion(bool _bInverseDirection)
    {
        if (CheckWalkingArea(4))
        {
            LookTowards(startPosition, fROTATE_SPEED);
            rbody.MovePosition(transform.position + transform.forward * fSpeed * Time.fixedDeltaTime);
        }
        else
        {
            fWalkCounter += Time.deltaTime;
            if(fWalkCounter < fWalkTime)
            {
                bIsMoving = true;

                if (bRandomizePoints)
                {
                    fChangeDirectionCounter += Time.deltaTime;
                    if(fChangeDirectionCounter > fChangeDirectionTime)
                    {
                        fChangeDirectionCounter = 0;
                        bReverseDirection = !bReverseDirection;
                        fChangeDirectionTime = Random.Range(1, fRandomizeDirAfter);
                    }
                    //StartCoroutine(HelpUtils.WaitForSeconds(delegate { bReverseDirection = !bReverseDirection; }, Random.Range(1f, fRandomizeDirAfter)));
                }
                RotateInCircle(bReverseDirection);
                rbody.MovePosition(transform.position + transform.forward * fSpeed * Time.fixedDeltaTime);
            }
            else
            {
                bIsMoving = false;
                fWaitCounter += Time.deltaTime;
                if(fWaitCounter > fWaitTime)
                {
                    fWaitCounter = 0;
                    fWalkCounter = 0;
                }

            }
        }
    }
    private void RotateInCircle(bool _bReverseDirection)
    {
        Vector3 newAngle = transform.eulerAngles;

        if(_bReverseDirection)
            newAngle.y += fRotateSpeed * 1 * Time.deltaTime;
        else
            newAngle.y += fRotateSpeed * -1 * Time.deltaTime;

        transform.eulerAngles = newAngle;
    }
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
    public void FollowTarget(Vector3 _target, float _fRotSpeed = fROTATE_SPEED)
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
    public bool CheckWalkingArea(float _fWalkingAreaMinDivider = 3)
    {
      //  if (movementType != MovementType.PATROLLING)
        {
            if ((transform.position - startPosition).sqrMagnitude > fMaxWalkingDistance)
            {
                randomPosition = startPosition;
                bOutOfWalkingArea = true;
                return bOutOfWalkingArea;
            }
            else if((transform.position - startPosition).sqrMagnitude < fMaxWalkingDistance - (fMaxWalkingDistance / _fWalkingAreaMinDivider))
                bOutOfWalkingArea = false;

            return bOutOfWalkingArea;
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
        ResetBools();
        bActive = _bToSet;
    }
    public void ResetBools()
    {
        StopAllCoroutines();
        bCanMove = false;
        bIsMoving = false;
    }
}
