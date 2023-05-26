using Unity.Mathematics;
using UnityEngine;

public abstract class BaseTower : MonoBehaviour {

    public ushort objectID;

    public float timer;
    public float currentTimer;

    public float minDist = 0f;
    public float maxDist = 20f;

    private float maxDistSquared;
    private float targetSqrDistance;


    public bool NeedsTarget => slimeTarget == null;
    public Transform slimeTarget;

	public Transform rotBoneHoz;
	public Transform rotBoneVert;
	public Animator animator;

    public LayerMask mask;

	private void Start() 
    {
		animator = GetComponent<Animator>();
		animator.SetTrigger("Wake");

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

        currentTimer -= Time.deltaTime;

        if (currentTimer > 0)
            return;

        if (slimeTarget != null)
            ShootTurret();

        currentTimer = timer;


    }


    public abstract void ShootTurret();

}