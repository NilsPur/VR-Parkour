using Oculus.Interaction;
using Oculus.Interaction.Input;
using OculusSampleFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using static OVRPlugin;
using Pose = UnityEngine.Pose;

public class Exponential : MonoBehaviour, ITrackingToWorldTransformer
{
    [SerializeField, Interface(typeof(IOVRCameraRigRef))]
    private MonoBehaviour _cameraRigRef;
    public IOVRCameraRigRef CameraRigRef { get; private set; }

    public Transform Transform => CameraRigRef.CameraRig.transform;

    public Quaternion WorldToTrackingWristJointFixup => FromOVRHandDataSource.WristFixupRotation;

    /// <summary>
    /// Converts a world space pose to a tracking space pose (Removes any transform applied to the OVRCameraRig)
    /// </summary>
    public Pose ToWorldPose(Pose trackingPose)
    {
        Transform trackingToWorldSpace = Transform;

        trackingPose.position = trackingToWorldSpace.TransformPoint(trackingPose.position);
        trackingPose.rotation = trackingToWorldSpace.rotation * trackingPose.rotation;
        return trackingPose;
    }

    /// <summary>
    /// Converts a tracking space pose to a world space pose (Applies any transform applied to the OVRCameraRig)
    /// </summary>
    public Pose ToTrackingPose(in Pose worldPose)
    {
        Transform trackingToWorldSpace = Transform;

        Vector3 position = trackingToWorldSpace.InverseTransformPoint(worldPose.position);
        Quaternion rotation = Quaternion.Inverse(trackingToWorldSpace.rotation) * worldPose.rotation;

        return new Pose(position, rotation);
    }

    protected virtual void Awake()
    {
        CameraRigRef = _cameraRigRef as IOVRCameraRigRef;
    }

    protected virtual void Start()
    {
        Assert.IsNotNull(CameraRigRef);
    }

    #region Inject

    public void InjectAllTrackingToWorldTransformerOVR(IOVRCameraRigRef cameraRigRef)
    {
        InjectCameraRigRef(cameraRigRef);
    }

    public void InjectCameraRigRef(IOVRCameraRigRef cameraRigRef)
    {
        _cameraRigRef = cameraRigRef as MonoBehaviour;
        CameraRigRef = cameraRigRef;
    }

    #endregion
}
