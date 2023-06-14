using UnityEngine;
public class CreateMeshBounds : MonoBehaviour {
	void Start() {
		GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.up, Vector3.one);
	}
}