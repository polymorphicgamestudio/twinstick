using UnityEngine;
using UnityEngine.UIElements;

public class TowerBuildUpEffect : MonoBehaviour {

	Vector3 towerPos;
	Quaternion towerRot;
	GameObject tower;

	public void Initialize(Vector3 towerPosition ,Quaternion towerRotation, float buildTime, GameObject towerObject) {
		GetComponent<Animator>().SetTrigger("Build");
		towerPos = towerPosition;
		towerRot = towerRotation;
		tower = towerObject;
		//Rotate this tower

		Invoke("ReplaceWithTurret", buildTime);
	}
	void ReplaceWithTurret() {
		GameObject newTower = Instantiate(tower,towerPos,Quaternion.identity);
		//rotate new tower

		Destroy(gameObject);
	}

	/*
	void RotateTower(Transform root) {
		foreach (GameObject child in root) {
			if (child.name == "Pivot")
				child.transform.rotation = towerRot;
		}
	}*/
}