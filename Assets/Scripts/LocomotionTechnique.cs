﻿using JetBrains.Annotations;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.UI.Image;

public enum LocomotionType
{
    Bow,
    Teleport
}

[RequireComponent(typeof(AudioSource))]
public class LocomotionTechnique : MonoBehaviour
{
    public static readonly LocomotionType LocomotionType = LocomotionType.Bow;

    // Please implement your locomotion technique in this script.
    [SerializeField] Grabber leftGrabber;
    [SerializeField] Grabber rightGrabber;
    [SerializeField] HandVisual leftHandVisual;
    [SerializeField] HandVisual rightHandVisual;
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
    [SerializeField] private LineDrawer lineDrawerLeftHand;
    [SerializeField] private LineDrawer lineDrawerRightHand;
    private AudioSource audioSource;
    private LineDrawer[] lineDrawers = new LineDrawer[2];
    public ParkourCounter parkourCounter;
    public string stage;
    public SelectionTaskMeasure selectionTaskMeasure;
    public Hand[] hands { get; set; } = new Hand[2];
    public Grabber[] grabbers { get; set; } = new Grabber[2];
    public bool[] TeleportationStarted { get; set; } = new bool[2];

    int bowIndex = -1;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        hands[0] = leftHand;
        hands[1] = rightHand;
        grabbers[0] = leftGrabber;
        grabbers[1] = rightGrabber;
        lineDrawers[0] = lineDrawerLeftHand;
        lineDrawers[1] = lineDrawerRightHand;
    }

    void Update()
    {       
		for (int i = 0; i < hands.Length; i++)
        {
            if (hands[i].IsHighConfidence)
            {
                if (hands[i].GetFingerIsPinching(HandFinger.Index))
                {
                    if (LocomotionType == LocomotionType.Bow)
                    {
                        TeleportationStarted[i] = true;
                        // Is the bow now drawn ?
                        if (TeleportationStarted[(i + 1) % 2])
                        {
                            lineDrawerRightHand.SetDrawArrowTrajectory(true);
                        }
                        // This hand is the bow
                        else
                        {
                            bowIndex = i;
                        }
                    }
                    else
                    {
                        if (grabbers[(i + 1) % 2].IsGrabbing)
                        {
                            if (!grabbers[i].IsRotating)
                            {
                                grabbers[i].Rotate(grabbers[(i + 1) % 2].SelectedObject);
                            }
                        }
                        else if (grabbers[i].CanGrab)
                        {
                            if (!grabbers[i].IsGrabbing)
                            {
                                grabbers[i].Grab();
                            }
                        }
                        else if (!TeleportationStarted[i])
                        {
                            hands[i].GetRootPose(out Pose handPose);
                            lineDrawers[i]?.FixateEndPoint(handPose.position);
                            TeleportationStarted[i] = true;
                        }
                    }
                }
                else
                {
                    if (LocomotionType == LocomotionType.Bow)
                    {
                        // Bow was drawn
                        if (TeleportationStarted[i] && TeleportationStarted[(i + 1) % 2])
                        {
                            // Was the bow released? -> cancel shot
                            if (i == bowIndex)
                            {
                                TeleportationStarted[i] = false;
                                // The other hand is now the bow
                                // bowIndex = (i + 1) % 2;
                            }
                            // The bowstring was release -> execute shot
                            else
                            {
                                Teleport(i);
                            }
                            lineDrawerRightHand.SetDrawArrowTrajectory(false);
                        }
                    }
                    else
                    {
                        if (grabbers[i].IsRotating)
                        {
                            grabbers[i].ReleaseRotation();
                        }
                        else if (grabbers[i].IsGrabbing)
                        {
                            grabbers[i].ReleaseGrab();
                        }
                        else if (TeleportationStarted[i])
                        {
                            lineDrawers[i]?.ReleaseFixedEndpoint();
                            Teleport(i);
                            TeleportationStarted[i] = false;
                        }
                    }
                }
            }
		}
    }

    const float g = 9.81f;

    public Vector3[] CalculateArrowTrajectory()
    {
        List<Vector3> path = new List<Vector3>();

        hands[bowIndex].GetRootPose(out Pose bowPose);
        hands[(bowIndex + 1) % 2].GetRootPose(out Pose stringPose);
        Vector3 bowPosition = bowPose.position;
        Vector3 stringPosition = stringPose.position;

        Vector3 direction = bowPosition - stringPosition;
        Vector3 groundDirection = new Vector3(direction.x, 0, direction.z).normalized;
        float force = direction.magnitude;
        float v0 = force * force * 100;
        float h0 = bowPosition.y;
        float beta = Mathf.Acos(Mathf.Abs(Vector3.Dot(Vector3.up, direction)) / (Vector3.up.magnitude * direction.magnitude));

        float step = 0.25f;
        float y = h0;

        for (float x = 0; y < h0 - 20; x += step)
        {
            y = x * Mathf.Tan(beta) - (g * Mathf.Pow(x, 2)) / (2 * Mathf.Pow(v0, 2) * Mathf.Pow(Mathf.Cos(x), 2)) + h0;
            path.Add(bowPosition + groundDirection * x + new Vector3(0, y, 0));
        }
        
        return path.ToArray();
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

    public void SwitchToCalibrationScene()
    {
        SceneManager.LoadScene("Calibration");
    }

    private RaycastHit[] hits = new RaycastHit[20];
    const int rayY = 200;

    private void Teleport(int handIndex)
    {
        Hand hand = hands[handIndex];
        if (hand.GetRootPose(out Pose handPose) && hmd.GetRootPose(out Pose hmdPose))
        {
            float newRigRotation = (360 + transform.InverseTransformDirection(0, handPose.rotation.eulerAngles.y, 0).y - 90 - hmdPose.rotation.eulerAngles.y) % 360;
            //Debug.LogWarning("hand rotation: " + handPose.rotation.eulerAngles);
            //Debug.LogWarning("hmd rotation: " + hmdPose.rotation.eulerAngles);
            //Debug.LogWarning("camera rig rotation: " + transform.rotation.eulerAngles);
            //Debug.LogWarning("new camera rig rotation: " + newRigRotation);

            float newY = transform.position.y;
            if (Physics.Raycast(handPose.position + rayY * Vector3.up, Vector3.down, out RaycastHit hit, 2 * rayY, LayerMask.GetMask("Terrain")))
            {
                newY = hit.point.y;
            }

            Vector3 newPosition = new Vector3(handPose.position.x, newY, handPose.position.z);
            Quaternion newOrientation = Quaternion.Euler(transform.rotation.eulerAngles.x, newRigRotation, transform.rotation.eulerAngles.z);

            // add some height so the ray is able to hit coins
            Vector3 origin = transform.position + new Vector3(0, 1.5f, 0); 
            bool coinHit = false;
            bool terrainHit = false;

            Vector3[] points = LocomotionTechnique.LocomotionType == LocomotionType.Bow ?
                CalculateArrowTrajectory()
                : lineDrawers[handIndex]?.GetPoints() ?? Array.Empty<Vector3>();

            // collect everything on the path of the curve indicator
            for (int i = 0; i < points.Length - 1; i++)
            {
                if (LocomotionTechnique.LocomotionType == LocomotionType.Bow)
                {
                    if (terrainHit == false)
                    {
                        int hitCount = Physics.RaycastNonAlloc(points[i], points[i + 1] - points[i], hits, (points[i + 1] - points[i]).magnitude, LayerMask.GetMask("Terrain"));
                        for (int j = 0; j < hitCount; j++)
                        {
                            newPosition = hits[i].point;
                            newOrientation = transform.rotation; // dont change rotation
                            terrainHit = true;
                        }
                    }
                }
                coinHit |= CastRay(points[i], points[i + 1] - points[i], "locomotion");
            }

            // collect everything in a straight line from current position to new position
            coinHit |= CastRay(origin, newPosition - transform.position, "locomotion");

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

    private bool CastRay(Vector3 origin, Vector3 direction, params string[] mask)
    {
        bool coinHit = false;
        int hitCount = Physics.RaycastNonAlloc(origin, direction, hits, direction.magnitude, LayerMask.GetMask(mask));
		for (int i = 0; i < hitCount; i++)
		{
			GameObject hitObject = hits[i].collider.gameObject;

			if (hitObject.CompareTag("coin"))
			{
				parkourCounter.coinCount++;
				hitObject.SetActive(false);
				coinHit = true;
			}
			else if (hitObject.CompareTag("banner"))
			{
				stage = hitObject.name;
				parkourCounter.isStageChange = true;
			}
		}
        return coinHit;
	}
}