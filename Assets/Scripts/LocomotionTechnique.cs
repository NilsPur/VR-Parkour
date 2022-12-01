using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(AudioSource))]
public class LocomotionTechnique : MonoBehaviour
{
    // Please implement your locomotion technique in this script. 
    public OVRInput.Controller leftController;
    public OVRInput.Controller rightController;
    [Range(0, 10)] public float translationGain = 0.5f;
    public GameObject hmd;
    [SerializeField] private OVRHand leftHand;
    [SerializeField] private OVRSkeleton leftHandSkeleton;
    [SerializeField] private OVRHand rightHand;
    [SerializeField] private OVRSkeleton rightHandSkeleton;
    [SerializeField] private float leftTriggerValue;
    [SerializeField] private float rightTriggerValue;
    [SerializeField] private Vector3 startPos;
    [SerializeField] private Vector3 offset;
    [SerializeField] private bool isIndexTriggerDown;
    private AudioSource audioSource;

    /////////////////////////////////////////////////////////
    // These are for the game mechanism.
    public ParkourCounter parkourCounter;
    public string stage;
    public SelectionTaskMeasure selectionTaskMeasure;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        //Debug.Log("L-hand scale: " + leftHand.HandScale);
        //Debug.Log("R-hand scale: " + rightHand.HandScale);

        OVRBone[] leftHandBones = leftHandSkeleton.Bones.ToArray();
        OVRBone[] rightHandBones = rightHandSkeleton.Bones.ToArray();

        Debug.Log("L-hand Bones (Local): " + string.Join<Vector3>(", ", Array.ConvertAll(leftHandBones, bone => bone.Transform.localPosition)));
        Debug.Log("R-hand Bones (Local): " + string.Join<Vector3>(", ", Array.ConvertAll(rightHandBones, bone => bone.Transform.localPosition)));

        Debug.Log("L-hand Bones (Global): " + string.Join<Vector3>(", ", Array.ConvertAll(leftHandBones, bone => bone.Transform.position)));
        Debug.Log("R-hand Bones (Global): " + string.Join<Vector3>(", ", Array.ConvertAll(rightHandBones, bone => bone.Transform.position)));

        Debug.Log("L-hand Bones (Rotation): " + string.Join<Quaternion>(", ", Array.ConvertAll(leftHandBones, bone => bone.Transform.localRotation)));
        Debug.Log("R-hand Bones (Rotation): " + string.Join<Quaternion>(", ", Array.ConvertAll(rightHandBones, bone => bone.Transform.localRotation)));

        Debug.Log("L-hand:");
        PrintPinch(leftHand);
        Debug.Log("R-hand:");
        PrintPinch(rightHand);

        //Debug.Log("Active Controller: " + OVRInput.GetActiveController());
        //if (OVRInput.GetActiveController() != OVRInput.Controller.None)
        //{

        //}

        if (IsFist(leftHand) || IsFist(rightHand))
        {
            Debug.Log("FIST");
        }

        //Debug.Log("Connected Controller: " + OVRInput.GetConnectedControllers());

