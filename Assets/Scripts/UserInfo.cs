using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class UserInfo : MonoBehaviour {
    private static bool[] _partsCompleted = new bool[3];

    public static void CompletePart(int part) {
        try {
            _partsCompleted[part - 1] = true;
            Debug.Log(string.Format("Player has now completed Part {0}", part));
        } catch (Exception e) {
            Debug.LogError(string.Format("Error! Unable to recognise \"{0}\" as a valid part!", part));
            Debug.LogException(e);
        }
    }

    public static bool CheckPartCompletion(int part) {
        try {
            return _partsCompleted[part-1];
        } catch (Exception e) {
            Debug.LogError(string.Format("Error! Unable to recognise \"{0}\" as a valid part!", part));
            Debug.LogException(e);
            return false;
        }
    }

    public void LoadPart(int part) {
        /*if (!CheckPartCompletion(part-1) && part != 1) {
            Debug.Log(string.Format("Part {0} isn't completed yet!", part-1));
            return;
        }*/

        Debug.Log(string.Format("Loading part {0}...", part));
        SceneManager.LoadSceneAsync(string.Format("Part_{0}", part));
    }

}
