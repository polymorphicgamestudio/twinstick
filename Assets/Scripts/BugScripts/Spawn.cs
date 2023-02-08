using UnityEngine;

public class Spawn : MonoBehaviour {

	public GameObject critter;
	public int numberToSpawn = 1000;
	int numberSpawned = 0;
	Colors cm;
	public LayerMask layerMask; // optionally ~ operator inverts this when it's being used so that this would be the layer being ignored


	private void Start() {
		cm = GM.cm;
	}
	void Update() {
		if (numberSpawned >= numberToSpawn)
			return;

		int spawnPixelIndex = cm.PixIndexByColour((int)Colors.ColorName.White)[
			Random.Range(0,cm.PixIndexByColour((int)Colors.ColorName.White).Count)];
		Vector3 spawnPoint = cm.pixIndexToVector3(spawnPixelIndex);

		
		RaycastHit hit;
		if (Physics.Raycast(spawnPoint, Vector3.down, out hit, 50, layerMask)) {
			Debug.DrawLine(spawnPoint, hit.point, Color.red);
			spawnPoint.y = hit.point.y;
		}
		Quaternion randomRot = Quaternion.Euler(0, Random.Range(0, 360), 0);



		Instantiate(critter, spawnPoint, randomRot);

		numberSpawned++;
	}

	protected virtual Vector3 RotateVector(Vector3 vector, float angle) {
		return Quaternion.AngleAxis(angle, Vector3.up) * vector;
	}

}