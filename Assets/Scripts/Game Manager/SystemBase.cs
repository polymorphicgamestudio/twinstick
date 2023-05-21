using ShepProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShepProject
{

	public abstract class SystemBase : MonoBehaviour
	{
		private ShepGM inst;
		protected ShepGM Inst => inst;

		public virtual void Initialize(ShepGM inst)
		{
			this.inst = inst;
		}

	}


}