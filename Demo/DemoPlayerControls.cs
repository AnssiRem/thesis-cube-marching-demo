using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoPlayerControls : MonoBehaviour
{
    [Header("Terrain Editing: ")]
    [SerializeField] float raycastRange = 5.0f;
    [SerializeField] float editRadius = 1.0f;
    [SerializeField] bool useGradient = false;

    [Header("Movement: ")]
    [SerializeField] private float flySpeed = 1.0f;
    [SerializeField] private float shiftSpeedMultiplier = 10f;

    [Header("Mouse Look: ")]
    [SerializeField] private bool smoothing = false;
    [SerializeField] private float smoothingTime = 10f;
    [SerializeField] private float xSens = 2f;
    [SerializeField] private float ySens = 2f;
    private readonly float maxX = 90f;
    private readonly float minX = -90f;
    private Transform playerCamera;
    private Quaternion cameraTargetRot;
    private Transform character;
    private Quaternion characterTargetRot;

    private void MouseLook()
    {
        float xRotation = Input.GetAxisRaw("Mouse Y") * ySens;
        float yRotation = Input.GetAxisRaw("Mouse X") * xSens;

        cameraTargetRot *= Quaternion.Euler(-xRotation, 0f, 0f);
        cameraTargetRot = ClampRotationAroundXAxis(cameraTargetRot);
        characterTargetRot *= Quaternion.Euler(0f, yRotation, 0f);

        if (smoothing)
        {
            character.localRotation =
                Quaternion.Slerp(character.localRotation,
                characterTargetRot,
                smoothingTime * Time.deltaTime);
            playerCamera.localRotation = Quaternion.Slerp(playerCamera.localRotation,
                cameraTargetRot, smoothingTime * Time.deltaTime);
        }
        else
        {
            character.localRotation = characterTargetRot;
            playerCamera.localRotation = cameraTargetRot;
        }
    }

    private void Fly(float timeStep)
    {
        if (Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0)
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                transform.localPosition +
                (playerCamera.forward * Input.GetAxisRaw("Vertical") +
                playerCamera.right * Input.GetAxisRaw("Horizontal")).normalized,
                flySpeed * timeStep *
                Mathf.Pow(shiftSpeedMultiplier, (Input.GetKey(KeyCode.LeftShift)) ? 1 : 0));
        }

        if (Input.GetKey(KeyCode.Space))
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                transform.localPosition + Vector3.up,
                flySpeed * timeStep *
                Mathf.Pow(shiftSpeedMultiplier, (Input.GetKey(KeyCode.LeftShift)) ? 1 : 0));
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                transform.localPosition - Vector3.up,
                flySpeed * timeStep *
                Mathf.Pow(shiftSpeedMultiplier, (Input.GetKey(KeyCode.LeftShift)) ? 1 : 0));
        }
    }

    private void EditTerrain()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit,
                raycastRange))
            {
                List<Chunk> editChunks = new List<Chunk>();
                Collider[] hitColliders = Physics.OverlapSphere(hit.point, editRadius);
                foreach (Collider coll in hitColliders)
                {
                    Chunk chunk = coll.GetComponentInParent<Chunk>();
                    if (chunk != null && !editChunks.Contains(chunk))
                    {
                        editChunks.Add(chunk);
                    }
                }

                foreach (Chunk chunk in editChunks)
                {
                    TerrainEditing.CreateEditTask(chunk, hit.point, editRadius, true, useGradient);
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit,
                raycastRange))
            {
                List<Chunk> editChunks = new List<Chunk>();
                Collider[] hitColliders = Physics.OverlapSphere(hit.point, editRadius);
                foreach (Collider coll in hitColliders)
                {
                    Chunk chunk = coll.GetComponentInParent<Chunk>();
                    if (chunk != null && !editChunks.Contains(chunk))
                    {
                        editChunks.Add(chunk);
                    }
                }

                foreach (Chunk chunk in editChunks)
                {
                    TerrainEditing.CreateEditTask(chunk, hit.point, editRadius, false, useGradient);
                }
            }
        }
    }

    private Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, minX, maxX);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

    private void Start()
    {
        playerCamera = GetComponentInChildren<Camera>().transform;
        character = transform;
        cameraTargetRot = playerCamera.localRotation;
        characterTargetRot = character.localRotation;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        MouseLook();
        EditTerrain();
    }

    private void FixedUpdate()
    {
        Fly(Time.fixedDeltaTime);
    }
}
