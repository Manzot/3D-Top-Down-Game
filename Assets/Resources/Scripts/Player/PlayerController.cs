﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour, IHittable, ISaveable
{
    protected static PlayerController instance;
    public static PlayerController Instance { get { return instance; } }

    structGameSavePlayer structPlayerSaveData;

    const float fHEAD_OFFSET = 1f;
    const float fNPC_DISTANCE_CHECK = 0.8f;
    const float fDISTANCE_TO_GROUND = 0.25f;
    const float fINVULNERABILITY_TIME = 2f;
    const float fSTUN_TIME = 0.5f;
    const float fSPRINT_STAMINA_COST = 10f; // is multipleid by deltaTime
    const float fATTACK_STAMINA_COST = 5f;
    const float fSHIELD_STAMINA_COST = 10f;
    const float fSTAMINA_RECOVER_START_TIME = 0.1f;
    const float fSTAMINA_RECOVERY_RATE = 25f; // is multipleid by deltaTime
    const float fSPEED_DIVISION = 0.6f;

    Rigidbody rbody;
    Animator anim;

    public float fMaxHitPoints;
    [HideInInspector]
    public float fCurrentHitPoints;
    public float fMaxStamina;
    [HideInInspector]
    public float fCurrentStamina;
    float fStaminaTimeCounter = 0;
    

    public float fSpeed;
    public float fJumpForce;
    public float fSpeedMultiplier = 1.5f;
    float horizontal;
    float vertical;
    Vector3 lastFacinDirection;
    Vector3 movementVector;

    private bool bStaminaUsed;
    private bool bIsAlive;
    private bool bIsSprinting;
    private bool bCanSprint = true;
    private bool bIsShielding;
    private bool bPrimaryWeaponEquipped;
    private bool bIsAttacking;
    private bool bIsInteracting;
    private bool bIsInvulnerable;
    private bool bIsStun;
    private bool bIsOnSlope;
    private bool bIsGrounded;
    private bool bCanAttack;
   
    //Keys Input
    private bool bJumpPressed;
    private bool bSprintPressed;
    private bool bAttackPressed;
    private bool bShieldPressed;
    private bool bInteractPressed;
    private bool bSheathWeaponPressed;

    int iAttackCombo = -1;

    public UnityEvent OnReciveDamageUI;
    public UnityEvent OnStaminaChangeUI;

    private Inventory playerInventory;
    public int iStartInventorySize;

    PlayerEquipmentManager pEquimentManager;

    // Experimental floats
    //public float fMinDepth;
   // public float fMaxDepth;
  //  public float fMultUp;
    //public float fMultDown;

    RaycastHit hitGround;
    void Awake()
    {
        instance = this;
        rbody = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        fCurrentHitPoints = fMaxHitPoints;
        fCurrentStamina = fMaxStamina;
        bIsAlive = true;
        playerInventory = new Inventory(iStartInventorySize);
        pEquimentManager = GetComponent<PlayerEquipmentManager>();
        bCanSprint = true;
       // hightlightMat = rend.materials[1];
    }
    private void Start()
    {

    }
    void Update()
    {
        if (GameController.inPlayMode)
        {
            if (bIsAlive)
            {
                PlayerAnimations();
                if (!bIsStun)
                {
                    PlayerInputs();
                    CheckGrounded();
                    StaminaCheck();
                    UseShield();
                    SwordAttacks();
                    DrawSheathPrimaryWeapon();

                    if (bIsGrounded)
                    {
                        CheckAheadForColliders();
                        CheckOnSlope(); // this controlling players y velocity for now
                    }
                }
            }
        }
    }
    void FixedUpdate()
    {
        if (GameController.inPlayMode)
        {
            if (bIsAlive && !bIsStun)
            {
                if (bIsGrounded)
                {
                    Jump();
                    DirectionalMovement();
                }
                else
                    JumpControlling();
            }
        }
    }
    void DirectionalMovement()
    {
        //if (bMovementEnabled)
        {
            if (horizontal != 0 || vertical != 0)
            {
                if (bSprintPressed && !bIsAttacking)
                {
                    if (!bIsShielding && bCanSprint)
                    {
                        if (fCurrentStamina > fSPRINT_STAMINA_COST * Time.deltaTime)
                        {
                            fCurrentStamina -= fSPRINT_STAMINA_COST * Time.deltaTime;
                            bIsSprinting = true;
                        }
                        else
                        {
                            bIsSprinting = false;
                            bCanSprint = false;
                        }
                    }
                }
                else
                {
                    if (fCurrentStamina > 5f)
                    {
                        bCanSprint = true;
                    }
                    bIsSprinting = false;
                }

                lastFacinDirection = new Vector3(horizontal, 0, vertical);

                if (!bIsAttacking)
                {
                    if (lastFacinDirection != Vector3.zero)
                    {
                        transform.forward = lastFacinDirection;
                    }
                }

                //It is for keeping the direction while shielding
                //if (!bIsShielding)
                //{
                //    if (Mathf.Abs(horizontal) > 0.2f || Mathf.Abs(vertical) > 0.2f)
                //        lastFacinDirection = new Vector3(horizontal, 0f, vertical);
                //}
                ////// mOVEMENT cONTROLLER //////////////////////////

                movementVector = new Vector3(horizontal, 0, vertical).normalized;

                if (movementVector != Vector3.zero)
                {
                    if (bIsSprinting)
                        rbody.velocity = (movementVector * fSpeed * fSpeedMultiplier * Time.fixedDeltaTime) + HelpUtils.VectorZeroWithY(rbody); //rbody.AddForce(movementVector * fSpeed * fSpeedMultiplier * Time.fixedDeltaTime, ForceMode.VelocityChange); ////;
                    else if (bIsShielding || bIsAttacking)
                        rbody.velocity = (movementVector * fSPEED_DIVISION * fSpeed * Time.fixedDeltaTime) + HelpUtils.VectorZeroWithY(rbody); //rbody.AddForce(movementVector * fSpeed * fSPEED_DIVISION * Time.fixedDeltaTime, ForceMode.VelocityChange); ////
                    else
                        rbody.velocity = (movementVector * fSpeed * Time.fixedDeltaTime) + HelpUtils.VectorZeroWithY(rbody);// rbody.AddForce(movementVector * fSpeed * Time.fixedDeltaTime, ForceMode.VelocityChange);

                    //rbody.MovePosition(transform.position + movementVector * fSpeed * Time.fixedDeltaTime);
                }
                ///////////////////////////////////////////////////
            }
            else
            {
                rbody.velocity = HelpUtils.VectorZeroWithY(rbody);
                bIsSprinting = false;
            }
        }
    }
    void UseShield()
    {
        if (pEquimentManager.shield != null)
        {
            if (bShieldPressed)
            {
                if(horizontal == 0 && vertical == 0)
                    rbody.velocity = Vector3.zero;

                if (fCurrentStamina > fSHIELD_STAMINA_COST)
                {
                    bIsShielding = true;
                    bIsSprinting = false;
                }
                else
                {
                    bIsShielding = false;
                }
            }
            else
                bIsShielding = false;
        }
    }
    void DrawSheathPrimaryWeapon()
    {
        if (bAttackPressed)
        {
            if (pEquimentManager.primaryWeapon != null)
            {
                if (!bPrimaryWeaponEquipped)
                {
                   // if (bAttackPressed)
                    {
                        bPrimaryWeaponEquipped = true;
                    }
                }
            }
        }
        else if (bSheathWeaponPressed)
        {
            if (bPrimaryWeaponEquipped)
            {
                SheathWeapon();
            }
        }
    }
    void SheathWeapon(bool _bIsRemoved = false)
    {
        //anim.SetBool("weapon_equipped", bDrawPrimaryWeapon);
        bPrimaryWeaponEquipped = _bIsRemoved;

        bPrimaryWeaponEquipped = false;
        bIsAttacking = false;
        iAttackCombo = -1;
    }
    void SwordAttacks()
    {
        if (bPrimaryWeaponEquipped)
        {
            if (bAttackPressed)
            {
                iAttackCombo++;
                if (iAttackCombo >= 0 && !bIsAttacking && fCurrentStamina > fATTACK_STAMINA_COST)
                {
                    bIsAttacking = true;
                    bSprintPressed = false;
                    fWaitTime = 0;
                    fCurrentStamina -= fATTACK_STAMINA_COST;
                    anim.SetTrigger("attack1");
                }

            }
            if(iAttackCombo >= 0)
                WaitForAttack();
        }
    }
    float fWaitTime = 0;
    void WaitForAttack()
    {
        fWaitTime += Time.deltaTime;
        if(fWaitTime >= 0.4f)
        {
            bIsAttacking = false;
            fWaitTime = 0;
            iAttackCombo = -1;
        }
    }
    public void Jump()
    {
        if (bJumpPressed)
        {
            if(fCurrentStamina > 11f)
            {
                if (bIsOnSlope) // when on slope 
                {
                    if (rbody.velocity != Vector3.zero)
                        rbody.AddForce(new Vector3(0, fJumpForce * 2f, 0), ForceMode.Impulse); // on slope increaing jump force
                    else
                        rbody.AddForce(new Vector3(0, fJumpForce, 0), ForceMode.Impulse);

                    bJumpPressed = false;
                }
                else // normal jumping on ground
                {
                    if (rbody.velocity.y <= 0 && rbody.velocity.y >= -0.1f)
                        rbody.AddForce(new Vector3(0, fJumpForce, 0), ForceMode.Impulse);
                    bJumpPressed = false;
                }
            }
            fCurrentStamina -= 10f;
        }
        bJumpPressed = false;
    }
    void JumpControlling()
    {
        if (rbody.velocity.x != 0 || rbody.velocity.z != 0)
        {
            if (rbody.velocity.y >= fJumpForce)
            {
                rbody.velocity = new Vector3(rbody.velocity.x, fJumpForce / 1.2f, rbody.velocity.z);
            }
        }
    }
    public void DisablePlayerMoveActions()
    {
        rbody.velocity = new Vector3(0, 0, 0);
        anim.SetTrigger("idle");
        anim.SetFloat("moveVelocity", 0f);
       // anim.SetBool("isShielding", false);
    }
    public void CheckGrounded()
    {
        //bIsGrounded = Physics.Raycast(transform.position + new Vector3(0, 0.2f, 0), Vector3.down, out hitGround, fDISTANCE_TO_GROUND, LayerMask.GetMask("Ground","Slope","Default"));
        bIsGrounded = Physics.CheckSphere(transform.position, fDISTANCE_TO_GROUND, LayerMask.GetMask("Ground", "Default", "Slope"));
    }
    public void CheckAheadForColliders()
    {
        if (bInteractPressed)
        {
            Collider[] _hitColliders = Physics.OverlapSphere(transform.position + transform.forward, 1f, LayerMask.GetMask("Npc", "Item"));
            foreach (var _collider in _hitColliders)
            {
                Debug.Log(_collider.gameObject.name);
                ItemContainer _itemContainer = _collider.transform.GetComponent<ItemContainer>();
                NPC _npc = _collider.transform.GetComponent<NPC>();
               
                if (_itemContainer)
                {
                    CheckForItems(_itemContainer);
                    break;
                }
                else if (_npc)
                {
                    CheckForNPC(_npc);
                    break;
                }
            }
        }
    }
    public void CheckForNPC(NPC _collidedNPC)
    {
        if (!bIsInteracting)
        {
            if (_collidedNPC.SetDialog())
            {
                bIsInteracting = true;
                DisablePlayerMoveActions();
                _collidedNPC.LookAtTarget(transform);
                StartCoroutine(HelpUtils.RotateTowardsTarget(transform, _collidedNPC.transform));
                GameController.inPlayMode = false;
            }
        }
        else
            bIsInteracting = false;
    }
    private void CheckForItems(ItemContainer _collidedItemContainer)
    {
        if (_collidedItemContainer.item.bIsStackable)
        {
            bool _bItemAlreadyInventory = false;
            for (int i = 0; i < playerInventory.lstItems.Count; i++)
            {
                if (playerInventory.lstItems[i].sItemName == _collidedItemContainer.item.sItemName)
                {
                    if (playerInventory.lstItems[i].iQuantity < playerInventory.lstItems[i].iStackLimit)
                    {
                        playerInventory.lstItems[i].SetItemQuantity(playerInventory.lstItems[i].iQuantity + 1); // increase stack amount by 1
                        _bItemAlreadyInventory = true;
                        _collidedItemContainer.DestroySelf();
                        break;
                    }
                }
            }
            if (!_bItemAlreadyInventory)
            {
                if (playerInventory.lstItems.Count < playerInventory.iInventorySize)
                {
                    Item _newItem = ScriptableObject.CreateInstance<Item>();// new Item(_item);
                    _newItem.SetItem(_collidedItemContainer.item);
                    playerInventory.AddItem(_newItem);
                    _collidedItemContainer.DestroySelf();
                }
                else
                {
                    Debug.Log("Inventory is Full");
                }
            }
        }
        else
        {
            if (playerInventory.lstItems.Count < playerInventory.iInventorySize)
            {
                Item _newItem = ScriptableObject.CreateInstance<Item>();// new Item(_item);
                _newItem.SetItem(_collidedItemContainer.item);
                playerInventory.AddItem(_newItem);
                _collidedItemContainer.DestroySelf();
            }
            else
            {
                Debug.Log("Inventory is Full");
            }
        }
        PopupUIManager.Instance.inventoryPopup.UpdateInventoryUI(playerInventory);
    }
    public void CheckOnSlope()
    {
        //if (hitGround.normal != Vector3.up)
        //    bIsOnSlope = true;
        //else
        //    bIsOnSlope = false;

        if (rbody.velocity != Vector3.zero)
        {
            if (rbody.velocity.y > 0.1f && rbody.velocity.y < fJumpForce - 1) // going up, 0.1f is if the velocity is increasing means there is a slope
            {
                rbody.velocity = new Vector3(rbody.velocity.x, 0.1f, rbody.velocity.z); // 0.1f is setting velocity.y to slope travel speed
            }
            if (rbody.velocity.y < -0.1f) // going down, -0.2f means if the player is moving down with atleast -0.2f velovity.y
            {
                rbody.velocity = new Vector3(rbody.velocity.x, -2.5f, rbody.velocity.z); // then setting velovity.y to -2.5f so that there are no bounces
            }
        }
    }

    /// Health System
    public void ApplyKnockback(Vector3 _sourcePosition, float _pushForce)
    {
        // Stops for a hit time to play the hit animation and then move
        Stun();
        if (!bIsInvulnerable)
        {
            fCurrentStamina -= _pushForce * 1.5f;
            OnStaminaChangeUI.Invoke();
            Vector3 pushForce = transform.position - _sourcePosition;
            pushForce.y = 0;
            //transform.forward = -pushForce.normalized;
            rbody.AddForce(pushForce.normalized * _pushForce, ForceMode.Impulse);
        }
    }
    public void ApplyDamage(float _damage)
    {
        if (!bIsInvulnerable)
        {
            bIsInvulnerable = true;
            fCurrentHitPoints -= _damage;
            if (fCurrentHitPoints <= 0)
                Die();
            else if(_damage > 0)
                anim.SetTrigger("isHit");
            OnReciveDamageUI.Invoke();
            StartCoroutine(HelpUtils.ChangeBoolAfter((bool b) => { bIsInvulnerable = b; bIsAttacking = b; }, false, fINVULNERABILITY_TIME));
        }
    }
    public void Die()
    {
        if(bIsAlive)
            anim.SetTrigger("isDead");

        bIsAlive = false;
        rbody.isKinematic = true;
        //GetComponent<Collider>().isTrigger = true;
        //TODO: Add a beath animation and remove collisions
        //gameObject.SetActive(false); // deactivating it or destroying can coz some loading bugs
    }
    public void HealthCheck()
    {
        if (fCurrentHitPoints > fMaxHitPoints)
            fCurrentHitPoints = fMaxHitPoints;

    }
    public void StaminaCheck()
    {
        if (fCurrentStamina < 0)
        {
            fCurrentStamina = 0;
        }

        if (bIsSprinting || bIsAttacking)
        {
            OnStaminaChangeUI.Invoke();
            bStaminaUsed = true;
        }
        if (!bCanSprint)
        {
            if ((fCurrentStamina >= fMaxStamina / 2f))
            {
                bCanSprint = true;
            }
        }

        if (!bIsSprinting && !bIsAttacking)
        {
            if ((int)fCurrentStamina <= (int)fMaxStamina + 10)
            {
                OnStaminaChangeUI.Invoke();
                if (bStaminaUsed)
                {
                    fStaminaTimeCounter += Time.deltaTime;
                    if (fStaminaTimeCounter >= fSTAMINA_RECOVER_START_TIME)
                    {
                        bStaminaUsed = false;
                        fStaminaTimeCounter = 0;
                    }
                }
                if (fStaminaTimeCounter <= 0)
                {
                    if (bIsShielding)
                    {
                        fCurrentStamina += (fSTAMINA_RECOVERY_RATE / 5) * Time.deltaTime;
                    }
                    else
                    {
                        fCurrentStamina += fSTAMINA_RECOVERY_RATE * Time.deltaTime;
                    }

                    if (fCurrentStamina > fMaxStamina)
                    {
                        fCurrentStamina = fMaxStamina;
                    }
                }
            }
        }
    }
    public void Stun()
    {
        //rbody.velocity = HelperFunctions.VectorZero(rbody);
        anim.SetFloat("moveVelocity", 0f);
        bIsStun = true;
        if (bIsStun)
        {
            StartCoroutine(HelpUtils.ChangeBoolAfter((bool b) => { bIsStun = b; }, false, fSTUN_TIME)); // Replace stun time with stun animation
        }
    }

    // Inventory
    public void UpdateInventory(Inventory _inventory)
    {
        // TODO: Update Player about the sword that its gone now, make make sword to null;
        playerInventory = _inventory;
        PopupUIManager.Instance.inventoryPopup.UpdateInventoryUI(playerInventory);
    }
    public Inventory GetInventory()
    {
        return playerInventory;
    }
    /// Bools Getter And Setters
    public void PlayerInputs()
    {
       // if (!bIsAttacking)
        {
            if (bIsGrounded && !bIsAttacking)
            {
                horizontal = Input.GetAxis("Horizontal");
                vertical = Input.GetAxis("Vertical");
            }
            bSheathWeaponPressed = Input.GetKeyDown(KeyCode.T);
            if (Input.GetButtonDown("Sprint"))
            {
                bSprintPressed = true;
            }
            if (Input.GetButtonUp("Sprint"))
            {
                bSprintPressed = false;
            }
            bShieldPressed = Input.GetButton("Shield");
            bAttackPressed = Input.GetButtonDown("Attack");
            bInteractPressed = Input.GetButtonDown("Interact");
        
            if(Input.GetKeyDown(KeyCode.Space))
                bJumpPressed = true;
        }
    }
    public void PlayerAnimations()
    {
        anim.SetBool("isSprinting", bIsSprinting);
        anim.SetBool("shield_equipped", bIsShielding);
        anim.SetBool("weapon_equipped", bPrimaryWeaponEquipped);
        anim.SetFloat("moveVelocity", rbody.velocity.sqrMagnitude);
    }
    public bool IsInteracting()
    {
        return bIsInteracting;
    }
    public bool IsInvulnerable()
    {
        return bIsInvulnerable;
    }
    public bool IsAlive()
    {
        return bIsAlive;
    }
    public bool IsAttacking()
    {
        return bIsAttacking;
    }
    public bool IsUsingShield()
    {
        return bIsShielding;
    }
    public bool PrimaryWeaponEquipped()
    {
        return bPrimaryWeaponEquipped;
    }
    public void SetPrimaryWeaponEquipped(Item _swordToEquip)
    {
        // TODO: Set player animtaion to deafult on changing new weapon or place the new weapon in its hand according to drawWeaponBool
        if (_swordToEquip != null)
        {
            SheathWeapon(true);
            ItemContainer _newWeapon = Instantiate(_swordToEquip.GetItemPrefab(), pEquimentManager.phPrimaryWeaponUnEquipped);
            _newWeapon.SetItemEquipable();
            pEquimentManager.SetPrimaryWeapon(_newWeapon.gameObject);
        }
        else // when there is no weapon equipped
        {
            SheathWeapon(true);
            pEquimentManager.SetPrimaryWeapon(null);
        }
    }
    public void SetSecondaryWeaponEquipped(Item _swordToEquip)
    {


    }
    public void SetShieldEquipped(Item _shieldToEquip)
    {
        if (_shieldToEquip != null)
        {
            ItemContainer _newShield = Instantiate(_shieldToEquip.GetItemPrefab(), pEquimentManager.phShieldUnEquipped);
            _newShield.SetItemEquipable();
            pEquimentManager.SetShield(_newShield.gameObject);
        }
        else
            pEquimentManager.SetShield(null);
    }
    public void SetIsAttacking(bool _bIsAttacking)
    {
       // anim.ResetTrigger("attack1");
        bIsAttacking = _bIsAttacking;
    }
  
    public Animator GetAnimator()
    {
        return anim;
    }
    ///// Saving And Loading Data
    public void SaveAllData(SaveData _saveData)
    {
        structGameSavePlayer _playerSaveData = new structGameSavePlayer();
        _playerSaveData.fCurrentHitPoints = instance.fCurrentHitPoints;
        _playerSaveData.fCurrentStamina = instance.fCurrentStamina;
        _playerSaveData.tPosition = new float[3] { instance.transform.position.x, instance.transform.position.y, instance.transform.position.z };
        _playerSaveData.tRotation = new float[3] { instance.transform.rotation.eulerAngles.x, instance.transform.rotation.eulerAngles.y, instance.transform.rotation.eulerAngles.z};
       // _playerSaveData.tRotation = new float[3] { lastFacinDirection.x, lastFacinDirection.y, lastFacinDirection.z };
        _playerSaveData.playerInventory = instance.playerInventory.SaveInventoryStats();

        _saveData.playerSaveData = _playerSaveData;
    }
    public void LoadSaveData(SaveData _saveData)
    {
        structPlayerSaveData = new structGameSavePlayer();
        structPlayerSaveData = _saveData.playerSaveData;

        instance.fCurrentHitPoints = structPlayerSaveData.fCurrentHitPoints;
        instance.fCurrentStamina = structPlayerSaveData.fCurrentStamina;

        instance.transform.position = new Vector3(structPlayerSaveData.tPosition[0], structPlayerSaveData.tPosition[1], structPlayerSaveData.tPosition[2]); // 0 = x, 1 = y, 2 = z
        instance.lastFacinDirection = new Vector3(structPlayerSaveData.tRotation[0], structPlayerSaveData.tRotation[1], structPlayerSaveData.tRotation[2]); // 0 = x, 1 = y, 2 = z
        instance.transform.rotation = Quaternion.Euler(instance.lastFacinDirection);
        instance.playerInventory = new Inventory(structPlayerSaveData.playerInventory.iInventorySize);
        instance.playerInventory.LoadInventoryStats(structPlayerSaveData.playerInventory);
        PopupUIManager.Instance.inventoryPopup.UpdateInventoryUI(instance.playerInventory);
        instance.EquipItems();

        instance.OnReciveDamageUI.Invoke();
        instance.OnStaminaChangeUI.Invoke();
    }
    public void EquipItems()
    {
        for (int i = 0; i < playerInventory.lstItems.Count; i++)
        {
            if (playerInventory.lstItems[i].bIsEquipable)
            {
                if (playerInventory.lstItems[i].bIsEquipped)
                {
                    switch (playerInventory.lstItems[i].eType)
                    {
                        case ItemType.Shield:
                            SetShieldEquipped(playerInventory.lstItems[i]);
                            break;
                        case ItemType.PrimaryWeapon:
                            SetPrimaryWeaponEquipped(playerInventory.lstItems[i]);
                            break;
                        case ItemType.SecondaryWeapon:
                            SetSecondaryWeaponEquipped(playerInventory.lstItems[i]);
                            break;
                    }
                }
            }
        }
    }
    public void SwitchMaterial(string sMatName)
    {
        if (sMatName == "switch")
        {
            pEquimentManager.HighlightMaterial();
        }
        else
        {
            pEquimentManager.SetDefaultMaterials();
        }
    }
    // Gizmos
    private void OnDrawGizmos()
    {
        // Gizmos.DrawSphere(transform.position + transform.forward, 1f);
        // Gizmo for below Function
        Gizmos.DrawSphere(transform.position, fDISTANCE_TO_GROUND);
    }
}

