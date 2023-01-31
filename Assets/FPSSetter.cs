using UnityEngine;

namespace TeamLab.Fireflies
{
	public class FPSSetter : MonoBehaviour
	{
		const int TargetFPS = 30;

		[RuntimeInitializeOnLoadMethod]
		static void SetFPS()
		{
			QualitySettings.vSyncCount  = 0;
			Application.targetFrameRate = TargetFPS;
		}
	}
}