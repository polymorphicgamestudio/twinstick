using System.Collections;
using UnityEngine;

public class RandomizeIdleAnim : MonoBehaviour {

    Animator animController;

    void Start() {
        animController = GetComponent<Animator>();
		InvokeRepeating("RandomizeIdle", 2f, 2f);
    }

	void RandomizeIdle() {
		animController.SetFloat("RandomIdle", Random.value);
	}

    //IEnumerator RandimizeIdle() {
    //  yield return new WaitForSeconds(Random.Range(2f,4f));
    //  animController.SetFloat("RandomIdle", Random.value);
    //  StartCoroutine(RandimizeIdle());
    //}
}