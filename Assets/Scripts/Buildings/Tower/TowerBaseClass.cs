using ShepProject;
using Unity.Mathematics;
using UnityEngine;

public abstract class TowerBaseClass : MonoBehaviour 
{

    public ShepGM gameManager;

    [HideInInspector]
    public ushort objectID;

    public float timer;

    //[HideInInspector]
    public float currentTimer;

    public float minDist = 0f;
    public float maxDist = 20f;
    public float towerDamage;

    private float maxDistSquared;
    private float targetSqrDistance;


    public bool NeedsTarget => slimeTarget == null;

    //[HideInInspector]
    public Transform slimeTarget;

	public Transform rotBoneHoz;
	public Transform rotBoneVert;


	protected Animator animator;

    public LayerMask mask;
    public Transform barrel;

    public abstract bool IsShooting { get; }

    protected virtual void Start() 
    {
		animator = GetComponent<Animator>();
		animator.SetTrigger("Wake");

        //rotBoneHoz.rotation = transform.rotation;
        //transform.rotation = Quaternion.identity;

        maxDistSquared = maxDist * maxDist;

	}

    public virtual void ManualUpdate()
    {

        if (slimeTarget != null)
        {

            targetSqrDistance = math.distancesq(transform.position, slimeTarget.position);

            Quaternion slimeDirection = Quaternion.LookRotation(slimeTarget.position - transform.position);
            rotBoneHoz.rotation = Quaternion.RotateTowards(rotBoneHoz.rotation, slimeDirection, Time.deltaTime * 90);

            if (!slimeTarget.gameObject.activeInHierarchy || targetSqrDistance > maxDistSquared)
            {
                slimeTarget = null;

            }



        }

        if (IsShooting)
            return;

        currentTimer -= Time.deltaTime;

        if (currentTimer > 0)
            return;

        if (slimeTarget != null)
            ShootTurret();

        currentTimer = timer;


    }

    public virtual void EndOfWave()
    {

        
    }


    public virtual void ShootTurret()
    {

        animator.SetTrigger("Shoot");

    }

}