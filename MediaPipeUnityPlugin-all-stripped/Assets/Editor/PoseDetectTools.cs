using System.Collections;
using System.Collections.Generic;
using Mediapipe;
using Mediapipe.Tasks.Vision.Core;
using Mediapipe.Unity.Sample.PoseTracking;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class PoseDetectTools : OdinEditorWindow
{
  private static PoseDetectTools _window;
  [MenuItem("Tools/图片姿态输出工具")]
  public static void Popup()
  {
    _window = GetWindow<PoseDetectTools>("图片姿态输出工具");
    _window.maxSize = new Vector2(600, 800);
    _window.minSize = new Vector2(600, 400);
  }
  private string baseFolder = "Assets/Data/CorrectMoves";//图片输出的根目录
  public string poseName;
  private CorrectMoveSO correctMove;
  private IReadOnlyList<NormalizedLandmark> _currentPoseLandmarkList;
  private Vector3[] _currentPostWorldPosArray = new Vector3[33];//33个特征点检测
  private float gizmoDepth = 2;
  private Camera cameraMain;
  
  [Button("开始检测", ButtonSizes.Medium)]
  private void OnToolbarGUI()
  {
      if (!Application.isPlaying)
      {
          Debug.Log("请在运行模式使用工具");
          return;
      }
      if (string.IsNullOrEmpty(poseName))
      {
          Debug.LogWarning("请输入图片的名称");
          return;
      }
      if (correctMove != null && correctMove.poseName != poseName) correctMove = null;

      // 1. 获取当前 Mediapipe 姿态
      var poseLandmarkList = PoseTrackingSolution.CurrentPoseLandmarks;
      _currentPoseLandmarkList = poseLandmarkList == null ? null : poseLandmarkList.Landmark;
      if (_currentPoseLandmarkList == null) return;

      cameraMain = PoseTrackingSolution.cameraMain;
      if (cameraMain == null) return;

      // 转换 33 个关键点
      for (var i = 0; i < _currentPoseLandmarkList.Count; i++)
      {
          var worldPos = GetWorldPositionFromLandmark(_currentPoseLandmarkList[i]);
          worldPos = new Vector3(-worldPos.x, worldPos.y, worldPos.z); // 左右镜像
          _currentPostWorldPosArray[i] = worldPos;
      }

      // 2. 查找是否已经存在同名 SO
      if (!AssetDatabase.IsValidFolder(baseFolder))
      {
          Debug.LogError("根目录不存在：" + baseFolder);
          return;
      }

      string foundPath = null;
      string[] guids = AssetDatabase.FindAssets("t:CorrectMoveSO", new[] { baseFolder });

      foreach (string guid in guids)
      {
          string path = AssetDatabase.GUIDToAssetPath(guid);
          var move = AssetDatabase.LoadAssetAtPath<CorrectMoveSO>(path);

          if (move != null && move.poseName == poseName)
          {
              correctMove = move;   // 已经存在
              foundPath = path;
              break;
          }
      }

      // 3. 如果没找到，就新建一个
      if (correctMove == null)
      {
          correctMove = ScriptableObject.CreateInstance<CorrectMoveSO>();
          correctMove.poseName = poseName;
          correctMove.poseLandmarks = _currentPostWorldPosArray;

          // 保存路径
          string assetPath = $"{baseFolder}/{poseName}.asset";

          AssetDatabase.CreateAsset(correctMove, assetPath);
          AssetDatabase.SaveAssets();
          AssetDatabase.Refresh();

          Debug.Log($"新建 CorrectMoveSO：{assetPath}");
      }
      else
      {
          // 更新已有文件的数据
          correctMove.poseLandmarks = _currentPostWorldPosArray;
          EditorUtility.SetDirty(correctMove);
          AssetDatabase.SaveAssets();

          Debug.Log($"更新已有 CorrectMoveSO：{foundPath}");
      }
  }

  /// <summary>
  /// 将 0~1 归一化坐标转换为世界坐标（以主相机为参考）
  /// </summary>
  private Vector3 GetWorldPositionFromLandmark(NormalizedLandmark landmark)
  {
    // MediaPipe 的 y 轴是向下的，这里做一次翻转到 Unity 的视口坐标
    var viewport = new Vector3(landmark.X, 1f - landmark.Y, gizmoDepth);
    return cameraMain != null ? cameraMain.ViewportToWorldPoint(viewport) : Vector3.zero;
  }

}
