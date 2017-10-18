using Core.XmlDatabase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Core.Managers
{
    public class ResourceManager : MonoBehaviour
    {
        public static string AssetBundlePath { get { return Application.streamingAssetsPath + "/AssetBundles"; } }
        public static string AssetInfoXMLPath { get { return Application.streamingAssetsPath + "/XML"; } }

        public bool ForceAssetBundlesInEditor = false;
        public bool debug = false;

        private Dictionary<ID, AssetBundle> loadedBundles = new Dictionary<ID, AssetBundle>();

        private IEnumerator LoadBundlesCo(Action onComplete)
        {
            AssetBundleInfo[] bundlesInfos = Database.Instance.GetEntries<AssetBundleInfo>();

            foreach (AssetBundleInfo info in bundlesInfos)
            {
                string url = "file://" + AssetBundlePath + "/" + info.Name;

                if (debug)
                    Debug.Log("Loading bundle: " + url);

                WWW www = new WWW(url);
                yield return www;

                loadedBundles.Add(info.DatabaseID, www.assetBundle);
            }

            onComplete?.Invoke();
        }

        public T LoadAsset<T>(AssetInfo info) where T : UnityEngine.Object
        {
            if (!Application.isEditor || ForceAssetBundlesInEditor)
            {
                if (info.AssetBundleInfoRef != null)
                {
                    AssetBundle bundle;
                    if (!loadedBundles.TryGetValue(info.AssetBundleInfoRef.Entry.DatabaseID, out bundle))
                        throw new Exception("Assert bundle not found: " + info.AssetBundleInfoRef.Entry.Name);

                    AssetBundleRequest request = bundle.LoadAssetAsync(info.Path, typeof(T));

                    while (!request.isDone) { }

                    if (request.asset is T)
                        return request.asset as T;
                    else
                        throw new Exception("Asset Request type mismatch");
                }
                else
                    throw new Exception("Invalid Asset Bundle Info");
            }
            else
            {
#if UNITY_EDITOR
                return EditorLoadAsset<T>(info);
#else
            throw new Exception("Trying to use editor load outside of the editor");
#endif

            }
        }

        public void LoadAssetAsync<T>(AssetInfo info, Action<T> onComplete) where T : UnityEngine.Object
        {
            if (!Application.isEditor || ForceAssetBundlesInEditor)
            {
                StartCoroutine(LoadAssetAsyncCo<T>(info, onComplete));
            }
            else
            {
#if UNITY_EDITOR
                T asset = EditorLoadAsset<T>(info);
                onComplete?.Invoke(asset);
#else
            throw new Exception("Trying to use editor load outside of the editor");
#endif
            }
        }

        public void LoadBundlesAsync(Action onComplete)
        {
            StartCoroutine(LoadBundlesCo(onComplete));
        }

        private IEnumerator LoadAssetAsyncCo<T>(AssetInfo info, Action<T> onComplete) where T : UnityEngine.Object
        {
            if (info.AssetBundleInfoRef != null)
            {
                AssetBundle bundle;
                if (!loadedBundles.TryGetValue(info.AssetBundleInfoRef.Entry.DatabaseID, out bundle))
                    throw new Exception("Asset bundle not found: " + info.AssetBundleInfoRef.Entry.Name);

                AssetBundleRequest request = bundle.LoadAssetAsync(info.Path, typeof(T));
                yield return request;

                if (request.asset == null)
                    throw new Exception("Asset missing from bundle. Did you forget to rebuild?");

                if (request.asset is T)
                    onComplete?.Invoke(request.asset as T);
                else
                    throw new Exception("Asset type mismatch");
            }
            else
                throw new Exception("Invalid Asset Bundle Info");
        }

#if UNITY_EDITOR
        private T EditorLoadAsset<T>(AssetInfo info) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(info.Path);
        }
#endif

#if UNITY_EDITOR
        private static void ValidateAssetBundleAssetsInternal()
        {
            AssetInfo[] assetInfos = Database.Instance.GetEntries<AssetInfo>();
            foreach (AssetInfo assetInfo in assetInfos)
            {
                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetInfo.Path);

                if (!asset)
                    Debug.LogWarning("Invalid Asset: " + assetInfo.Path + " in AssetInfo: " + assetInfo.DatabaseID.ToString());
            }

            Resources.UnloadUnusedAssets();
        }

        [MenuItem("Assets/Validate AssetBundle Assets")]
        private static void ValidateAssetBundleAssets()
        {
            Debug.Log("Validating assets...");

            Database.Instance.Clear();
            Database.Instance.ReadFiles(AssetInfoXMLPath);

            ValidateAssetBundleAssetsInternal();

            Database.Instance.Clear();

            Debug.Log("Finished validating assets.");
        }

        [MenuItem("Assets/Clear AssetBundles")]
        private static void ClearAssetBundles()
        {
            if (Directory.Exists(AssetBundlePath))
            {
                string[] existingAssetBundlePaths = Directory.GetFiles(AssetBundlePath);
                foreach (string bundlePath in existingAssetBundlePaths)
                {
                    Debug.Log("Deleting bundle: " + bundlePath);
                    File.Delete(bundlePath);
                }
            }
            else
                Directory.CreateDirectory(AssetBundlePath);

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Assets/Build AssetBundles")]
        private static void BuildAssetBundles()
        {
            ClearAssetBundles();

            Database.Instance.ReadFiles(AssetInfoXMLPath);

            ValidateAssetBundleAssetsInternal();

            AssetInfo[] assetInfos = Database.Instance.GetEntries<AssetInfo>();
            AssetBundleInfo[] assetBundleInfos = Database.Instance.GetEntries<AssetBundleInfo>();

            Dictionary<ID, List<UnityEngine.Object>> bundleObjects = new Dictionary<ID, List<UnityEngine.Object>>();
            Dictionary<ID, List<string>> bundleNames = new Dictionary<ID, List<string>>();

            foreach (AssetInfo info in assetInfos)
            {
                List<UnityEngine.Object> objectList;
                if (!bundleObjects.TryGetValue(info.AssetBundleInfoRef.Entry.DatabaseID, out objectList))
                {
                    objectList = new List<UnityEngine.Object>();
                    bundleObjects.Add(info.AssetBundleInfoRef.Entry.DatabaseID, objectList);
                }

                List<string> nameList;
                if (!bundleNames.TryGetValue(info.AssetBundleInfoRef.Entry.DatabaseID, out nameList))
                {
                    nameList = new List<string>();
                    bundleNames.Add(info.AssetBundleInfoRef.Entry.DatabaseID, nameList);
                }

                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(info.Path);
                if (asset == null)
                    throw new Exception("Invalid asset: " + info.Path);

                objectList.Add(asset);
                nameList.Add(info.Path);
            }

            foreach (AssetBundleInfo info in assetBundleInfos)
            {
                List<UnityEngine.Object> objectList;
                if (!bundleObjects.TryGetValue(info.DatabaseID, out objectList))
                {
                    Debug.LogWarning("No objects for bundle: " + info.Name);
                    continue;
                }

                List<string> nameList;
                if (!bundleNames.TryGetValue(info.DatabaseID, out nameList))
                    throw new Exception("No names generated for objects");

                string path = AssetBundlePath;// + ".unity3d";
                BuildAssetBundleOptions options = (BuildAssetBundleOptions.DeterministicAssetBundle);
                BuildTarget target = BuildTarget.StandaloneWindows64;

                AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
                buildMap[0].assetBundleName = info.Name;

                buildMap[0].assetNames = nameList.ToArray();

                if (!BuildPipeline.BuildAssetBundles(path, buildMap, options, target))
                    throw new Exception("Unable to build asset bundle: " + info.Name);

                AssetDatabase.SaveAssets();

                Debug.Log("Build bundle: " + info.Name);
            }
        }
#endif
    }
}