using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabber : MonoBehaviour
{
	private Quaternion lastGrabberRotation;
    public bool IsRotating { get; private set; }
    public bool CanGrab { get; private set; }
    public bool IsGrabbing { get; set; }

    public GameObject SelectedObject { get; private set; }

    public SelectionTaskMeasure selectionTaskMeasure;

    public GameObject GrabbingObject;

    public void Update()
    {
        if (SelectedObject == null)
        {
            IsGrabbing = false;
            IsRotating = false;
        }
        else if (IsRotating)
        {
            Quaternion currentRotationOffset = Quaternion.Inverse(lastGrabberRotation) * GrabbingObject.transform.rotation;
            Vector3 rotationAngle = currentRotationOffset.eulerAngles;
            rotationAngle = new Vector3(rotationAngle.z, rotationAngle.x, rotationAngle.y); // hand rotation needs to be mapped
            SelectedObject.transform.parent.transform.Rotate(rotationAngle, Space.World);
            lastGrabberRotation = GrabbingObject.transform.rotation;
        }
		else if (IsGrabbing)
        {
			SelectedObject.transform.parent.transform.position = GrabbingObject.transform.position;
		}
	}

    public void Grab()
    {
        if (SelectedObject != null)
        {
            //SelectedObject.transform.parent.transform.parent = GrabbingObject.transform;
            IsGrabbing = true;
        }
    }

    public void ReleaseGrab()
    {
        if (SelectedObject != null)
        {
            //SelectedObject.transform.parent.transform.parent = null;
        }
        IsGrabbing = false;
    }

    public void Rotate(GameObject selectedObject)
    {
        SelectedObject = selectedObject;
        IsRotating = true;
        lastGrabberRotation = GrabbingObject.transform.rotation;
        Debug.LogWarning("Start Hand: " + GrabbingObject.transform.rotation.eulerAngles);
		Debug.LogWarning("Start T: " + SelectedObject.transform.parent.transform.rotation.eulerAngles);
	}

    public void ReleaseRotation()
    {
		Debug.LogWarning("End Hand: " + GrabbingObject.transform.rotation.eulerAngles);
		Debug.LogWarning("End T: " + SelectedObject.transform.parent.transform.rotation.eulerAngles);
		SelectedObject = null;
        IsRotating = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsRotating && !IsGrabbing)
        {
            if (other.gameObject.CompareTag("objectT"))
            {
                CanGrab = true;
                SelectedObject = other.gameObject;
            }
            else if (other.gameObject.CompareTag("selectionTaskStart"))
            {
                if (!selectionTaskMeasure.isCountdown)
                {
                    selectionTaskMeasure.isTaskStart = true;
                    selectionTaskMeasure.StartOneTask();
                }
            }
            else if (other.gameObject.CompareTag("done"))
            {
                if (selectionTaskMeasure.taskTime > 1)
                {
					selectionTaskMeasure.isTaskStart = false;
					selectionTaskMeasure.EndOneTask();
				}
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsRotating && !IsGrabbing)
        {
            if (other.gameObject.CompareTag("objectT"))
            {
                CanGrab = false;
                IsGrabbing = false;
                SelectedObject = null;
            }
        }
    }
}