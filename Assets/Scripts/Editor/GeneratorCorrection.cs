using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editor
{
	public class GeneratorCorrection : AssetPostprocessor
	{
		public void OnPostprocessTexture(Texture2D texture) 
		{
			var importer = assetImporter as TextureImporter;
			var check =
				assetPath.Contains(SystemController.ASSETS_PATH) &&
				assetPath.Contains(SystemController.RESOURCE_NAME) &&
				importer != null;
			if(check)
			{
				importer.textureType = TextureImporterType.Advanced;
				importer.mipmapEnabled = false;
				importer.alphaIsTransparency = true;
				importer.filterMode = FilterMode.Bilinear;
				importer.isReadable = false;
				importer.wrapMode = TextureWrapMode.Clamp;
			}
		}
	}
}