using UnityEngine;

public class DragSprite : MonoBehaviour {
    public GameObject selectedTile;
    public Vector3 center, touchPosition, offset, updatedCenter;
    private RaycastHit _hit;
    public bool dragging;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            Debug.Log("Pressed");
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out _hit)) {
                Debug.Log(_hit.ToString());
                selectedTile = _hit.collider.gameObject;
                center = selectedTile.transform.position;
                touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                offset = touchPosition - center;
                dragging = true;

            }
        }

        if (Input.GetMouseButton(0)) {
            if (dragging) {
                touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                updatedCenter = touchPosition - offset;
                selectedTile.transform.position = updatedCenter;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }
    }



}
