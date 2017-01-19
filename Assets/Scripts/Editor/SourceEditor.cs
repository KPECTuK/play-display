using UnityEditor;

namespace Assets.Scripts.Editor
{
	[CustomEditor(typeof(SourceController))]
	public class SourceEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var controller = target as SourceController;
			if(controller == null)
				return;

			controller._color = EditorGUILayout.ColorField(controller._color);
		}
	}
}
