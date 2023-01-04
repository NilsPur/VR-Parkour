using Oculus.Interaction.Input;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Calibration : MonoBehaviour
{
    [SerializeField]
    Hmd hmd;

    [SerializeField]
    Hand leftHand;

    [SerializeField]
    Hand rightHand;

    [SerializeField]
    TextMeshPro text;

    private float sceneLoadTime;

    // Start is called before the first frame update
    void Start()
    {
        sceneLoadTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Calibrate()
    {
        if (Time.time > sceneLoadTime + 2 && hmd.GetRootPose(out Pose hmdPose) && leftHand.GetRootPose(out Pose leftHandPose) && rightHand.GetRootPose(out Pose rightHandPose))
        {
            // get hand offset from hmd normalized in regard of the y rotation of the hmd
            Vector3 leftHandRootOffset = Quaternion.Euler(new Vector3(0, -hmdPose.rotation.eulerAngles.y, 0)) * (leftHandPose.position - hmdPose.position);
            Vector3 rightHandRootOffset = Quaternion.Euler(new Vector3(0, -hmdPose.rotation.eulerAngles.y, 0)) * (rightHandPose.position - hmdPose.position);

            PlayerPrefs.SetString("leftHandRootOffset", SerializeVector3(leftHandRootOffset));
            PlayerPrefs.SetString("rightHandRootOffset", SerializeVector3(rightHandRootOffset));

            StartCoroutine(SwitchScene(5));
        }
    }

    public void CancelCalibration()
    {
        if (!IsCalibrated())
        {
            PlayerPrefs.SetString("leftHandRootOffset", SerializeVector3(Vector3.zero));
            PlayerPrefs.SetString("rightHandRootOffset", SerializeVector3(Vector3.zero));
        }

        StartCoroutine(SwitchScene(5));
    }

    IEnumerator SwitchScene(float waitSeconds)
    {
        text.text = "Calibration finished. If you want to recalibrate make the \"thumps up\" gesture. Switching to main scene...";
        yield return new WaitForSeconds(waitSeconds);
        SceneManager.LoadScene("ParkourChallenge");
    }

    public static string SerializeVector3(Vector3 vector3)
    {
        return vector3.x + " | " + vector3.y + " | " + vector3.z;
    }

    public static Vector3 DeserializeVector3(string vector3String)
    {
        string[] vector3Array = vector3String.Split(" | ");
        Debug.Assert(vector3Array.Length == 3);
        return new Vector3(float.Parse(vector3Array[0]), float.Parse(vector3Array[1]), float.Parse(vector3Array[2]));
    }

    public static Vector3 GetLeftHandRootOffset()
    {
        return DeserializeVector3(PlayerPrefs.GetString("leftHandRootOffset"));
    }

    public static Vector3 GetRightHandRootOffset()
    {
        return DeserializeVector3(PlayerPrefs.GetString("rightHandRootOffset"));
    }

    public static bool IsCalibrated()
    {
        return !string.IsNullOrEmpty(PlayerPrefs.GetString("leftHandRootOffset")) && !string.IsNullOrEmpty(PlayerPrefs.GetString("rightHandRootOffset"));
    }
}
