using Oculus.Interaction;
using Oculus.Interaction.Input;
using OculusSampleFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using static OVRPlugin;
using Pose = UnityEngine.Pose;

public class ExponentialTrackingToWorldTransformer : MonoBehaviour, ITrackingToWorldTransformer
{
    [SerializeField, Interface(typeof(IOVRCameraRigRef))]
    private MonoBehaviour _cameraRigRef;
    public IOVRCameraRigRef CameraRigRef { get; private set; }

    [SerializeField, Interface(typeof(HmdRef))]
    private MonoBehaviour _hmdRef;
    public HmdRef HmdRef { get; private set; }

    public Transform Transform => CameraRigRef.CameraRig.transform;

    public Quaternion WorldToTrackingWristJointFixup => FromOVRHandDataSource.WristFixupRotation;

    const int k = 100;

    /// <summary>
    /// Converts a tracking space pose to a world space pose (Applies any transform applied to the OVRCameraRig)
    /// </summary>
    public Pose ToWorldPose(Pose pose)
    {
        Transform trackingToWorldSpace = Transform;
        Pose rootPose;
        GetComponent<HmdRef>().GetRootPose(out rootPose);
        float xOffset = pose.position.x - rootPose.position.x;
        float zOffset = pose.position.z - rootPose.position.z;
        float newX = Mathf.Pow(xOffset * k, 2) / k + rootPose.position.x;
        float newZ = Mathf.Pow(zOffset * k, 2) / k + rootPose.position.z;
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

        Pose rootPose;
        GetComponent<HmdRef>().GetRootPose(out rootPose);
        float newX = rootPose.position.x + Mathf.Sqrt(-k * (position.x - rootPose.position.x)) / k;
        float newZ = rootPose.position.z + Mathf.Sqrt(-k * (position.z - rootPose.position.z)) / k;
        position.x = newX;
        position.z = newZ;

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
        Assert.IsNotNull(CameraRigRef);
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
