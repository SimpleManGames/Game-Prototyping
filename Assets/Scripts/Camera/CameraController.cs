using System;
using UnityEditor;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Transform target = null;
    [SerializeField]
    private float verticalOffset;
    private Player playerController;

    private Vector2 mouse;
    private Vector2 finalInput;

    private Vector2 currentRotation;

    public CameraSettings settings;

    private Transform cameraTransform;

    private void OnEnable()
    {
        cameraTransform = GetComponentInChildren<Camera>().transform;
    }

    void Start()
    {
        if (target.GetComponentInParent<Player>())
            playerController = target.GetComponentInParent<Player>();
        else
            Debug.LogError("Target's Parent needs to have a PlayerController component" + target.parent.name);

        settings.ActOnSettings();

        playerController.cameraRigTransform = transform;

        currentRotation = transform.localRotation.eulerAngles;

        if (cameraTransform == null)
            cameraTransform = GetComponentInChildren<Camera>().transform;

        cameraTransform.localPosition = settings.distance;
    }

    void Update()
    {
        float inputX = Input.GetAxis("RightStick Horz");
        float inputZ = Input.GetAxis("RightStick Vert");

        mouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        finalInput = new Vector2(inputX + mouse[0], inputZ + mouse[1]);

        currentRotation[1] += finalInput.x * settings.sensitivity * Time.deltaTime;
        currentRotation[0] -= finalInput.y * settings.sensitivity * Time.deltaTime;

        currentRotation[0] = Mathf.Clamp(currentRotation[0], -settings.clampAngle, settings.clampAngle);

        Quaternion localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(new Vector3(currentRotation[0], currentRotation[1], 0.0f)), settings.rotationSmoothTime);

        transform.rotation = localRotation;
    }

    private void LateUpdate()
    {
        CameraUpdater();
    }

    private void CameraUpdater()
    {
        float step = settings.moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position + new Vector3(0, verticalOffset, 0), step);
    }

    private void OnValidate()
    {
        if (cameraTransform == null)
            cameraTransform = GetComponentInChildren<Camera>().transform;

        cameraTransform.localPosition = settings.distance;

        if (target != null)
            transform.position = target.position + new Vector3(0, verticalOffset, 0);
    }

    [Serializable]
    public struct CameraSettings
    {
        public bool lockCursor;

        public float moveSpeed;
        public float sensitivity;
        public Vector3 distance;

        [Range(0, 90)]
        public float clampAngle;

        public float rotationSmoothTime;

        public void ActOnSettings()
        {
            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void SetDefaultValues()
        {
            sensitivity = 10f;
            moveSpeed = 10f;
            distance = Vector3.one;
            clampAngle = 80.0f;
            rotationSmoothTime = .12f;
        }
    }
}