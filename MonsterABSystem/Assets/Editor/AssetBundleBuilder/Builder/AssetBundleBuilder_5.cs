using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LitJson;
using Monster.BaseSystem.ResourceManager;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Assets.Editor.AssetBundleBuilder
{
    class AssetBundleBuilder_5
    {
        public void Excute(BuilderConfig config)
        {
            var rootList = LoadRootAssets(config.LoadPath);
            var deptList = LoadDependenceAssets(rootList);

            //Group
            var nodeList = Group(rootList, deptList);

            //Convert
            var bundleBuildList = ConvertToBundleBulidList(nodeList);

            //WriteInfo
            ExplortAssetBundleConfig(nodeList, config.ExportPath);

            //Build 
            DoBuild(config.ExportPath, bundleBuildList.ToArray(), config.options, config.target);
        }

        public void Debug(BuilderConfig config)
        {
            var rootList = LoadRootAssets(config.LoadPath);
            var deptList = LoadDependenceAssets(rootList);

            //Group
            var nodeList = Group(rootList, deptList);

            //Convert
            var bundleBuildList = ConvertToBundleBulidList(nodeList);

            //WriteInfo
            ExplortAssetBundleConfig(nodeList, config.ExportPath);

            EditorUtil.ClearLog();

            foreach (var bundleBuild in bundleBuildList)
            {
                StringBuilder sb= new StringBuilder();
                sb.AppendLine(bundleBuild.assetBundleName);
                foreach (var path in bundleBuild.assetNames)
                {
                    sb.AppendLine(path);
                }
                UnityEngine.Debug.Log(sb.ToString());
            }
        }

        private List<string> LoadRootAssets(string path)
        {
            var list = new List<string>();
            Stack<DirectoryInfo> floderStack = new Stack<DirectoryInfo>();
            floderStack.Push(new DirectoryInfo(Utility.GetFullPath(path)));
            while (floderStack.Count > 0)
            {
                var floder = floderStack.Pop();
                foreach (var directoryInfo in floder.GetDirectories())
                {
                    floderStack.Push(directoryInfo);
                }

                foreach (var fileInfo in floder.GetFiles())
                {
                    if (Utility.IsLoadingAsset(fileInfo.Extension))
                    {
                        list.Add(Utility.GetRelativeAssetsPath(fileInfo.FullName));
                    }
                }
            }
            return list;
        }

        private List<string> LoadDependenceAssets(List<string> rootList)
        {
            var rootPathSet = new HashSet<string>();
            var listPathSet = new HashSet<string>();

            foreach (var path in rootList)
            {
                rootPathSet.Add(path);
            }

            var rootObjectList = new List<Object>();

            foreach (var path in rootList)
            {
                rootObjectList.Add(AssetDatabase.LoadAssetAtPath(path, typeof (Object)));
            }
            foreach (var depObject in EditorUtility.CollectDependencies(rootObjectList.ToArray()))
            {
                var depPath = AssetDatabase.GetAssetPath(depObject);
                if (!listPathSet.Contains(depPath) && !rootPathSet.Contains(depPath))
                {
                    if (Utility.IsLoadingAsset(Utility.GetExtension(depPath)))
                        listPathSet.Add(depPath);
                }
            }
            return listPathSet.ToList();
        }

        private List<AssetRefrenceNode> Group(List<string> rootList, List<string> depList)
        {
            //准备
            var allPathSet = new HashSet<string>();
            foreach (var path in rootList)
            {
                allPathSet.Add(path);
            }
            foreach (var path in depList)
            {
                allPathSet.Add(path);
            }

            //构造依赖关系图
            var map = new Dictionary<string, AssetRefrenceNode>();
            foreach (var path in allPathSet)
            {
                map[path] = new AssetRefrenceNode() {AssetPath = path};
            }
            foreach (var pair in map)
            {
                var path = pair.Key;
                var curNode = pair.Value;
                var nodeObj = AssetDatabase.LoadAssetAtPath(path, typeof (Object));
                var depObjs = EditorUtility.CollectDependencies(new Object[] {nodeObj});

                foreach (var depObj in depObjs)
                {
                    var depPath = AssetDatabase.GetAssetPath(depObj);
                    if (map.ContainsKey(depPath) && path != depPath)
                    {
                        curNode.depence.Add(depPath);
                        map[depPath].depenceOnMe.Add(path);
                    }
                }
            }

            //合并依赖 清除同层之间的依赖 把同层之间被依赖的结点下移 两层结构 a->b,a->c,b->c 下移c 为:a->b->c 三层结构
            /*
             *          a                      a
             *         /  \                   /
             *        b -> c       ==>       b
             *                              /
             *                             c
             */
            var rootQueue = new Queue<string>();
            List<string> toRemove = new List<string>();
            HashSet<string> depSet = new HashSet<string>();
            foreach (var path in rootList)
            {
                rootQueue.Enqueue(path);
            }
            while (rootQueue.Count > 0)
            {
                depSet.Clear();
                toRemove.Clear();
                var path = rootQueue.Dequeue();
                var node = map[path];
                foreach (var depPath in node.depence)
                {
                    depSet.Add(depPath);
                }
                foreach (var depPath in node.depence)
                {
                    var depNode = map[depPath];
                    foreach (var onMepath in depNode.depenceOnMe)
                    {
                        if (depSet.Contains(onMepath))
                            toRemove.Add(depPath);
                    }
                }
                foreach (var depPath in toRemove)
                {
                    node.depence.Remove(depPath);
                    var depNode = map[depPath];
                    depNode.depenceOnMe.Remove(path);
                }
                foreach (var depPath in node.depence)
                {
                    if (!rootQueue.Contains(depPath))
                        rootQueue.Enqueue(depPath);
                }
            }

            //打组 向上合并, a->b->c
            /*                                          
             *          a        e                  (a,b) (e,f ) --> d          
             *           \      /                   / |  \        _-^   
             *  root:     b    f       ==>         c  h   L_____-`        ==>     group:   (a,b,c,h) -> (d) <- (e,f)
             *          / | \ /                          
             *         c  h  d                   
             */
            HashSet<string> hasSearchSet = new HashSet<string>();
            var list = new List<AssetRefrenceNode>();
            foreach (var path in rootList)
            {
                rootQueue.Enqueue(path);
            }
            while (rootQueue.Count > 0)
            {
                var nodePath = rootQueue.Dequeue();
                var node = map[nodePath];
                hasSearchSet.Add(nodePath);
                var depQueue = new Queue<string>(node.depence);
                while (depQueue.Count > 0)
                {
                    var depNodePath = depQueue.Dequeue();
                    var depNode = map[depNodePath];
                    if (depNode.depenceOnMe.Count == 1)
                    {
                        node.depence.Remove(depNodePath);
                        map.Remove(depNodePath);
                        node.incluedDepReference.Add(depNodePath);
                        foreach (var dep2Path in depNode.depence)
                        {
                            node.depence.Add(dep2Path);
                            depQueue.Enqueue(dep2Path);
                            var dep2Node = map[dep2Path];
                            dep2Node.depenceOnMe.Remove(depNodePath);
                            dep2Node.depenceOnMe.Add(nodePath);
                        }
                    }
                    else if (depNode.depenceOnMe.Count > 1)
                    {
                        if (!rootQueue.Contains(depNodePath) && !hasSearchSet.Contains(depNodePath))
                            rootQueue.Enqueue(depNodePath);
                    }
                }
                list.Add(node);
            }

            return list;
        }

        private AssetBundleBuild[] ConvertToBundleBulidList(List<AssetRefrenceNode> nodeList)
        {
            var list = new List<AssetBundleBuild>();
            foreach (var node in nodeList)
            {
                var path = node.AssetPath;
                var name = Utility.GetBundleNames(path);
                List<string> assetNames = node.incluedDepReference;
                assetNames.Add(path);
                list.Add(new AssetBundleBuild() { assetNames = assetNames.ToArray(), assetBundleName = name });
            }
            return list.ToArray();
        }

        private void DoBuild(string ouputPath, AssetBundleBuild[] builds, BuildAssetBundleOptions assetBundleOptions, BuildTarget targetPlatform)
        {
            if (!Directory.Exists(ouputPath))
            {
                Directory.CreateDirectory(ouputPath);
            }
            //Directory.CreateDirectory(ouputPath);

            BuildPipeline.BuildAssetBundles(ouputPath, builds, assetBundleOptions, targetPlatform);
        }

        private void ExplortAssetBundleConfig(List<AssetRefrenceNode> list, string exportPath)
        {
            Dictionary<string, AssetBundleInfoNode> map = new Dictionary<string, AssetBundleInfoNode>();
            foreach (var assetRefrenceNode in list)
            {
                var bundleNode = new AssetBundleInfoNode();
                var name = Utility.GetFileNameWithOutExtension(assetRefrenceNode.AssetPath);
                bundleNode.assetName = name;
                foreach (var depPath in assetRefrenceNode.depence)
                {
                    var depName = Utility.GetFileNameWithOutExtension(depPath);
                    bundleNode.depenceList.Add(depName);
                }
                map.Add(name, bundleNode);
            }

            var json = JsonMapper.ToJson(map);
            var configPath = exportPath + "/" + Monster.BaseSystem.ResourceManager.BundleConfig.CONFIG_FILE_NAME;
            if (File.Exists(configPath))
            {
                File.Delete(configPath);
            }
            using (var writer = File.CreateText(configPath))
            {
                writer.Write(json);
            }
        }
    }
}
