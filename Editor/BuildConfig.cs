using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildConfig", menuName = "Build/Config")]
public class BuildConfig : ScriptableObject
{
    public string buildName;

    // 新しく追加するフィールド
    public string productName; // Product Name 設定
    public string bundleIdentifier; // Bundle Identifier 設定

    public BuildTarget buildTarget;
    public string[] scriptingDefineSymbols; // スクリプト定義シンボル
    public ScriptingImplementation scriptingBackend; // スクリプティングバックエンド

    // LogTypeごとのStackTraceLogType設定
    public StackTraceLogType logStackTraceLogType = StackTraceLogType.ScriptOnly;
    public StackTraceLogType errorStackTraceLogType = StackTraceLogType.Full;
    public StackTraceLogType exceptionStackTraceLogType = StackTraceLogType.Full;
    public StackTraceLogType warningStackTraceLogType = StackTraceLogType.ScriptOnly;

    public SceneAsset[] sceneAssets; // シーンを設定するためのSceneAsset

    // シーンのパスを取得して、ビルド時に利用するメソッド
    public string[] GetScenePaths()
    {
        var scenePaths = new string[sceneAssets.Length];
        for (var i = 0; i < sceneAssets.Length; i++)
        {
            scenePaths[i] = AssetDatabase.GetAssetPath(sceneAssets[i]);
        }

        return scenePaths;
    }
}