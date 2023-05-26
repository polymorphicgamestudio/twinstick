using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class RobotModeController : MonoBehaviour
{

    [SerializeField]
    private ParticleSystem projectorParticles;
    private RobotController controller;
    private bool buildMode;

    public bool BuildMode => buildMode;


    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<RobotController>();
    }

    public void TurnOnProjectorParticles()
    {
        projectorParticles.Play();
    }

    public void TurnOffProjectorParticles()
    {
        projectorParticles.Stop();
    }

    public void TurnOnBuildMode()
    {

        buildMode = true;
        controller.buildMode = true;
        controller.animController.SetTrigger("BuildMode");
        projectorParticles.Play();

    }

    public void TurnOffBuildMode()
    {
        buildMode = false;
        controller.buildMode = false;
        controller.animController.SetTrigger("AttackMode");
        projectorParticles.Stop();

    }

    public Vector3 WallReferencePosition()
    {
        Vector3 wallRefPos = projectorParticles.transform.position + projectorParticles.transform.forward * 5f;
        wallRefPos.y = 0;
        return wallRefPos;
    }
    public Quaternion WallReferenceRotation()
    {
        return Quaternion.Euler(0, projectorParticles.transform.eulerAngles.y, 0);
    }





}
