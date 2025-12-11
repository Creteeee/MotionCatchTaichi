using System;
using System.Collections.Generic;
using Mediapipe;
using Mediapipe.Unity.Sample;
using UnityEngine;

namespace Mediapipe.Unity.Sample.Holistic
{
  // 从 HolisticTrackingSolution 中拿到当前帧的姿态特征点，避免直接操作 GraphRunner 的内部流
  public class AvatarBonePositionTest : MonoBehaviour
  {
    public List<Transform> bones = new List<Transform>();
    public Camera cameraMain;

    // 实时更新的姿态特征点列表（NormalizedLandmark）
    private IReadOnlyList<NormalizedLandmark> _currentPoseLandmarkList;
    public static Vector3[] _currentPostWorldPosArray = new Vector3[33];//33个特征点检测
    public BoneRelocator boneRelocator;

    // 为每个特征点生成/复用的红色小球
    private readonly List<GameObject> _landmarkSpheres = new List<GameObject>();

    // 对应每个小球前面的编号 UI（3D TextMesh）
    private readonly List<TextMesh> _indexLabels = new List<TextMesh>();

    // 直接引用场景中的 HolisticTrackingSolution（Holistic 示例主组件）
    [SerializeField] private HolisticTrackingSolution holisticSolution;

    [Header("Sphere 设置")]
    [SerializeField] private float gizmoDepth = 2.0f;   // 特征点在相机前方的深度
    [SerializeField] private float gizmoRadius = 0.02f; // 红色球半径

    
    

    private void Start()
    {
      boneRelocator.PoseLandmarkWorldposArray =  _currentPostWorldPosArray;
    }

    private void Update()
    {
      if (cameraMain == null || holisticSolution == null)
      {
        return;
      }

      // 从 HolisticTrackingSolution 里读取当前帧的 pose 特征点
      var poseLandmarkList = holisticSolution.CurrentPoseLandmarks;
      _currentPoseLandmarkList = poseLandmarkList == null ? null : poseLandmarkList.Landmark;

      if (_currentPoseLandmarkList == null)
      {
        HideAllSpheres();
        return;
      }

      EnsureSphereCount(_currentPoseLandmarkList.Count);

      for (var i = 0; i < _currentPoseLandmarkList.Count; i++)
      {
        var landmark = _currentPoseLandmarkList[i];
        var worldPos = GetWorldPositionFromLandmark(landmark);
        var sphere = _landmarkSpheres[i];
        worldPos = new Vector3(-worldPos.x, worldPos.y, worldPos.z); // 镜像
        _currentPostWorldPosArray[i] = worldPos;

        sphere.transform.position = worldPos;
        if (!sphere.activeSelf)
        {
          sphere.SetActive(true);
        }

        // 更新编号 UI 位置与文字
        if (i < _indexLabels.Count && _indexLabels[i] != null)
        {
          var label = _indexLabels[i];
          // 稍微往相机方向偏移一点，避免和球重叠
          var cam = cameraMain;
          var offset = cam != null ? cam.transform.forward * gizmoRadius * 1.5f : Vector3.forward * gizmoRadius * 1.5f;
          label.transform.position = worldPos + offset;
          if (cam != null)
          {
            // 始终朝向相机
            label.transform.rotation = Quaternion.LookRotation(label.transform.position - cam.transform.position);
          }
          label.text = i.ToString();
          if (!label.gameObject.activeSelf)
          {
            label.gameObject.SetActive(true);
          }
        }
      }
      boneRelocator.PoseLandmarkWorldposArray =  _currentPostWorldPosArray;

      // 多余的小球和编号隐藏
      for (var i = _currentPoseLandmarkList.Count; i < _landmarkSpheres.Count; i++)
      {
        if (_landmarkSpheres[i].activeSelf)
        {
          _landmarkSpheres[i].SetActive(false);
        }
        if (i < _indexLabels.Count && _indexLabels[i] != null && _indexLabels[i].gameObject.activeSelf)
        {
          _indexLabels[i].gameObject.SetActive(false);
        }
      }
    }

    private void OnDisable()
    {
      HideAllSpheres();
    }

    private void OnDestroy()
    {
      // 回收创建的所有小球
      foreach (var sphere in _landmarkSpheres)
      {
        if (sphere != null)
        {
          Destroy(sphere);
        }
      }
      _landmarkSpheres.Clear();

      foreach (var label in _indexLabels)
      {
        if (label != null)
        {
          Destroy(label.gameObject);
        }
      }
      _indexLabels.Clear();
    }

    /// <summary>
    /// 确保有足够数量的 sphere 及其编号 UI 供当前帧使用，不够就新建，过多则保留备用（只隐藏）
    /// </summary>
    private void EnsureSphereCount(int count)
    {
      while (_landmarkSpheres.Count < count)
      {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = $"PoseLandmarkSphere_{_landmarkSpheres.Count}";
        sphere.transform.SetParent(transform, false);
        sphere.transform.localScale = Vector3.one * gizmoRadius * 2f;

        var renderer = sphere.GetComponent<Renderer>();
        if (renderer != null)
        {
          renderer.material.color = UnityEngine.Color.red;
        }

        _landmarkSpheres.Add(sphere);

        // 创建编号 UI（TextMesh），作为该索引对应的 label
        var labelObj = new GameObject($"PoseLandmarkLabel_{_indexLabels.Count}");
        labelObj.transform.SetParent(transform, false);
        var textMesh = labelObj.AddComponent<TextMesh>();
        textMesh.text = _indexLabels.Count.ToString();
        textMesh.fontSize = 32;
        textMesh.characterSize = gizmoRadius * 0.8f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = UnityEngine.Color.white;

        _indexLabels.Add(textMesh);
      }
    }

    /// <summary>
    /// 隐藏所有已创建的小球和编号
    /// </summary>
    private void HideAllSpheres()
    {
      foreach (var sphere in _landmarkSpheres)
      {
        if (sphere != null && sphere.activeSelf)
        {
          sphere.SetActive(false);
        }
      }

      foreach (var label in _indexLabels)
      {
        if (label != null && label.gameObject.activeSelf)
        {
          label.gameObject.SetActive(false);
        }
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

    /// <summary>
    /// 如果你需要在其他地方访问当前的姿态特征点，可以通过这个方法获取。
    /// </summary>
    public IReadOnlyList<NormalizedLandmark> GetRealtimePoseLandmarks()
    {
      return _currentPoseLandmarkList;
    }
    
  }
}
