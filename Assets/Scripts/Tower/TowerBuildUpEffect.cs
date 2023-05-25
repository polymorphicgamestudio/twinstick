using UnityEngine;
using UnityEngine.UIElements;

public class TowerBuildUpEffect : MonoBehaviour {

	Vector3 towerPos;
	Quaternion towerRot;
	GameObject tower;
	[SerializeField] Transform hozRotationBone;

	public void Initialize(Vector3 towerPosition ,Quaternion towerRotation, float buildTime, GameObject towerObject) {
		hozRotationBone.rotation = towerRotation;
		GetComponent<Animator>().SetTrigger("Build");
		towerPos = towerPosition;
		towerRot = towerRotation;
		tower = towerObject;
		Invoke("ReplaceWithTurret", buildTime);
	}
	void ReplaceWithTurret() {
		GameObject newTower = Instantiate(tower,towerPos,Quaternion.identity);
		newTower.GetComponent<BaseTower>().rotBoneHoz.rotation = towerRot;
		Destroy(gameObject);
	}
}