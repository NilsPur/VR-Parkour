using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabber : MonoBehaviour
{
    private Quaternion currentRotation;
    public bool IsRotating { get; private set; }
    public bool CanGrab { get; private set; }
    public bool IsGrabbing { get; set; }

    public GameObject SelectedObject { get; private set; }

    public SelectionTaskMeasure selectionTaskMeasure;

    public GameObject GrabbingObject;

    public void Update()
    {
        if (IsRotating)
        {
            if (SelectedObject != null)
            {
                Quaternion relativRotation = Quaternion.Inverse(currentRotation) * GrabbingObject.transform.rotation;
                SelectedObject.transform.parent.transform.rotation *= relativRotation;
                currentRotation = GrabbingObject.transform.rotation;
            }
        }
    }

    public void Grab()
    {
        if (SelectedObject != null)
        {
            SelectedObject.transform.parent.transform.parent = GrabbingObject.transform;
            IsGrabbing = true;
        }
    }

    public void ReleaseGrab()
    {
        if (SelectedObject != null)
        {
            SelectedObject.transform.parent.transform.parent = null;
        }
        IsGrabbing = false;
    }

    public void Rotate(GameObject selectedObject)
    {
        SelectedObject = selectedObject;
        IsRotating = true;
        currentRotation = GrabbingObject.transform.rotation;         
    }

    public void ReleaseRotation()
    {
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
                selectionTaskMeasure.isTaskStart = false;
                selectionTaskMeasure.EndOneTask();
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