        switch (OVRInput.GetActiveController())
        {
            case OVRInput.Controller.LTouch:
            case OVRInput.Controller.RTouch:
            case OVRInput.Controller.Touch:
                break;

            case OVRInput.Controller.Hands:
            case OVRInput.Controller.RHand:
            case OVRInput.Controller.LHand:
                break;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Please implement your LOCOMOTION TECHNIQUE in this script :D.
        leftTriggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, leftController);
        rightTriggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, rightController);

        if (leftTriggerValue > 0.95f && rightTriggerValue > 0.95f)
        {
            if (!isIndexTriggerDown)
            {
                isIndexTriggerDown = true;
                startPos = (OVRInput.GetLocalControllerPosition(leftController) + OVRInput.GetLocalControllerPosition(rightController)) / 2;
            }
            offset = hmd.transform.forward.normalized *
                    ((OVRInput.GetLocalControllerPosition(leftController) - startPos) +
                     (OVRInput.GetLocalControllerPosition(rightController) - startPos)).magnitude;
            Debug.DrawRay(startPos, offset, Color.red, 0.2f);
        }
        else if (leftTriggerValue > 0.95f && rightTriggerValue < 0.95f)
        {
            if (!isIndexTriggerDown)
            {
                isIndexTriggerDown = true;
                startPos = OVRInput.GetLocalControllerPosition(leftController);
            }
            offset = hmd.transform.forward.normalized *
                     (OVRInput.GetLocalControllerPosition(leftController) - startPos).magnitude;
            Debug.DrawRay(startPos, offset, Color.red, 0.2f);
        }
        else if (leftTriggerValue < 0.95f && rightTriggerValue > 0.95f)
        {
            if (!isIndexTriggerDown)
            {
                isIndexTriggerDown = true;
                startPos = OVRInput.GetLocalControllerPosition(rightController);
            }
            offset = hmd.transform.forward.normalized *
                     (OVRInput.GetLocalControllerPosition(rightController) - startPos).magnitude;
            Debug.DrawRay(startPos, offset, Color.red, 0.2f);
        }
        else
        {
            if (isIndexTriggerDown)
            {
                isIndexTriggerDown = false;
                offset = Vector3.zero;
            }
        }
        this.transform.position = this.transform.position + (offset) * translationGain;


        ////////////////////////////////////////////////////////////////////////////////
        // These are for the game mechanism.
        if (OVRInput.Get(OVRInput.Button.Two) || OVRInput.Get(OVRInput.Button.Four))
        {
            if (parkourCounter.parkourStart)
            {
                this.transform.position = parkourCounter.currentRespawnPos;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {

        // These are for the game mechanism.
        if (other.CompareTag("banner"))
        {
            stage = other.gameObject.name;
            parkourCounter.isStageChange = true;
        }
        else if (other.CompareTag("objectInteractionTask"))
        {
            selectionTaskMeasure.isTaskStart = true;
            selectionTaskMeasure.scoreText.text = "";
            selectionTaskMeasure.partSumErr = 0f;
            selectionTaskMeasure.partSumTime = 0f;
            // rotation: facing the user's entering direction
            float tempValueY = other.transform.position.y > 0 ? 12 : 0;
            Vector3 tmpTarget = new Vector3(hmd.transform.position.x, tempValueY, hmd.transform.position.z);
            selectionTaskMeasure.taskUI.transform.LookAt(tmpTarget);
            selectionTaskMeasure.taskUI.transform.Rotate(new Vector3(0, 180f, 0));
            selectionTaskMeasure.taskStartPanel.SetActive(true);
        }
        else if (other.CompareTag("coin"))
        {
            parkourCounter.coinCount += 1;
            audioSource.Play();
            other.gameObject.SetActive(false);
        }
        // These are for the game mechanism.
    }

    private void PrintPinch(OVRHand hand)
    {
        for (int fingerIndex = (int)OVRHand.HandFinger.Thumb; fingerIndex < (int)OVRHand.HandFinger.Max; fingerIndex++)
        {
            OVRHand.HandFinger finger = (OVRHand.HandFinger)fingerIndex;
            Debug.Log(finger.ToString() + " (Pinch): " + hand.GetFingerPinchStrength(finger));
        }
    }

    //private bool IsFist(OVRHand hand)
    //{
    //    return hand.GetFingerPinchStrength(OVRHand.HandFinger.Thumb) > 0.6 && hand.GetFingerPinchStrength(OVRHand.HandFinger.Index) > 0.6 &&
    //           hand.GetFingerPinchStrength(OVRHand.HandFinger.Middle) > 0.5 && hand.GetFingerPinchStrength(OVRHand.HandFinger.Ring) > 0.3 &&
    //           hand.GetFingerPinchStrength(OVRHand.HandFinger.Pinky) > 0.2;
    //}
}