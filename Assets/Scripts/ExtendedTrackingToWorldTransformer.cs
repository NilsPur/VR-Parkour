using Oculus.Interaction;
using Oculus.Interaction.Input;
using OculusSampleFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Pose = UnityEngine.Pose;

public class ExtendedTrackingToWorldTransformer : MonoBehaviour, ITrackingToWorldTransformer
{
    public Vector3 RootOffset = new Vector3();

    public LocomotionTechnique Locomotion;

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

    private bool teleportationHappend = false;
    private Vector3 oldHmdPosition;
    private Quaternion oldHmdOrientation;
    private Pose oldHandPose;

    const int k = 100;

    /// <summary>
    /// Converts a tracking space pose to a world space pose (Applies any transform applied to the OVRCameraRig)
    /// </summary>
    public Pose ToWorldPose(Pose pose)
    {
        GetComponent<HmdRef>().GetRootPose(out Pose rootPose);
        Transform trackingToWorldSpace = Transform;
        float xOffset = pose.position.x - trackingToWorldSpace.InverseTransformPoint(rootPose.position).x;
        float zOffset = pose.position.z - trackingToWorldSpace.InverseTransformPoint(rootPose.position).z;
        float newX = (xOffset > 0 ? 1 : (-1)) * Mathf.Pow(xOffset * k, 2) / k + pose.position.x;
        float newZ = (zOffset > 0 ? 1 : (-1)) * Mathf.Pow(zOffset * k, 2) / k + pose.position.z;
        pose.position = trackingToWorldSpace.TransformPoint(new Vector3(newX, pose.position.y, newZ));
        pose.rotation = trackingToWorldSpace.rotation * pose.rotation;
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
        Assert.IsNotNull(Locomotion);
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
