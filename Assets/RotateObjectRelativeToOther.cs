using UnityEngine;

public class RotateObjectRelativeToOther : MonoBehaviour 
{
    public Transform referenceObject;
    public Transform objectToRotate;

    private Quaternion lastReferenceRotation;

    private bool once = true;

    void Start() 
    {
        lastReferenceRotation = referenceObject.rotation;
    }

    void Update() 
    {
        Quaternion currentRotationOffset = Quaternion.Inverse(lastReferenceRotation) * referenceObject.rotation;
        //Vector3 position = objectToRotate.GetComponent<Renderer>().bounds.center;
        Vector3 rotationAngle = currentRotationOffset.eulerAngles;
        rotationAngle = new Vector3(rotationAngle.z, rotationAngle.x, rotationAngle.y);
        objectToRotate.Rotate(rotationAngle, Space.World);
        lastReferenceRotation = referenceObject.rotation;

        // ROTATION TEST
        //if (once) {
        //    once = false;
        //    referenceObject.Rotate(180, 0, 0);
        //}

        referenceObject.Rotate(180f * Time.deltaTime, 0, 0);
    }
}