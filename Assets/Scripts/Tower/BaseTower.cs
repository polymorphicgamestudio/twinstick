using UnityEngine;

public class BaseTower : MonoBehaviour {

    public ushort objectID;

    public float timer;
    public float currentTimer;

    public float minDist = 0f;
    public float maxDist = 20f;

    public Transform slimeTarget;

	public Transform rotBoneHoz;
	public Transform rotBoneVert;
	Animator animator;

	private void Start() {
        ShepProject.ShepGM.inst.EnemyManager.AddTowerToList(this);
		animator = GetComponent<Animator>();
		animator.SetTrigger("Wake");
	}

    // Update is called once per frame
    public virtual void Update() {
		if (!slimeTarget || slimeTarget.gameObject.activeInHierarchy)
			SearchForSlime();
		else {
			Quaternion slimeDirection = Quaternion.LookRotation(slimeTarget.position - transform.position);
			rotBoneHoz.rotation = Quaternion.RotateTowards(rotBoneHoz.rotation, slimeDirection, Time.deltaTime * 90);
		}
        
        currentTimer -= Time.deltaTime;
        if (currentTimer > 0)
            return;

        if (slimeTarget)
			ShootTurret();

		currentTimer = timer;
	}

    public void SearchForSlime() {
        //slimeTarget = ShepProject.ShepGM.inst.EnemyManager.QuadTree.GetClosestVisibleObject(objectID, ShepProject.ObjectType.Slime, minDist, maxDist);
        slimeTarget = ShepProject.ShepGM.inst.EnemyManager.QuadTree.GetClosestObject(objectID, ShepProject.ObjectType.Slime, minDist, maxDist);
    }

    public virtual void ShootTurret() {
		Debug.Log("PEW!");
    }
}