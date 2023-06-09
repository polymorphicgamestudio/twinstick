using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEMPRandomStartMaterial : MonoBehaviour {

	Material mat;

    void Start() {
		mat = GetComponent<MeshRenderer>().material;
		mat.SetFloat("_Length", Random.Range(1.0f, 10f));
		mat.color = Random.ColorHSV();
	}
}