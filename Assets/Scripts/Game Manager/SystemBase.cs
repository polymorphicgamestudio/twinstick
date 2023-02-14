using ShepProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SystemBase : MonoBehaviour
{
	private ShepGM manager;
	protected ShepGM Manager => manager;

	public void Initialize(ShepGM inst) {
		this.manager = inst;
	}

}
