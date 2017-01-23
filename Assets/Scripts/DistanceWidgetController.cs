using UnityEngine.UI;

namespace Assets.Scripts
{
	public class DistanceWidgetController : MonoBehaviourExtended
	{
		private Text _text;

		protected override void Awake()
		{
			base.Awake();

			_text = GetComponentInChildren<Text>(true);
		}

		public void SetDistance(float distance)
		{
			_text.text = "distance: " + distance.ToString("####.00' cm'");
		}
	}
}