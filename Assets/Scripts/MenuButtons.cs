using UnityEngine;
using UnityEngine.UI;

public class MenuButtons : MonoBehaviour {
    public Image flag;

    public void Enter() {
        Debug.Log("Enter.");
        flag.enabled = true;
    }

    public void Exit() {
        Debug.Log("Exit.");
        flag.enabled = false;
    }
}
