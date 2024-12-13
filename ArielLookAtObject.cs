using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ArielLookAtObject : MonoBehaviour
{
    [SerializeField] private bool debug;
    private MeshRenderer _meshRender;
    [SerializeField] private CameraManager camManager;
    [SerializeField] private CinemachineFreeLook arielCamera;
    [SerializeField] private float ySpeed;
    private Camera _mainCamera;
    private CinemachineBrain _cinemachineBrain;
    [Header("LookAT Cached Values")]
    private Vector3 _cacheLookAtPosition;
    private Vector3 _cacheLookAtRotation;

    [Header("Cinemachine Cached Values")]
    private CinemachineFreeLook.Orbit[] originalOrbits;
    private Vector3 _cachedPosition;
    private Vector3 _cachedRotation;
    private float _xValue;
    private float _yValue;

    void Awake()
    {
        _mainCamera = Camera.main;
        _cinemachineBrain = _mainCamera.GetComponent<CinemachineBrain>();
        _cacheLookAtPosition = this.transform.position;
        _cacheLookAtRotation = this.transform.rotation.eulerAngles;
        _meshRender = GetComponent<MeshRenderer>();
        if (arielCamera != null)
        {
            _cachedPosition = arielCamera.transform.position;
            _cachedRotation = arielCamera.transform.rotation.eulerAngles;

            originalOrbits = new CinemachineFreeLook.Orbit[arielCamera.m_Orbits.Length];
            for (int i = 0; i < originalOrbits.Length; i++)
            {
                originalOrbits[i].m_Height = arielCamera.m_Orbits[i].m_Height;
                originalOrbits[i].m_Radius = arielCamera.m_Orbits[i].m_Radius;
            }

            _yValue = arielCamera.m_YAxis.Value;
            _xValue = arielCamera.m_XAxis.Value;
        }
    }

    void Start()
    {
        if (debug)
            _meshRender.enabled = true;
        else
            _meshRender.enabled = false;
    }

    void Update()
    {
        //checking if 5 sec coroutine runs then no rotaion otherwise continously rotating
        if (!camManager.InitialFlyDone)
            transform.Rotate(0, ySpeed * Time.deltaTime, 0);
        else
        {
            this.transform.rotation = Quaternion.Euler(new Vector3(0f, 180, 0f));
            ActivateThisLookAt(ArielCamActive());
        }
    }
    private bool ArielCamActive()
    {
        //checking if arielCamera has priority 65 then calling MoveCamera and RotateCamera fun otherwise not.
        if (arielCamera.m_Priority == 1) return false;
        else return true;
    }

    private void MoveCamera()
    {
        Vector3 inputDir = new Vector3(0f, 0f, 0f);

        if (Input.GetKey(KeyCode.W)) inputDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) inputDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) inputDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) inputDir.x = +1f;

        Vector3 moveDir = transform.forward * inputDir.z + transform.right * inputDir.x;
        float moveSpeed = 50f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    private void RotateCamera()
    {
        float yawnCamera = _mainCamera.transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(transform.rotation.x, yawnCamera, transform.rotation.z), 5 * Time.deltaTime);
    }

    public void ActivateThisLookAt(bool status)
    {
        if (status)
        {
            MoveCamera();
            RotateCamera();
        }
    }

    public IEnumerator ResetLookAtObject()
    {
        this.transform.position = _cacheLookAtPosition;
        this.transform.rotation = Quaternion.Euler(new Vector3(0f, 178f, 0f));
        yield return _cinemachineBrain.IsBlending;
        ResetingAssignedCineCamera();
    }

    private void ResetingAssignedCineCamera()
    {
        //print("Resetti`ng values");
        arielCamera.transform.position = _cachedPosition;
        arielCamera.transform.rotation = Quaternion.Euler(_cachedRotation);
        RestoreOriginalOrbits();
        arielCamera.m_XAxis.Value = _xValue;
        arielCamera.m_YAxis.Value = _yValue;
        arielCamera.m_YAxis.m_InputAxisName = "";
        arielCamera.m_XAxis.m_InputAxisName = "";
        arielCamera.m_XAxis.m_InputAxisValue = 0;
        arielCamera.m_YAxis.m_InputAxisValue = 0;
    }

    private void RestoreOriginalOrbits()
    {
        if (arielCamera != null)
        {
            if (originalOrbits != null)
            {
                for (int i = 0; i < originalOrbits.Length; i++)
                {
                    arielCamera.m_Orbits[i].m_Height = originalOrbits[i].m_Height;
                    arielCamera.m_Orbits[i].m_Radius = originalOrbits[i].m_Radius;
                }
            }
        }
    }
}
