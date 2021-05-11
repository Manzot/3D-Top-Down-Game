using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BodyPartToAttachTo { spine, leftHand, rightHand }

public class PlayerEquipmentManager : MonoBehaviour
{
    PlayerController player;
    // Assigned equipped weapons Placeholder
    public Transform phShieldEquipped;
    public Transform phPrimaryWeaponEquipped;
    public Transform phSecondaryWeaponEquipped;
    // Assigned Wepaons and Equipment Unequipped Placeholder
    public Transform phShieldUnEquipped;
    public Transform phPrimaryWeaponUnEquipped;
    public Transform phSecondaryWeaponUnEquipped;
    //  Assigned Weapons and Shiled Itself
   // [HideInInspector]
    public Shield shield;
   // [HideInInspector]
    public Weapon primaryWeapon;
    [HideInInspector]
    public Weapon secondaryWeapon;

    public GameObject playerHead;
    public GameObject playerHair;
    public GameObject playerBody;
    public Material highLightMaterial;
    public GameObject slash;
    // Assigned Weapons and Equipment SFX
    // public GameObject trialEffectContainer;
    //GameObject swish1;
    // materials
    Renderer headRenderer;
    Renderer hairRenderer;
    Renderer bodyRenderer;

    Material defaultHeadMaterial;
    Material defaultHairMaterial;
    Material defaultBodyMaterial;
    Material defaultPriWeaponMaterial;
    Material defaultShieldMaterial;
    Material defaultSecWeaponMaterial;

    ParticleSystem trialEffectPrimaryWeapon;
    //public GameObject swordSlashParticles;

    private void Start()
    {
        player = GetComponent<PlayerController>();
        headRenderer = playerHead.GetComponent<Renderer>();
        hairRenderer = playerHair.GetComponent<Renderer>();
        bodyRenderer = playerBody.GetComponent<Renderer>();

        defaultHeadMaterial = headRenderer.material;
        defaultHairMaterial = hairRenderer.material;
        defaultBodyMaterial = bodyRenderer.material;
        //trialEffectContainer = trialEffects.GetComponent<Animator>();
       // swish1 = trialEffects.transform.GetChild(0).gameObject;
       // swish1.SetActive(false);
    }
    public void ShieldActivate(BodyPartToAttachTo bodyPart)
    {
        if(shield != null)
        {
            if (bodyPart == BodyPartToAttachTo.spine)
                shield.transform.parent = phShieldUnEquipped.transform;
            else if (bodyPart == BodyPartToAttachTo.leftHand)
                shield.transform.parent = phShieldEquipped.transform;

            shield.transform.localPosition = Vector3.zero;
            shield.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }
    public void PrimaryWeaponActivate(BodyPartToAttachTo bodyPart)
    {
        if(primaryWeapon != null)
        {
            if(bodyPart == BodyPartToAttachTo.spine)
                primaryWeapon.transform.parent = phPrimaryWeaponUnEquipped.transform;
            else if(bodyPart == BodyPartToAttachTo.rightHand)
                primaryWeapon.transform.parent = phPrimaryWeaponEquipped.transform;
            primaryWeapon.transform.localPosition = Vector3.zero;
            primaryWeapon.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }

    public void EnableSlashParticles()
    {
        slash.SetActive(true);
        StartCoroutine(HelpUtils.WaitForSeconds(delegate { slash.SetActive(false); }, 0.4f));
        //trialEffectPrimaryWeapon.transform.parent = primaryWeapon.transform;
        //trialEffectPrimaryWeapon.transform.localPosition = new Vector3(0, -0.14f, 0);
        //trialEffectPrimaryWeapon.transform.localRotation = Quaternion.Euler( Vector3.zero);
        //trialEffectPrimaryWeapon.Play();
    }
    public void DisableSlashParticles()
    {
       // slash.SetActive(false);
        // trialEffectPrimaryWeapon.Stop();
        //trialEffectPrimaryWeapon.Clear();
        //  trialEffectPrimaryWeapon.transform.parent = null;// trialEffectContainer.transform;
    }
    public void SetAttackBool(int _bSetAttack)
    {
        if (primaryWeapon)
        {
            if(_bSetAttack == 1)
            {
                //player.SetIsAttacking(true);
                primaryWeapon.WeaponColiSetActive(true);
            }
            else
            {
               // player.SetIsAttacking(false);
                primaryWeapon.WeaponColiSetActive(false);
            }
        }
    }

    IEnumerator disableGameObjectAfter(GameObject _go, bool _enableDisable)
    {
        yield return new WaitForSeconds(.2f);
        _go.SetActive(_enableDisable);
    }

    public void SetPrimaryWeapon(GameObject _primaryWeaponToSet)
    {
        if(primaryWeapon != null)
            Destroy(primaryWeapon.gameObject);

        if (_primaryWeaponToSet != null)
        {
            primaryWeapon = _primaryWeaponToSet.GetComponent<Weapon>();
            defaultPriWeaponMaterial = primaryWeapon.GetComponent<Renderer>().material;
            trialEffectPrimaryWeapon = primaryWeapon.weaponTrialEffect;
        }
        else
            primaryWeapon = null;
    }
    public void SetSecondaryWeapon(GameObject _secondaryWeaponToSet)
    {
        if (secondaryWeapon != null)
            Destroy(secondaryWeapon.gameObject);

        if (_secondaryWeaponToSet != null)
        {
            secondaryWeapon = _secondaryWeaponToSet.GetComponent<Weapon>();
            defaultSecWeaponMaterial = secondaryWeapon.GetComponent<Renderer>().material;
        }
        else
            secondaryWeapon = null;
    }
    public void SetShield(GameObject _shieldToSet)
    {
        if (shield != null)
            Destroy(shield.gameObject);

        if (_shieldToSet != null)
        {
            shield = _shieldToSet.GetComponent<Shield>();
            defaultShieldMaterial = shield.GetComponent<Renderer>().material;
        }
        else
            shield = null;
    }
    public void HighlightMaterial()
    {
        headRenderer.material = highLightMaterial;
        hairRenderer.material = highLightMaterial;
        bodyRenderer.material = highLightMaterial;

        if (primaryWeapon)
            primaryWeapon.GetComponent<Renderer>().material = highLightMaterial;
        if (shield)
            shield.GetComponent<Renderer>().material = highLightMaterial;
    }
    public void SetDefaultMaterials()
    {
        headRenderer.material = defaultHeadMaterial;
        hairRenderer.material = defaultHairMaterial;
        bodyRenderer.material = defaultBodyMaterial;
        if(primaryWeapon)
            primaryWeapon.GetComponent<Renderer>().material = defaultPriWeaponMaterial;
        if(shield)
            shield.GetComponent<Renderer>().material = defaultShieldMaterial;
    }
    public Item GetPrimaryWeaponItem()
    {
        return primaryWeapon.GetComponent<ItemContainer>().item;
    }
    public Item GetSecondaryWeaponItem()
    {
        return secondaryWeapon.GetComponent<ItemContainer>().item;
    }
    public Item GetShieldItem()
    {
        return shield.GetComponent<ItemContainer>().item;
    }
}
