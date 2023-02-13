using Oculus.Interaction.Input;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineDrawer : MonoBehaviour
{
    private bool endPointFixed;
    private Vector3 endPoint;
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
        if (LocomotionTechnique.LocomotionType == LocomotionType.Bow)
        {
            if (drawArrowTrajectory)
            {
                
            }
        }
        else
		{
            if (rightHand.GetRootPose(out Pose handPose) && hmd.GetRootPose(out Pose hmdPose))
            {
                Vector3 from = new Vector3(hmdPose.position.x - 0.05f, cameraRig.transform.position.y, hmdPose.position.z - 0.05f);
                Vector3 to = endPointFixed ? endPoint : handPose.position;

                Vector3 direction = to - from;
                float directionLength = direction.magnitude;

                float c = Mathf.Min(1f, directionLength / 4);

                int pointCount = 100;//(int)Mathf.Max(-15 * Mathf.Log(2, directionLength + 1) + 100, 15);//(int)Mathf.Max(directionLength / 10, 20);

                Vector3 position = new Vector3(from.x, from.y, from.z);
                Vector3 positionOffset = direction / (pointCount - 1);

                float x = -1;
                float xOffset = 2f / (pointCount - 1);

                float stretch = 0f;
                Vector3 rotation = Vector3.up;

                if (endPointFixed)
                {
                    // r = n * (p - f) / n * n
                    // q(r) = f + r * n
                    Vector3 perpendicularPoint = from + direction * Vector3.Dot(direction, handPose.position - from) / Vector3.Dot(direction, direction);
                    Vector3 distance = handPose.position - perpendicularPoint;
                    stretch = distance.magnitude * 10;
                    rotation = distance.normalized;
                }

                Vector3[] positions = new Vector3[pointCount];

                for (int i = 0; i < pointCount; i++)
                {
                    //float y = cVector[0] * Mathf.Pow(x, 2) + cVector[1] * x + cVector[2]; // a*x^2 + b*x + c
                    float y = -(c + stretch) * Mathf.Pow(x, 2) + (c + stretch);
                    positions[i] = position + y * rotation;
                    position += positionOffset;
                    x += xOffset;
                }

                lineRenderer.positionCount = positions.Length;
                lineRenderer.SetPositions(positions);
            }
        }
	}

    bool drawArrowTrajectory = false;

    public void SetDrawArrowTrajectory(bool value)
    {
        drawArrowTrajectory = value;
    }

    public Vector3[] GetPoints()
	{
		Vector3[] points = new Vector3[lineRenderer.positionCount];
		lineRenderer.GetPositions(points);
		return points;
	}

    public void FixateEndPoint(Vector3 endPoint)
    {
        this.endPoint = endPoint;
        endPointFixed = true;
    }

    public void ReleaseFixedEndpoint()
    {
		endPointFixed = false;
	}

    private Vector4 GetVectorX(float x) => new Vector4(x * x, x, 1, 0);
}
