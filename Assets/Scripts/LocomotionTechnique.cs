using Oculus.Interaction;
using Oculus.Interaction.Input;
using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LocomotionTechnique : MonoBehaviour
{
    [SerializeField] HandVisual leftHandVisual;
    [SerializeField] HandVisual rightHandVisual;

    // Please implement your locomotion technique in this script. 
    public OVRInput.Controller leftController;
    public OVRInput.Controller rightController;
    [Range(0, 10)] public float translationGain = 0.5f;
    [SerializeField] private Hmd hmd;
    [SerializeField] private Hand leftHand;
    private HandSkeleton leftHandSkeleton;
    [SerializeField] private Hand rightHand;
    private HandSkeleton rightHandSkeleton;
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
    public Hand[] hands = new Hand[2];

    bool[] teleportationStarted = new bool[2];
    Pose[] startTeleportationPose = new Pose[2];

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        hands[0] = leftHand;
        hands[1] = rightHand;
    }

    void Update()
    {
        //Debug.Log("L-hand scale: " + leftHand.HandScale);
        //Debug.Log("R-hand scale: " + rightHand.HandScale);

        //OVRBone[] leftHandBones = leftHandSkeleton.Bones.ToArray();
        //OVRBone[] rightHandBones = rightHandSkeleton.Bones.ToArray();

        //Debug.Log("L-hand Bones (Local): " + string.Join<Vector3>(", ", Array.ConvertAll(leftHandBones, bone => bone.Transform.localPosition)));
        //Debug.Log("R-hand Bones (Local): " + string.Join<Vector3>(", ", Array.ConvertAll(rightHandBones, bone => bone.Transform.localPosition)));

        //Debug.Log("L-hand Bones (Global): " + string.Join<Vector3>(", ", Array.ConvertAll(leftHandBones, bone => bone.Transform.position)));
        //Debug.Log("R-hand Bones (Global): " + string.Join<Vector3>(", ", Array.ConvertAll(rightHandBones, bone => bone.Transform.position)));

        //Debug.Log("L-hand Bones (Rotation): " + string.Join<Quaternion>(", ", Array.ConvertAll(leftHandBones, bone => bone.Transform.localRotation)));
        //Debug.Log("R-hand Bones (Rotation): " + string.Join<Quaternion>(", ", Array.ConvertAll(rightHandBones, bone => bone.Transform.localRotation)));

        //Debug.Log("L-hand:");
        //PrintPinch(leftHand);
        //Debug.Log("R-hand:");
        //PrintPinch(rightHand);

        //Debug.Log("Active Controller: " + OVRInput.GetActiveController());
        //if (OVRInput.GetActiveController() != OVRInput.Controller.None)
        //{

        //}

        //Debug.Log("Connected Controller: " + OVRInput.GetConnectedControllers());
        
		for (int i = 0; i < hands.Length; i++)
        {
			if (hands[i].GetFingerIsPinching(HandFinger.Index))
			{
                if (!teleportationStarted[i])
                {
                    hands[i].GetRootPose(out Pose handPose);
                    startTeleportationPose[i] = handPose;
                    teleportationStarted[i] = true;
				}
			}
            else
            {
                if (teleportationStarted[i])
                {
                    TeleportToHandPosition(hands[i], startTeleportationPose[i]);
                    hands[i].GetRootPose(out Pose handPose);
                    Vector3 offset = startTeleportationPose[i].position - handPose.position;
                    
                    teleportationStarted[i] = false;
                }
            }
		}
        
        //switch (OVRInput.GetActiveController())
        //{
        //    case OVRInput.Controller.LTouch:
        //    case OVRInput.Controller.RTouch:
        //    case OVRInput.Controller.Touch:
        //        break;

        //    case OVRInput.Controller.Hands:
        //    case OVRInput.Controller.RHand:
        //    case OVRInput.Controller.LHand:
        //        break;
        //}

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        //// Please implement your LOCOMOTION TECHNIQUE in this script :D.
        //leftTriggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, leftController);
        //rightTriggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, rightController);

        //if (leftTriggerValue > 0.95f && rightTriggerValue > 0.95f)
        //{
        //    if (!isIndexTriggerDown)
        //    {
        //        isIndexTriggerDown = true;
        //        startPos = (OVRInput.GetLocalControllerPosition(leftController) + OVRInput.GetLocalControllerPosition(rightController)) / 2;
        //    }
        //    offset = hmd.transform.forward.normalized *
        //            ((OVRInput.GetLocalControllerPosition(leftController) - startPos) +
        //             (OVRInput.GetLocalControllerPosition(rightController) - startPos)).magnitude;
        //    Debug.DrawRay(startPos, offset, Color.red, 0.2f);
        //}
        //else if (leftTriggerValue > 0.95f && rightTriggerValue < 0.95f)
        //{
        //    if (!isIndexTriggerDown)
        //    {
        //        isIndexTriggerDown = true;
        //        startPos = OVRInput.GetLocalControllerPosition(leftController);
        //    }
        //    offset = hmd.transform.forward.normalized *
        //             (OVRInput.GetLocalControllerPosition(leftController) - startPos).magnitude;
        //    Debug.DrawRay(startPos, offset, Color.red, 0.2f);
        //}
        //else if (leftTriggerValue < 0.95f && rightTriggerValue > 0.95f)
        //{
        //    if (!isIndexTriggerDown)
        //    {
        //        isIndexTriggerDown = true;
        //        startPos = OVRInput.GetLocalControllerPosition(rightController);
        //    }
        //    offset = hmd.transform.forward.normalized *
        //             (OVRInput.GetLocalControllerPosition(rightController) - startPos).magnitude;
        //    Debug.DrawRay(startPos, offset, Color.red, 0.2f);
        //}
        //else
        //{
        //    if (isIndexTriggerDown)
        //    {
        //        isIndexTriggerDown = false;
        //        offset = Vector3.zero;
        //    }
        //}
        //this.transform.position = this.transform.position + (offset) * translationGain;


        //////////////////////////////////////////////////////////////////////////////////
        //// These are for the game mechanism.
        //if (OVRInput.Get(OVRInput.Button.Two) || OVRInput.Get(OVRInput.Button.Four))
        //{
        //    if (parkourCounter.parkourStart)
        //    {
        //        this.transform.position = parkourCounter.currentRespawnPos;
        //    }
        //}
    }

    void OnTriggerEnter(Collider other)
    {
        // These are for the game mechanism.
        if (other.CompareTag("objectInteractionTask"))
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
        else if (other.CompareTag("banner"))
        {
            stage = other.gameObject.name;
            parkourCounter.isStageChange = true;
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

    //public void ThumpsUp()
    //{
    //    Debug.Log("ThumpsUp");
    //    TeleportToHandPosition(rightHand);
    //}

    //public void Fist()
    //{
    //    Debug.Log("Fist");
    //    TeleportToHandPosition(rightHand);
    //}

    private RaycastHit[] hits = new RaycastHit[20];

    private void TeleportToHandPosition(Hand hand, Pose startHandPose)
    {
        if (hand.GetRootPose(out Pose handPose) && hmd.GetRootPose(out Pose hmdPose))
        {
            float newRigRotation = (360 + transform.InverseTransformDirection(0, handPose.rotation.eulerAngles.y, 0).y - 90 - hmdPose.rotation.eulerAngles.y) % 360;
            Debug.LogWarning("hand rotation: " + handPose.rotation.eulerAngles);
            Debug.LogWarning("hmd rotation: " + hmdPose.rotation.eulerAngles);
            Debug.LogWarning("camera rig rotation: " + transform.rotation.eulerAngles);

            //float armRotation = Quaternion.LookRotation(handPose.position - hmdPose.transform.position, Vector3.up).eulerAngles.y - hmdPose.rotation.eulerAngles.y;
            //float handRotation = transform.InverseTransformDirection(0, handPose.rotation.eulerAngles.y, 0).y - 90 - armRotation;
            //float newRigRotation = (720 + hmdPose.rotation.eulerAngles.y + handRotation) % 360;
            //Debug.Assert(t == newRigRotation);

            //Debug.Log("arm rotation: " + armRotation);
            Debug.LogWarning("new camera rig rotation: " + newRigRotation);

            Vector3 newPosition = new Vector3(handPose.position.x, transform.position.y, handPose.position.z);
            Quaternion newOrientation = Quaternion.Euler(transform.rotation.eulerAngles.x, newRigRotation, transform.rotation.eulerAngles.z);
            Vector3 direction = newPosition - transform.position;

            bool coinHit = false;
            Vector3 origin = transform.position + new Vector3(0, 1.5f, 0); // add some height so the ray is able to hit coins
            int hitCount = Physics.RaycastNonAlloc(origin, direction, hits, direction.magnitude, LayerMask.GetMask("locomotion"));
            for (int i = 0; i < hitCount; i++)
            {
                GameObject hitObject = hits[i].collider.gameObject;

                if (hitObject.CompareTag("coin"))
                {
                    parkourCounter.coinCount += 1;
                    hitObject.SetActive(false);
                    coinHit = true;
                }
                else if (hitObject.CompareTag("banner"))
                {
                    stage = hitObject.gameObject.name;
                    parkourCounter.isStageChange = true;
                }
            }
            if (coinHit)
            {
                audioSource.Play();
            }

            transform.SetPositionAndRotation(newPosition, newOrientation);
        }
        else
        {
            Debug.Log("Positioning failed");
        }
    }
}