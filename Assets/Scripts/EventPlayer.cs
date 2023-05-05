using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using TMPro;

[Serializable]
public class Dialog
{
    public string[] lines;
    public Dialog(string[] inLines) { lines = inLines; }
}

public class EventPlayer : MonoBehaviour
{
    //Set up activation mode for the event
    public enum ActivationMode { Auto, Trigger, Interact }
    public ActivationMode activationMode;

    //Textbox components for when they are necessary
    [Header("Dialog Box")]
    public TMP_Text dialogText;
    public float dialogLag;
    GameObject dialogBack;
    int activeDialog;
    int lineIndex;

    //Dialogs this event contains
    public Dialog[] dialogs;

    //Flags
    [Header("Flags")]
    public bool disposeOnComplete;

    //Misc variables
    ThirdPersonController player;
    PlayableDirector cutscene;
    PlayableGraph playingCutscene;
    Camera mainCamera;
    int layerMask;
    bool active;
    bool inTriggerRange;

    // Awake triggers at the start of a scene
    void Awake()
    {
        //Cache variables
        if (dialogText != null) { dialogBack = dialogText.transform.parent.gameObject; }
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<ThirdPersonController>();
        mainCamera = Camera.main;

        //Assign layer mask
        layerMask = 1 << LayerMask.NameToLayer("Range");
        layerMask = ~layerMask;

        //Catalog the timeline and activate it if it is set to play automatically
        if (TryGetComponent(out cutscene) && activationMode == ActivationMode.Auto)
        {
            ActivateEvent();
        }
    }

    // Update triggers every frame
    void Update()
    {
        //If it's not active and the player isn't in another event, look for opportunities to activate it
        if (!active && player.isActiveAndEnabled)
        {
            //In the case that it's an interact activation
            if (Input.GetMouseButtonDown(0) && inTriggerRange)
            {
                if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    if (hit.collider == GetComponent<Collider>())
                    {
                        ActivateEvent();
                    }
                }
            }
        }

        //Otherwise try to deactivate it
        else if (active)
        {
            //If the message back is active, play dialog
            if (dialogBack != null && dialogBack.activeSelf)
            {
                //Move to next line
                if (Input.GetMouseButtonDown(0) && dialogText.text == dialogs[activeDialog].lines[lineIndex])
                {
                    //If there's another line to move to, increment the line index and start that line
                    if (lineIndex + 1 < dialogs[activeDialog].lines.Length)
                    {
                        lineIndex++;
                        StartCoroutine(ScrollDialog());
                    }
                    else
                    {
                        //If a timeline is present, simply resume the timeline
                        if (cutscene != null)
                        {
                            dialogBack.SetActive(false);
                            cutscene.Resume();
                        }

                        //If a timeline isn't present, though, deactivate the event full-stop
                        else
                        {
                            DeactivateEvent();
                        }
                    }
                }
            }

            //If a timeline is present and it's done, deactivate the event
            if (cutscene != null && !cutscene.playableGraph.IsValid())
            {
                DeactivateEvent();
            }
        }
    }

    //Public functions
    public void TriggerRangeChange(bool change)
    {
        inTriggerRange = change;
    }

    public void OpenDialog(int index)
    {
        //Pause the timeline that called this function
        cutscene.Pause();

        //Open dialog box
        dialogBack.SetActive(true);

        //Reset dialog variables just to be sure
        dialogText.text = string.Empty;
        activeDialog = index;
        lineIndex = 0;

        //Start the dialog
        StartCoroutine(ScrollDialog());
    }

    //Private functions
    void ActivateEvent()
    {
        //First set the event to active
        active = true;

        //First deactivate the player script
        player.enabled = false;

        //Attempt to execute a playable
        if (cutscene != null)
        {
            cutscene.Play(cutscene.playableAsset);
        }

        //If that doesn't work, attempt to display dialog
        else if (dialogs.Length > 0)
        {
            //Face the player towards the event source since it's just regular dialog
            Vector3 playerEulers = player.transform.eulerAngles;
            player.transform.LookAt(transform);
            player.transform.eulerAngles = new Vector3(playerEulers.x, player.transform.eulerAngles.y, playerEulers.z);

            //I might want to swing an attached event camera to face between the event source and the player later

            //Open dialog box
            dialogBack.SetActive(true);

            //Reset dialog variables just to be sure
            activeDialog = 0;
            lineIndex = 0;

            //Start the dialog
            StartCoroutine(ScrollDialog());
        }

        //If that doesn't work, attempt to manipulate inventory

        //If that doesn't work, deactivate the event
    }

    void DeactivateEvent()
    {
        //Deactivate the event 
        active = false;

        //Hide the textbox just to be sure
        dialogBack.SetActive(false);

        //Give control back to the player character
        player.enabled = true;

        //Dispose of the event if it is flagged to do so upon deactivation
        if (disposeOnComplete)
        {
            Destroy(this);
        }
    }

    IEnumerator ScrollDialog()
    {
        //Reset dialog box
        dialogText.text = string.Empty;

        //Increment the line into the box
        foreach(char c in dialogs[activeDialog].lines[lineIndex])
        {
            dialogText.text += c;
            yield return new WaitForSeconds(dialogLag);
        }
    }
}