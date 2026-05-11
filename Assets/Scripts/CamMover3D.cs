using UnityEngine;

[DisallowMultipleComponent]
public class CamMover3D : MonoBehaviour {
    [Header("Look")]
    [SerializeField]
    float mouseSensitivity = 2.2f;
    [SerializeField]
    bool invertY;
    [SerializeField]
    bool lockCursorWhileLooking = true;
    [SerializeField, Range(-89f, 89f)]
    float minPitch = -89f;
    [SerializeField, Range(-89f, 89f)]
    float maxPitch = 89f;

    [Header("Movement")]
    [SerializeField, Min(0.01f)]
    float baseMoveSpeed = 6f;
    [SerializeField, Min(1f)]
    float fastMultiplier = 3f;
    [SerializeField, Min(0.01f)]
    float slowMultiplier = 0.35f;
    [SerializeField, Min(0f)]
    float acceleration = 18f;
    [SerializeField, Min(0f)]
    float deceleration = 22f;
    [SerializeField]
    bool moveOnlyWhileLooking = true;

    [Header("Speed Scroll")]
    [SerializeField, Min(0f)]
    float scrollStep = 1.5f;
    [SerializeField, Min(0.01f)]
    float minBaseSpeed = 0.5f;
    [SerializeField, Min(0.01f)]
    float maxBaseSpeed = 120f;

    float yaw;
    float pitch;
    Vector3 currentVelocity;

    void Awake() {
        var angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = NormalizePitch(angles.x);
    }

    void OnDisable() {
        if (lockCursorWhileLooking) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void Update() {
        ApplyScrollSpeed();

        var isLooking = Input.GetMouseButton(1);
        if (lockCursorWhileLooking) {
            Cursor.lockState = isLooking ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isLooking;
        }

        if (isLooking) {
            RotateFromMouse();
        }

        MoveCamera(isLooking);
    }

    void ApplyScrollSpeed() {
        var wheel = Input.mouseScrollDelta.y;
        if (Mathf.Approximately(wheel, 0f)) {
            return;
        }

        baseMoveSpeed = Mathf.Clamp(baseMoveSpeed + wheel * scrollStep, minBaseSpeed, maxBaseSpeed);
    }

    void RotateFromMouse() {
        var mouseX = Input.GetAxisRaw("Mouse X");
        var mouseY = Input.GetAxisRaw("Mouse Y") * (invertY ? 1f : -1f);

        yaw += mouseX * mouseSensitivity;
        pitch = Mathf.Clamp(pitch + mouseY * mouseSensitivity, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void MoveCamera(bool isLooking) {
        if (moveOnlyWhileLooking && !isLooking) {
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, 1f - Mathf.Exp(-deceleration * Time.deltaTime));
            transform.position += currentVelocity * Time.deltaTime;
            return;
        }

        var forward = Input.GetAxisRaw("Vertical");
        var right = Input.GetAxisRaw("Horizontal");
        var up = 0f;
        if (Input.GetKey(KeyCode.E)) up += 1f;
        if (Input.GetKey(KeyCode.Q)) up -= 1f;

        var moveInput = (transform.forward * forward) + (transform.right * right) + (Vector3.up * up);
        if (moveInput.sqrMagnitude > 1f) {
            moveInput.Normalize();
        }

        var speedMultiplier = 1f;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
            speedMultiplier = fastMultiplier;
        } else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
            speedMultiplier = slowMultiplier;
        }

        var targetVelocity = moveInput * (baseMoveSpeed * speedMultiplier);
        var blendRate = targetVelocity.sqrMagnitude > currentVelocity.sqrMagnitude ? acceleration : deceleration;
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 1f - Mathf.Exp(-blendRate * Time.deltaTime));

        transform.position += currentVelocity * Time.deltaTime;
    }

    static float NormalizePitch(float rawPitch) {
        if (rawPitch > 180f) {
            rawPitch -= 360f;
        }

        return rawPitch;
    }
}
