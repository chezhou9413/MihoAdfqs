using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.Rendering;

//用与游戏匹配的Unity编译战斗Shader并导出Windows资源包。
public static class BuildCombatShaders
{
    //编译前导入资源，构建中出现引擎错误时停止发布。
    public static void Build()
    {
        string output = Environment.GetEnvironmentVariable("MIHO_COMBAT_BUNDLE_OUTPUT");
        Directory.CreateDirectory(output);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        string[] shaders = { "Assets/Shader/CombatBeam.shader", "Assets/Shader/CombatBurst.shader",
            "Assets/Shader/CombatSmoke.shader", "Assets/Shader/ImperialShield.shader" };
        foreach (string path in shaders)
        {
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            if (shader == null) throw new InvalidOperationException("无法导入Shader：" + path);
            foreach (ShaderMessage message in ShaderUtil.GetShaderMessages(shader))
                if (message.severity == ShaderCompilerMessageSeverity.Error)
                    throw new InvalidOperationException(path + ": " + message.message);
        }
        string buildError = null;
        Application.LogCallback captureError = (message, stack, type) =>
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                buildError = message;
        };
        AssetBundleManifest manifest;
        Application.logMessageReceived += captureError;
        try
        {
            manifest = BuildPipeline.BuildAssetBundles(output,
                new[] { new AssetBundleBuild { assetBundleName = "miho_combat.ab", assetNames = shaders } },
                BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.StrictMode
                    | BuildAssetBundleOptions.ForceRebuildAssetBundle,
                BuildTarget.StandaloneWindows64);
        }
        finally
        {
            Application.logMessageReceived -= captureError;
        }
        if (buildError != null) throw new InvalidOperationException("战斗Shader资源包构建错误：" + buildError);
        if (manifest == null) throw new InvalidOperationException("战斗Shader资源包构建失败。");
        foreach (string path in shaders)
            foreach (ShaderMessage message in ShaderUtil.GetShaderMessages(AssetDatabase.LoadAssetAtPath<Shader>(path)))
                if (message.severity == ShaderCompilerMessageSeverity.Error)
                    throw new InvalidOperationException(path + ": " + message.message);
        Debug.Log("MIHO_COMBAT_BUILD_OK：" + shaders.Length + "个Shader，Windows x64，Unity " + Application.unityVersion);
    }
}
