using Oculus.Interaction;
using Oculus.Interaction.Input;
using OculusSampleFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using Pose = UnityEngine.Pose;

public class ExtendedTrackingToWorldTransformer : MonoBehaviour, ITrackingToWorldTransformer
{
    private Vector3 rootOffset = new Vector3();

    [SerializeField]
    private LocomotionTechnique locomotion;

    [SerializeField]
    private Handedness Hand;

    [SerializeField]
    private Transform interactionRigTransform;

    [SerializeField, Interface(typeof(IOVRCameraRigRef))]
    private MonoBehaviour _cameraRigRef;
    public IOVRCameraRigRef CameraRigRef { get; private set; }

    [SerializeField, Interface(typeof(HmdRef))]
    private MonoBehaviour _hmdRef;
    public HmdRef HmdRef { get; private set; }

    public Transform Transform => CameraRigRef.CameraRig.transform;

    public Quaternion WorldToTrackingWristJointFixup => FromOVRHandDataSource.WristFixupRotation;

    const int k = 10;
    float a = 2;

    const int rayY = 200;

    int handIndex;

    /// <summary>
    /// Converts a tracking space pose to a world space pose (Applies any transform applied to the OVRCameraRig)
    /// </summary>
    public Pose ToWorldPose(Pose pose)
    {
        GetComponent<HmdRef>().GetRootPose(out Pose hmdPose);
        Transform trackingToWorldSpace = Transform;

        if (LocomotionTechnique.LocomotionType == LocomotionType.Bow)
        {
            pose.position = trackingToWorldSpace.TransformPoint(pose.position);
            pose.rotation = trackingToWorldSpace.rotation * pose.rotation;
            return pose;
        }
        else
        {
            Quaternion hmdRotationY = Quaternion.Euler(new Vector3(0, hmdPose.rotation.eulerAngles.y, 0));
            Vector3 rootPosition = trackingToWorldSpace.InverseTransformPoint(hmdPose.position + hmdRotationY * rootOffset);
            float xOffset = pose.position.x - rootPosition.x;
            float zOffset = pose.position.z - rootPosition.z;

            if (locomotion.grabbers[handIndex].IsGrabbing)
            {
                a = 0.5f;
            }
            else
            {
                a = 2f;
            }

            float newX = (xOffset > 0 ? 1 : (-1)) * a * Mathf.Pow(xOffset * k, 2) + pose.position.x;
            float newZ = (zOffset > 0 ? 1 : (-1)) * a * Mathf.Pow(zOffset * k, 2) + pose.position.z;

            // get surface height;
            float newY = pose.position.y;
            if (Physics.Raycast(trackingToWorldSpace.TransformPoint(new Vector3(newX, rayY, newZ)), Vector3.down, out RaycastHit hit, 2 * rayY, LayerMask.GetMask("Terrain")))
            {
                newY += hit.point.y - trackingToWorldSpace.position.y;
            }

            pose.position = trackingToWorldSpace.TransformPoint(new Vector3(newX, newY, newZ));
            pose.rotation = trackingToWorldSpace.rotation * pose.rotation;
        }

        return pose;
    }

    /// <summary>
    /// Converts a world space pose to a tracking space pose (Removes any transform applied to the OVRCameraRig)
    /// </summary>
    public Pose ToTrackingPose(in Pose worldPose)
    {
        Transform trackingToWorldSpace = Transform;
        Vector3 position = trackingToWorldSpace.InverseTransformPoint(worldPose.position);
        Quaternion rotation = Quaternion.Inverse(trackingToWorldSpace.rotation) * worldPose.rotation;
        //return new Pose(position, rotation);

        Pose rootPose;
        GetComponent<HmdRef>().GetRootPose(out rootPose);

        float newX = rootPose.position.x + Mathf.Sqrt(-k * (position.x - rootPose.position.x)) / k;
        float newZ = rootPose.position.z + Mathf.Sqrt(-k * (position.z - rootPose.position.z)) / k;
        position.x = newX;
        position.z = newZ;

        if (ToWorldPose(new Pose(position, rotation)) != worldPose)
        {
            position.x = -position.x;
        }
        if (ToWorldPose(new Pose(position, rotation)) != worldPose)
        {
            position.z = -position.z;
        }
        if (ToWorldPose(new Pose(position, rotation)) != worldPose)
        {
            position.x = -position.x;
        }

        Debug.Assert(ToWorldPose(new Pose(position, rotation)) == worldPose);

        return new Pose(position, rotation);
    }
    
    protected virtual void Awake()
    {
        CameraRigRef = _cameraRigRef as IOVRCameraRigRef;
        HmdRef = _hmdRef as HmdRef;
    }

    protected virtual void Start()
    {
        Assert.IsNotNull(CameraRigRef);
        Assert.IsNotNull(locomotion);
        if (!Calibration.IsCalibrated())
        {
            SceneManager.LoadScene("Calibration");
        }
        switch (Hand)
        {
            case Handedness.Left:
                rootOffset = Calibration.GetLeftHandRootOffset();
                handIndex = 0;
                break;
            case Handedness.Right:
                rootOffset = Calibration.GetRightHandRootOffset();
                handIndex = 1;
                break;
        }
    }

    #region Inject

    public void InjectAllExponentialTrackingToWorldTransformer(HmdRef hmdRef, IOVRCameraRigRef cameraRigRef)
    {
        InjectCameraRigRef(cameraRigRef);
        InjectHmdRef(hmdRef);
    }

    public void InjectCameraRigRef(IOVRCameraRigRef cameraRigRef)
    {
        _cameraRigRef = cameraRigRef as MonoBehaviour;
        CameraRigRef = cameraRigRef;
    }

    public void InjectHmdRef(HmdRef hmdRef)
    {
        _hmdRef = hmdRef;
        HmdRef = hmdRef;
    }

    #endregion
}
