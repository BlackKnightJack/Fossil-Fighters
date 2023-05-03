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

    //Dialogs this event contains
    [Space(10)]
    public Dialog[] dialogs;

    //Misc variables
    Camera mainCamera;
    bool inTriggerRange;
    int layerMask;

    // Awake triggers at the start of a scene
    void Awake()
    {
        //Cache variables
        mainCamera = Camera.main;

        //Assign layer mask
        layerMask = 1 << LayerMask.NameToLayer("Range");
        layerMask = ~layerMask;
    }

    // Update triggers every frame
    void Update()
    {
        //In the case that it's an interact activation
        if (Input.GetMouseButtonDown(0) && inTriggerRange)
        {
            if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, layerMask))
            {
                if (hit.collider == GetComponent<Collider>())
                {
                    print("In trigger range");
                }
            }
        }
    }

    //Public functions
    public void TriggerRangeChange(bool change)
    {
        inTriggerRange = change;
    }
}