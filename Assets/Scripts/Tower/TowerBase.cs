using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBase : MonoBehaviour {

	Transform rotBoneHoz;
	Transform rotBoneVert;
	Animator animator;

    void Start() {
		rotBoneHoz = GetComponent<TowerRotationReference>().RotationBoneHoz;
		rotBoneVert = GetComponent<TowerRotationReference>().RotationBoneVert;
		animator = GetComponent<Animator>();
		animator.SetTrigger("Wake");
	}


}
