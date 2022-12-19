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
    private Hmd hmdRef;
    [SerializeField]
    private Hand leftHand;
    [SerializeField]
    private Hand rightHand;

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
        if (rightHand.GetRootPose(out Pose handPose) && hmdRef.GetRootPose(out Pose hmdPose))
        {
            DrawCurve(hmdPose.position - new Vector3(0.05f, hmdPose.position.y, 0.05f), handPose.position);
        }
    }

    public void DrawCurve(Vector3 from, Vector3 to)
    {
        Matrix4x4 xMatrix = new Matrix4x4(new Vector4(0, 0, 1, 0), new Vector4(0.25f, 0.5f, 1, 0),
            new Vector4(1, 1, 1, 0), new Vector4(0, 0, 0, 1));
        Vector4 yVector = new Vector4(from.y, Mathf.Max(to.y, from.y) + Mathf.Min(0.5f, (from - to).sqrMagnitude / 4), to.y, 1);

        Vector4 cVector = xMatrix.transpose.inverse * yVector;

        int pointCount = 100; //10 * (Mathf.Max(from.x - to.x, from.z - to.z) + 1);
        Vector3 offset = new Vector3((to.x - from.x) / pointCount, 0, (to.z - from.z) / pointCount);
        List<Vector3> positions = new List<Vector3>();
        for (Vector3 i = new Vector3(from.x, from.y, from.z);
            SolveLinearEquation(to.x - from.x, from.x, i.x) <= 1 && SolveLinearEquation(to.z - from.z, from.z, i.z) <= 1;
            i += offset)
        {
            float x = (i.x - from.x) / (to.x - from.x); // <=> (to.x-from.x)*x+from.x=i.x <=> a*x+b=i 
            float y = cVector[0] * Mathf.Pow(x, 2) + cVector[1] * x + cVector[2]; // a*x^2 + b*x + c
            positions.Add(new Vector3(i.x, y, i.z));
        }
        lineRenderer.positionCount = pointCount;
        lineRenderer.SetPositions(positions.ToArray());
    }

    /// <summary>
    /// Solves a*x + b = y <==> x = (y-b) / a
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="y"></param>
    private float SolveLinearEquation(float a, float b, float y) => (y - b) / a;
}
