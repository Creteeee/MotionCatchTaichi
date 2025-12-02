using System;
using System.Collections;
using System.Collections.Generic;
using Mediapipe;
using UnityEngine;

public class BoneRelocator : MonoBehaviour
{
  public List<LandmarkBone> bones = new List<LandmarkBone>(33);
  Vector3 modelForwardAxis = Vector3.right; // local axis，Mixamo默认的模型方向，试下看看对不对
  Vector3 modelUpAxis = Vector3.up;         // local up axis
  
  //先小小尝试下两个胳膊的节点可不可以正常旋转
  Quaternion Q11 = Quaternion.identity;
  Quaternion Q13 = Quaternion.identity;
  Quaternion Q15 = Quaternion.identity;
  public Vector3[] PoseLandmarkWorldposArray;
  //额外补充的骨骼
  public Transform Spine2;

  void Reset()
  {
    bones.Clear();
    for (int i = 0; i < 33; i++)
    {
      bones.Add(new LandmarkBone {
        landmark = (PoseLandmark)i,
        bone = null
      });
    }
  }

  private void Update()
  {
    CalculateBoneRotations();
    bones[11].bone.localRotation = Q11;
    bones[13].bone.localRotation = Q13;
    bones[15].bone.localRotation = Q15;
  }

  public Transform GetBone(int landmarkIndex)
  {
    return bones[landmarkIndex].bone;
  }

  
  #region 骨骼旋向计算
  /// <summary>
  /// 计算子骨骼相对于父骨骼的四元数
  /// </summary>
  /// <param name="parentPos"></param>
  /// <param name="childPos"></param>
  /// <param name="bone"></param>
  /// <param name="modelForwardAxis"></param>
  /// <param name="modelUpAxis"></param>
  /// <returns></returns>
  public static Quaternion ComputeLocalRotation(
    Vector3 parentPos,
    Vector3 childPos,
    Transform bone,
    Vector3 modelForwardAxis,
    Vector3 modelUpAxis)
  {
    //获取 MediaPipe 的方向（世界坐标）
    Vector3 targetDir = (childPos - parentPos).normalized;

    //获取模型的“初始朝向”
    //一般 mixamo 的骨骼指向 local X 轴（根据你的模型可能要调整）
    Vector3 boneAxis = bone.rotation * modelForwardAxis;

    //求出骨骼当前朝向 → 目标朝向 的旋转
    Quaternion toTarget = Quaternion.FromToRotation(boneAxis, targetDir);

    //计算新的世界旋转
    Quaternion newWorldRot = toTarget * bone.rotation;

    //换算成局部旋转（相对于父）
    return Quaternion.Inverse(bone.parent.rotation) * newWorldRot;
  }

  public void CalculateBoneRotations()
  {
    var l11 = PoseLandmarkWorldposArray[11];
    var l12 = PoseLandmarkWorldposArray[12];
    var l13 = PoseLandmarkWorldposArray[13];
    var l15 = PoseLandmarkWorldposArray[15];
    var l23 = PoseLandmarkWorldposArray[23];
    var l24 = PoseLandmarkWorldposArray[24];
    // //虚构一个虚空骨骼
    var l_spine2 = (l11 + l12 + l23 + l24)/4;
    // boneNeck.rotation = Quaternion.Euler(0,0,0);//这个估计不太对以后改
    // boneNeck.localScale = Vector3.one;
    
    
    Quaternion q11 = ComputeLocalRotation(l_spine2, l11, Spine2, modelForwardAxis, modelUpAxis);
    Quaternion q13 = ComputeLocalRotation(l11, l13, bones[11].bone, modelForwardAxis, modelUpAxis);
    Quaternion q15 = ComputeLocalRotation(l13, l15, bones[13].bone, modelForwardAxis, modelUpAxis);
    Q11 = q11;
    Q13 = q13;
    Q15 = q15;
  } 
  
  
  
  #endregion
    
}

/// <summary>
/// 骨骼名称和Transform对应
/// </summary>
[System.Serializable]
public struct LandmarkBone
{
  public PoseLandmark landmark;   // 显示名字用
  public Transform bone;          // 你要拖放的骨骼
}

/// <summary>
/// 骨骼名称
/// </summary>
public enum PoseLandmark
{
  Nose = 0,
  LeftEyeInner = 1,
  LeftEye = 2,
  LeftEyeOuter = 3,
  RightEyeInner = 4,
  RightEye = 5,
  RightEyeOuter = 6,
  LeftEar = 7,
  RightEar = 8,
  MouthLeft = 9,
  MouthRight = 10,
  LeftShoulder = 11,
  RightShoulder = 12,
  LeftElbow = 13,
  RightElbow = 14,
  LeftWrist = 15,
  RightWrist = 16,
  LeftPinky = 17,
  RightPinky = 18,
  LeftIndex = 19,
  RightIndex = 20,
  LeftThumb = 21,
  RightThumb = 22,
  LeftHip = 23,
  RightHip = 24,
  LeftKnee = 25,
  RightKnee = 26,
  LeftAnkle = 27,
  RightAnkle = 28,
  LeftHeel = 29,
  RightHeel = 30,
  LeftFootIndex = 31,
  RightFootIndex = 32
}




