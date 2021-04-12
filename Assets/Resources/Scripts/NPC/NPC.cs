using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public enum NPCBehaviour { SIMPLE = 0, QUEST_NPC = 1, MERCHANT = 2}
public class NPC : MonoBehaviour
{
    //public static List<NPCEntity> npcList;
    [HideInInspector]
    public string sNpcID = System.Guid.NewGuid().ToString();
    public string sNpcName = "NPC-";
    public NPCBehaviour npcBehaviour;
    public DialogArrays[] sRandomDialogs; // Use this to Randomly choose a dialog  to appear instead of sFirstDialogLines
    string[] sDialogsToUse;
    private Animator anim;
    private Rigidbody rbody;

    private bool bIsInteracting;
    private bool bDialogCheck;
    
    // Quest Variables
    private Quest myActiveQuest;
    private List<AddNewQuest> myQuestsLst;
    private NPCAssignedQuestDialog[] assignedQuestGoals;

    public List<Item> merchantInventoryLst;
    private Inventory merchantInventory;

    Movement moveScr;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        rbody = GetComponent<Rigidbody>();

        rbody.isKinematic = true;
        rbody.mass = 1000;
        rbody.drag = 10;
        rbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
    }
    void Start()
    {
        moveScr = GetComponent<Movement>();

        assignedQuestGoals = GetComponentsInChildren<NPCAssignedQuestDialog>();

        if(assignedQuestGoals.Length <= 0)
            assignedQuestGoals = null;

        if(npcBehaviour == NPCBehaviour.MERCHANT)
        {
            merchantInventory = new Inventory(30);
            merchantInventory.lstItems = merchantInventoryLst;
        }

        AddQuestsIfAny();

        gameObject.layer = LayerMask.NameToLayer("Npc");
        // Starting Idle Animation at random time
        AnimatorStateInfo _animState = anim.GetCurrentAnimatorStateInfo(0);
        anim.Play(_animState.fullPathHash, -1, Random.Range(0f, 1f));
    }
    void Update()
    {
        CheckForDialogToFinish();
        SetAnimations();
    }
    private void FixedUpdate()
    {

    }

    /// NPC QUESTING SYSTEM                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             
    public void AddQuestsIfAny()
    {
        if (npcBehaviour == NPCBehaviour.QUEST_NPC)
        {
            myQuestsLst = GetComponentsInChildren<AddNewQuest>().ToList();
        }
    }
    public void ActivateQuest()
    {
        myActiveQuest.Initialize();
    }

    // Setter Functions
    public bool SetDialog()
    {
        bIsInteracting = true;
        if (moveScr)
            moveScr.SetMovementActive(false);
        bDialogCheck = true;

        if(npcBehaviour == NPCBehaviour.QUEST_NPC)
        {
            if (myQuestsLst.Count > 0)
            {
                SetActiveQuest(); // Setting myActive Quest

                if (myActiveQuest == null) // if my active quest is null then it checks if it has assigned active goal, if there is a goal then
                {                          // then it sets dialog acc to that goal else it checks about this npc's quest
                    if (!CheckNpcAssignedTalkingGoals())
                    {
                        SetQuestDialogToUse();
                    }
                }
                //else if (myActiveQuest.IsActive()) // if this npc has an active quest then it prioritize its quest than other assigned goals
                //{
                //    SetQuestDialogToUse();
                //}
                else
                {
                    if (!CheckNpcAssignedTalkingGoals()) // if quest not active then it prioritize any active assigned goals
                    {
                        SetQuestDialogToUse();
                    }
                }
                PopupUIManager.Instance.dialogBoxPopup.SetQuestNPC(this);
                PopupUIManager.Instance.dialogBoxPopup.setDialogText(sDialogsToUse);
                return true;
            }
        }
        else
        {
            if (!CheckNpcAssignedTalkingGoals()) // if no assigned task then set a random dialog
            {
                if (sRandomDialogs.Length > 0)
                {
                    if (sRandomDialogs[0].sDialogLines.Length > 0)
                    {
                        sDialogsToUse = SelectRandomDialog();
                        PopupUIManager.Instance.dialogBoxPopup.SetQuestNPC(this);
                        PopupUIManager.Instance.dialogBoxPopup.setDialogText(sDialogsToUse);
                        return true;
                    }
                }
            }
            else
            {
                PopupUIManager.Instance.dialogBoxPopup.SetQuestNPC(this);
                PopupUIManager.Instance.dialogBoxPopup.setDialogText(sDialogsToUse);
                return true;
            }
            
        }
        bIsInteracting = false;

        if (moveScr)
            moveScr.SetMovementActive(true);

        bDialogCheck = false;

        return false;
    }
    public void SetQuestDialogToUse()
    {
        for (int i = 0; i < myQuestsLst.Count; i++)
        {
            if (myActiveQuest == null) // if this quest is null just continue to next one
            {
                sDialogsToUse = SelectRandomDialog();
                continue;
            }
            else
            {
                if (myActiveQuest.IsCompleted()) // if this is completed, continue to next one
                {
                    sDialogsToUse = SelectRandomDialog();
                    myActiveQuest = null;
                    continue;
                }
                else
                {
                    if (!myActiveQuest.IsActive()) // means quest is not active or started, or not there in quest manager
                    {
                        if (myActiveQuest.eQuestType == QuestType.MAINQUEST) // Initialize quest directly only if it is a main quest, else ask for response.
                        {
                            ActivateQuest();
                        }
                        sDialogsToUse = myQuestsLst[i].QuestStartDialog().ToArray();
                        break;
                    }
                    else
                    {
                        if (myActiveQuest.IsCompleted()) // quest is active and checking its progress if it is finished
                        {
                            sDialogsToUse = myQuestsLst[i].QuestFinishedDialog().ToArray();
                            myQuestsLst.Remove(myQuestsLst[i]);
                            break;
                        }
                        else // The quest is in progress
                        {
                            sDialogsToUse = myQuestsLst[i].QuestInProgressDialog().ToArray();
                            break;
                        }
                    }
                }
            }
        }
    }
    public void SetActiveQuest()
    {
        for (int i = 0; i < myQuestsLst.Count; i++)
        {
            myActiveQuest = QuestManager.Instance.GetQuestByID(myQuestsLst[i].quest);
            if (myActiveQuest != null)
                break;                
        }
    }
    public void LookAtTarget(Transform _target)
    {
        StopAllCoroutines();
        StartCoroutine(HelpUtils.RotateTowardsTarget(transform, _target));
    }
    public void SetAnimations()
    {
        if(moveScr)
            anim.SetBool("isMoving", moveScr.IsMoving());
    }

    /// Checker Functions
    public void CheckForDialogToFinish()
    {
        if (bIsInteracting)
        {
            if (!PopupUIManager.Instance.dialogBoxPopup.GetDialogInProgress())
            {
                if (bDialogCheck)
                {
                    StartCoroutine(HelpUtils.WaitForSeconds(delegate { bIsInteracting = false; if (moveScr)
                            moveScr.SetMovementActive(true);
                    }, 1f));
                    bDialogCheck = false;
                }
            }
        }
    }
    public bool CheckNpcAssignedTalkingGoals()
    {
        if(assignedQuestGoals != null) // right now the npc only prefers the latest goal its assigned -----
        {
            for (int i = 0; i < assignedQuestGoals.Length; i++)
            {
                if (assignedQuestGoals[i].IsFinished()) // it checks if this goal is done already, then check for the latest one
                {
                    /// Its to make npc start speaking its normal dialog after the assigned quest dialog.
                    continue;
                }
                if (assignedQuestGoals[i].QuestGoalCheck()) //this is to check if the assigned goal is not finished yet
                {
                    sDialogsToUse = assignedQuestGoals[i].sQuestDialog.ToArray();
                    return true;
                }
                else
                {
                    continue;
                }
            }
        }
        sDialogsToUse = SelectRandomDialog();
        return false;
    }

    /// Other Helper Functions
    public Quest GetQuest()
    {
        return myActiveQuest;
    }
    public string[] SelectRandomDialog()
    {
        int _iRandom = Random.Range(0, sRandomDialogs.Length);
        return sRandomDialogs[_iRandom].sDialogLines.ToArray();
    }

    public void OpenShop()
    {
        PopupUIManager.Instance.shopPopup.open(merchantInventory);
    }
}

