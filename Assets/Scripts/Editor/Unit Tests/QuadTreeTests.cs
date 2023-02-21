using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShepProject {


	public class QuadTreeTests {

		[Test]
		public void TestSort() {

			short count = 100;

			QuadTree tree = new QuadTree(count * 5, 2);




			for (short i = count; i > -count; i--) {
				Vector3 temp = new Vector3(i, 0, i);

				tree.AddPosition(temp);

			}

			//positions added now, so test if it updates correctly
			tree.Update();

			


	
			tree.Dispose();


	}


	}

}
