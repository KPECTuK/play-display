using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editor
{
	internal class SetSpheres
	{
		[MenuItem("Custom/Create spheres")]
		public static void SetSeparateMaterials()
		{
			var materialGuid = AssetDatabase.FindAssets("sphere_material_source t:material", new[] { "Assets/_Content" }).FirstOrDefault();
			if(string.IsNullOrEmpty(materialGuid))
				return;
			var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(materialGuid));
			if(material == null)
				return;

			const float SPHERES_MAX_F = 25f;

			var groupIndex = Selection.activeObject.name.Substring(Selection.activeObject.name.Length - 2, 2);
			for(var counter = 0f; counter < SPHERES_MAX_F; counter += 1f)
			{
				var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				sphere.name = "sphere_object_" + counter.ToString("00");
				sphere.transform.SetParent(Selection.activeTransform);
				sphere.transform.localScale = Vector3.one * .1f;
				sphere.transform.localPosition =
					Mathf.Sin(counter * Mathf.PI / (2f * SPHERES_MAX_F)) *
					new Vector3(Mathf.Cos(2f * counter * Mathf.PI / SPHERES_MAX_F), 1f, Mathf.Sin(2f * counter * Mathf.PI / SPHERES_MAX_F));
				sphere.transform.localRotation = Quaternion.Euler(0f, Mathf.Sin(counter * Mathf.PI / (2f * SPHERES_MAX_F)) * Mathf.Rad2Deg, 0f);
				var renderer = sphere.GetComponent<MeshRenderer>();
				if(renderer != null)
				{
					var copy = Object.Instantiate(material);
					copy.name = "sphere_group_" + groupIndex + "_material_" + counter.ToString("00");
					AssetDatabase.CreateAsset(copy, "Assets/_Content/Sphere/" + copy.name + ".mat");
					renderer.material = copy;
					copy.SetFloat("_id", counter);
				}
			}
		}
	}
}
