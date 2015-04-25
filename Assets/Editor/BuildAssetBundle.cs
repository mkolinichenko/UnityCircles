using UnityEngine;
using UnityEditor;

public class BuildAssetBundle {
	[MenuItem("Assets/Build AssetBundle")]
	static void ExportResource () {
		string path = "Assets/Bundle.unity3d";
		Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
		BuildPipeline.BuildAssetBundle(Selection.activeObject, selection, path, 
		                               BuildAssetBundleOptions.CollectDependencies 
		                               | BuildAssetBundleOptions.CompleteAssets);
	}
}

