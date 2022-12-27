using Oculus.Interaction.Input;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Line : MonoBehaviour
{
    private LineRenderer lineRenderer;
    [SerializeField]
    private OVRCameraRig cameraRig;
    [SerializeField]
    private Hmd hmd;
    [SerializeField]
    private Hand leftHand;
    [SerializeField]
    private Hand rightHand;
    [SerializeField]
    private LocomotionTechnique locomotion;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
    }

    // Update is called once per frame
    void Update()
    {
        if (rightHand.GetRootPose(out Pose handPose) && hmd.GetRootPose(out Pose hmdPose))
        {
            //Vector3 endPosition = locomotion.TeleportationStarted[1] ? locomotion.StartTeleportationPose[1].position : handPose.position;
            DrawCurve(new Vector3(hmdPose.position.x - 0.05f, cameraRig.transform.position.y, hmdPose.position.z - 0.05f), handPose.position, Vector3.up);
        }
    }

    public void DrawCurve(Vector3 from, Vector3 to, Vector3 rotation)
    {
        //// x-values
        //Matrix4x4 xMatrix = new Matrix4x4(
        //    new Vector4(0, 0, 1, 0),
        //    new Vector4(0.25f, 0.5f, 1, 0),
        //    new Vector4(1, 1, 1, 0),
        //    new Vector4(0, 0, 0, 1)
        //);
        //// y-values
        //Vector4 yVector = new Vector4(0, Mathf.Min(0.5f, directionLength / 4), 0, 1);
        //// coefficients
        //Vector4 cVector = xMatrix.transpose.inverse * yVector;
        Vector3 direction = to - from;
        float directionLength = direction.magnitude;

        float c = Mathf.Min(1f, directionLength / 4);

        int pointCount = 100;//(int)Mathf.Max(-15 * Mathf.Log(2, directionLength + 1) + 100, 15);//(int)Mathf.Max(directionLength / 10, 20);

        Vector3 position = new Vector3(from.x, from.y, from.z);
        Vector3 positionOffset = direction / (pointCount - 1);

        float x = -1;
        float xOffset = 2f / (pointCount - 1);

        Vector3[] positions = new Vector3[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            //float y = cVector[0] * Mathf.Pow(x, 2) + cVector[1] * x + cVector[2]; // a*x^2 + b*x + c
            float y = -c * Mathf.Pow(x, 2) + c;
            positions[i] = position + y * rotation;
            position += positionOffset;
            x += xOffset;
        }

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }

    private Vector4 GetVectorX(float x) => new Vector4(x * x, x, 1, 0);
}
