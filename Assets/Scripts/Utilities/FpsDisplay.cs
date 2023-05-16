using UnityEngine;
using System.Collections;
using TMPro;

namespace ShepProject
{

	public class FpsDisplay : MonoBehaviour
	{
		TMP_Text fpss;
		float deltaTime = 0.0f;
		public int counter = 0;
		public int current;
		
		public static int agentCounter;

		private string lastValues;
		private float maxMs;
		private float currentTimer;
		public float stringAddTimer;

		public float timer;

		private void Awake()
		{
			lastValues = "";
			fpss = GetComponent<TMP_Text>();
			//transform.position = new Vector3(Screen.safeArea.width / 2f, Screen.safeArea.height / 2f, 0);
		}

		void Update()
		{

			if (current > counter)
			{
				
				current = 0;
				currentTimer -= Time.deltaTime;
				deltaTime = (Time.unscaledDeltaTime) ;

			}

			if (maxMs < (Time.unscaledDeltaTime) * 100)
			{
				maxMs = (Time.unscaledDeltaTime) * 100;
			}
			if (timer > 2)
			{
				maxMs = 0;
				timer = 0;
				maxMs = 0;
			}


			fpss.text = string.Format("{0:0.0} ms\n({1:0.} fps)\n{2:000.0}" + "ms", deltaTime * 100.0f, 1.0f / deltaTime, maxMs);

			current++;
			timer += Time.deltaTime;
		}
	}


}