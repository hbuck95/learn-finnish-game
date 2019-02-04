using UnityEngine;
using System.Collections;

public class MoveSprite : MonoBehaviour {
    private float _x, _y;
    private bool _dragging, _foundMatch, _canTransmit;
    private GameObject[] _matches = new GameObject[2];
    private Collider2D _lastCol;
    private BoxCollider2D _myCol;

    private void Start() {
        _myCol = GetComponent<BoxCollider2D>();
        _matches[0] = gameObject;
        _canTransmit = tag == "Transmitter" ? true : false;
     //   Debug.Log(string.Format("This {0} {1} transmit!", name, _canTransmit ? "can" : "can't"));
    }

    // Update is called once per frame
    private void Update() {
        _x = Input.mousePosition.x;
        _y = Input.mousePosition.y;
        if (Input.GetMouseButtonUp(0)) {
            _dragging = false;

            if(_lastCol != null)
            {
                Camera.main.SendMessage("_IncorrectMatch", SendMessageOptions.RequireReceiver);
                _lastCol = null;
            }
            // _myCol.enabled = false;
            // Debug.Log("No longer dragging.");
            if (_foundMatch && _canTransmit) {
                StartCoroutine(DestroyMatch());
            }
        }
    }

    private IEnumerator DestroyMatch() {
        yield return new WaitForSeconds(0.5f);
        Camera.main.SendMessage("_MatchMade", _matches, SendMessageOptions.RequireReceiver);
    }

    private void OnMouseDrag() {
        if (!_canTransmit) return;
      //  _myCol.enabled = false;
        _dragging = true;
        transform.position = Camera.main.ScreenToWorldPoint(new Vector3(_x, _y, 10.0f));
    }

    private void CheckMatch(Collider2D c) {
        if (c.name == name) {
            _lastCol = null;
            _matches[1] = c.gameObject;
            _foundMatch = true;
        } else {

            if(_lastCol == null || _lastCol != c) {
                _lastCol = c;
              //  Camera.main.SendMessage("_IncorrectMatch", SendMessageOptions.RequireReceiver);
                return;
            }
            //.Log(string.Format("{0} isn't a match for {1}", c.name, name));
        }
    }

    private void OnTriggerStay2D(Collider2D c) {
        if (_dragging || _foundMatch) {
            CheckMatch(c);
        }
    }

    private void OnTriggerExit2D(Collider2D c) {
        _foundMatch = false;   
    }

}