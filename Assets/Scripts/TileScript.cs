using UnityEngine;
using System.Collections;

public class TileScript : MonoBehaviour {
    private Rigidbody2D _rb;
    private static float _maxSpeed = 1.5f;
    private Scenario2 _s2;

	private void Start () {
        _rb = GetComponent<Rigidbody2D>();
        _s2 = Camera.main.GetComponent<Scenario2>();
	}
	
	private void FixedUpdate () {

        _rb.angularVelocity = Mathf.Clamp(_rb.angularVelocity, 0, 10f);

        if (_rb.velocity.magnitude > _maxSpeed) {
            _rb.velocity = _rb.velocity.normalized * _maxSpeed;
        }

        if(_rb.velocity.magnitude < 0.05f) {
            //Debug.Log(string.Format("{0} is too slow! Adding more force to {1}!", _rb.velocity.magnitude, gameObject.name));
            _rb.AddForce(transform.position * -.15f);
        }

    }

    private void OnCollisionEnter2D(Collision2D c) {
        if (_s2.gameState != Scenario2.GameState.Active)
            return;

        if (_rb.gravityScale > 0)
            _rb.gravityScale = 0;

        switch (c.gameObject.name) {
            case "Top":
                _rb.AddForce(transform.up * -.1f);
                break;
            case "Bottom":
                _rb.AddForce(transform.up * .1f);
                break;
            case "Left":
                _rb.AddForce(transform.right * .1f);
                break;
            case "Right":
                _rb.AddForce(transform.right * -.1f);
                break;
            default:
                //Debug.Log(string.Format("Unrecognised object detected - '{0}'.", c.gameObject.name));
                break;
        }
    }

    private void OnMouseDown() {
        if (_s2.gameState != Scenario2.GameState.Active)
            return;

        _s2.ClickTile(gameObject);
    }
}
