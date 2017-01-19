using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Vuforia;

#if UNITY_EDITOR

namespace Assets.Scripts
{
	public partial class SystemController
	{
		private const int MAX_SOURCES = 2;
		private const float FPALNE_DISTANCE = 10f;
		private const float SOURCE_DISTANCE = 10f;

		private struct CameraLocation
		{
			public readonly Vector3 Target;
			public readonly Vector3 Position;
			public readonly Quaternion Rotation;
			public readonly float FarClip;

			public Vector3 Forward { get { return Target - Position; } }

			public CameraLocation(GameObject targetAsRoot)
			{
				var camera = targetAsRoot != null ? targetAsRoot.GetComponentInChildren<Camera>() : null;
				if(camera == null)
					throw new Exception("camera is null");
				Position = camera.transform.position;
				Target = camera.transform.position + Vector3.forward * camera.farClipPlane;
				Rotation = camera.transform.rotation;
				FarClip = camera.farClipPlane;
			}
		}

		public const string ASSETS_PATH = "Assets/_Content/";
		public const string RESOURCE_NAME = "particle";
		public const string SHADER_NAME = "Particles/Additive";
		public const string ASSET_TEXTURE = ASSETS_PATH + RESOURCE_NAME + ".png";
		public const string ASSET_METERIAL = ASSETS_PATH + RESOURCE_NAME + ".mat";

		private static string _materialUsed;

		private string GetMaterialGuid()
		{
			if(string.IsNullOrEmpty(_materialUsed))
			{
				var guids = AssetDatabase.FindAssets(RESOURCE_NAME + " t:material", new[] { ASSETS_PATH.TrimEnd('/') });
				_materialUsed = guids
					.FirstOrDefault(guid =>
					{
						var asset = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
						return
							asset != null &&
							string.Equals(asset.name, RESOURCE_NAME, StringComparison.InvariantCultureIgnoreCase) &&
							asset.shader.name == SHADER_NAME;
					});
				if(_materialUsed == null)
				{
					var shader = Shader.Find(SHADER_NAME);
					var material = new Material(shader);
					var textureGuid = AssetDatabase.FindAssets(RESOURCE_NAME + " t:texture2D").FirstOrDefault();
					if(string.IsNullOrEmpty(textureGuid))
					{
						var texture = new Texture2D(3, 3, TextureFormat.RGBA32, false);
						texture.SetPixel(1, 1, Color.white);
						var buffer = texture.EncodeToPNG();
						using(var stream = File.Create(Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - 6), ASSET_TEXTURE), buffer.Length))
							stream.Write(buffer, 0, buffer.Length);
						AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.DontDownloadFromCacheServer);
						textureGuid = AssetDatabase.AssetPathToGUID(ASSET_TEXTURE);
						DestroyImmediate(texture);
					}
					material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(textureGuid));
					AssetDatabase.CreateAsset(material, ASSET_METERIAL);
					_materialUsed = AssetDatabase.AssetPathToGUID(ASSET_METERIAL);
				}
			}
			return _materialUsed;
		}

		// ReSharper disable once UnusedMember.Local
		private void Reset()
		{
			var root = gameObject;
#pragma warning disable 0429
			// ReSharper disable once UnreachableCode
			const int SOURCES_COUNT = MAX_SOURCES > 1 ? MAX_SOURCES : 2;
#pragma warning restore 0429
			var cameraLocation = new CameraLocation(FindObjectOfType<VuforiaBehaviour>().gameObject);
			root.transform.rotation = cameraLocation.Rotation;
			var relativeSourcesCenter = cameraLocation.Forward.normalized * (FPALNE_DISTANCE < cameraLocation.FarClip ? FPALNE_DISTANCE : cameraLocation.FarClip);
			for(var counter = 0; counter < SOURCES_COUNT; counter++)
			{
				var guids = AssetDatabase.FindAssets("ImageTarget", new[] { "Assets/Vuforia" });
				var assetGuid = guids.FirstOrDefault(guid =>
				{
					var temp = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
					return temp != null && temp.GetComponent<ImageTargetBehaviour>() != null;
				});
				if(string.IsNullOrEmpty(assetGuid))
					throw new Exception("guid not found");
				var targetInstance = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(assetGuid)));
				if(targetInstance == null)
					throw new Exception("instance creation fail");
				targetInstance.name = "iTarget_" + counter.ToString("00");
				targetInstance.transform.SetParent(root.transform);
				targetInstance.transform.localPosition =
					relativeSourcesCenter +
					Quaternion.AngleAxis(360f * counter / SOURCES_COUNT, Vector3.forward) * Vector3.up * SOURCE_DISTANCE;
				targetInstance.transform.localRotation = Quaternion.identity;
				targetInstance.transform.localScale = Vector3.one;

				var source = new GameObject("source_" + counter.ToString("00"), typeof(SourceController));
				source.transform.SetParent(targetInstance.transform);
				source.transform.localPosition = Vector3.zero;
				source.transform.localRotation = Quaternion.identity;
				source.transform.localScale = Vector3.one;
				var pRenderer = source.GetComponent<ParticleRenderer>();
				pRenderer.material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(GetMaterialGuid()));

				var marker = new GameObject(MARKER_NAME + "_" + counter.ToString("00"));
				marker.transform.SetParent(targetInstance.transform);
				marker.transform.localPosition = Vector3.zero;
				marker.transform.localRotation = Quaternion.identity;
				marker.transform.localScale = Vector3.one;
				ComponentUtility.CopyComponent(targetInstance.GetComponent<MeshFilter>());
				ComponentUtility.PasteComponentAsNew(marker);
				ComponentUtility.CopyComponent(targetInstance.GetComponent<MeshRenderer>());
				ComponentUtility.PasteComponentAsNew(marker);
				var shader = Shader.Find("PlayDisplay/Marker");
				var mRenderer = marker.GetComponent<MeshRenderer>();
				mRenderer.material.shader = shader;
				var query = typeof(Color)
					.GetProperties(BindingFlags.Public | BindingFlags.Static)
					.Where(info => info.PropertyType == typeof(Color))
					.ToArray();
				mRenderer.material.color = query.Any() ? (Color)query[UnityEngine.Random.Range(0, query.Length - 1)].GetValue(null, null) : Color.magenta;
			}
		}
	}
}

#endif
