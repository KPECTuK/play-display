using UnityEditor;

namespace Assets.Scripts.Editor
{
	[CustomEditor(typeof(SystemController))]
	public class SystemEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var controller = target as SystemController;
			if(controller == null)
				return;
		}
	}
}
