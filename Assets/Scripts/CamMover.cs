using Unity.VisualScripting;

using UnityEngine;

class CamMover : MonoBehaviour {
    void Update() {
        Camera.main.transform.position += Time.deltaTime * Camera.main.orthographicSize / 4 * new Vector3(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );

        Camera.main.orthographicSize *= Mathf.Pow(1.3f, -Input.GetAxis("Mouse ScrollWheel"));
    }
}
