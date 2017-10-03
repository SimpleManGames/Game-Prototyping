using System;
using UnityEditor;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Transform target = null;

    [Header("Lock On Info")]
    [SerializeField]
    private Transform lockOnTarget = null;
    public Transform LockOnTarget
    {
        get { return lockOnTarget; }
    }

    [SerializeField, ReadOnly]
    private bool lockOn;
    public bool LockOn
    {
        get { return lockOn && lockOnTarget != null; }
        set
        {
            lockOn = value;
        }
    }

    [SerializeField]
    private bool debugLockOn = false;

    private Player playerController;

    private Vector2 mouse;
    private Vector2 finalInput;

    private Vector2 currentRotation;

    [SerializeField]
    private CameraSettings settings;

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
        if (HandleLockOn())
            return;

        float inputX = playerController.input.Current.RightStickInput.x;
        float inputZ = playerController.input.Current.RightStickInput.y;

        mouse = playerController.input.Current.MouseInput;
        finalInput = new Vector2(inputX + mouse[0], inputZ + mouse[1]);

        currentRotation[1] += finalInput.x * settings.sensitivity * Time.deltaTime;
        currentRotation[0] -= finalInput.y * settings.sensitivity * Time.deltaTime;

        currentRotation[0] = Mathf.Clamp(currentRotation[0], -settings.clampAngle, settings.clampAngle);

        Quaternion localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(new Vector3(currentRotation[0], currentRotation[1], 0.0f)), settings.rotationSmoothTime);

        transform.rotation = localRotation;
    }

    private void LateUpdate()
    {
        float step = settings.moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position + new Vector3(0, settings.verticalOffset, 0), step);
    }

    private bool HandleLockOn()
    {
        if (LockOn)
        {
            if (debugLockOn)
            {
                Debug.Log("Lock On");
                if (target != null && lockOnTarget != null)
                    Debug.DrawLine(target.position, lockOnTarget.position);
            }

            Vector3 targetDirection = lockOnTarget.position - target.position;
            targetDirection.Normalize();
            //targetDirection.y = 0f;

            if (targetDirection == Vector3.zero)
                targetDirection = transform.forward;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            var slerp = Quaternion.Slerp(transform.rotation, targetRotation, settings.lockOnRotationSmoothTime);

            transform.rotation = slerp;
            slerp = Quaternion.Euler(new Vector3(0f, slerp.eulerAngles.y, slerp.eulerAngles.z));
            target.rotation = slerp;

            currentRotation = targetRotation.eulerAngles;

            return true;
        }

        return false;
    }

    private void OnValidate()
    {
        if (cameraTransform == null)
            cameraTransform = GetComponentInChildren<Camera>().transform;

        cameraTransform.localPosition = settings.distance;

        if (target != null)
            transform.position = target.position + new Vector3(0, settings.verticalOffset, 0);
    }

    [Serializable]
    public struct CameraSettings
    {
        public bool lockCursor;

        public float moveSpeed;
        public float sensitivity;
        public Vector3 distance;
        public float verticalOffset;

        [Range(0, 90)]
        public float clampAngle;

        public float rotationSmoothTime;
        public float lockOnRotationSmoothTime;

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