using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editor
{
	internal class SetSpheres
	{
		[MenuItem("PlayDisplay/Create specials")]
		public static void GenerateSpecials()
		{
			var @object = Selection.activeObject as GameObject;
			if(@object == null)
			{
				Debug.LogWarning("select branch root");
				return;
			}

			var materialGuid = AssetDatabase
				.FindAssets("sphere t:material", new[] { "Assets/_Content/Materials" })
				.FirstOrDefault();
			var material = string.IsNullOrEmpty(materialGuid)
				? null
				: AssetDatabase
					.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(materialGuid));
			if(material == null)
			{
				Debug.LogWarning("sphere material not found");
				return;
			}

			var sourceGuid = AssetDatabase
				.FindAssets("sphere t:gameobject", new[] { "Assets/_Content" })
				.FirstOrDefault();
			var source = string.IsNullOrEmpty(sourceGuid)
				? null
				: AssetDatabase
					.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(sourceGuid));
			if(source == null)
			{
				Debug.LogWarning("sphere object not found");
				return;
			}

			const float SPHERES_MAX_F = 25f;
			var groupIndex = @object.name.Substring(Selection.activeObject.name.Length - 2, 2);

			for(var counter = 0f; counter < SPHERES_MAX_F; counter += 1f)
			{
				var instance = Object.Instantiate(source);
				instance.name = string.Format("sphere_{0:00}", counter);
				instance.transform.SetParent(@object.transform);
				instance.transform.localPosition =
					Mathf.Sin(counter * Mathf.PI / (2f * SPHERES_MAX_F)) *
					new Vector3(Mathf.Cos(2f * counter * Mathf.PI / SPHERES_MAX_F), 1f, Mathf.Sin(2f * counter * Mathf.PI / SPHERES_MAX_F));
				instance.transform.localRotation = Quaternion.Euler(0f, Mathf.Sin(counter * Mathf.PI / (2f * SPHERES_MAX_F)) * Mathf.Rad2Deg, 0f);
				//
				var renderer = instance.GetComponent<MeshRenderer>();
				if(renderer == null)
					continue;
				renderer.sharedMaterial = Object.Instantiate(material);
				renderer.sharedMaterial.name = string.Format("sphere_{0}_material_{1:00}", groupIndex, counter);
				renderer.sharedMaterial.SetFloat("_Index", counter);
				//AssetDatabase.CreateAsset(copy, "Assets/_Content/Sphere/" + copy.name + ".mat");
			}
		}
	}
}
