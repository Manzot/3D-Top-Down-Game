using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Cinemachine.CinemachineVirtualCamera thisCamera;
    // Start is called before the first frame update
    void Start()
    {
        thisCamera = GetComponent<Cinemachine.CinemachineVirtualCamera>();
        thisCamera.Follow = PlayerController.Instance.gameObject.transform;
    }
}
