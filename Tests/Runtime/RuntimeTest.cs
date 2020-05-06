using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace Undefined.DeJochenhecklNavmeshecs.Tests 
{
	
	class RuntimeTest
	{
		[Test]
		public void PlayModeSampleTestSimplePasses() 
		{
            Assert.Pass();
		}

		[UnityTest]
		public IEnumerator PlayModeSampleTestWithEnumeratorPasses() 
		{
			yield return null;
		}
	}
}