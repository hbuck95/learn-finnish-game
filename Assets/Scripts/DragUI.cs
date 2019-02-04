using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent (typeof(RectTransform))]
public class DragUI : MonoBehaviour, IDragHandler {
    private RectTransform _componentPosition;

    private void Start() {
        _componentPosition = GetComponent<RectTransform>();
    }

    public void OnDrag(PointerEventData data) {
        _componentPosition.position += new Vector3(data.delta.x, data.delta.y);
    }
}