using System;
using System.Collections;
using System.Collections.Generic;
using Mediapipe;
using UnityEditor;
using UnityEngine;

public class BoneRelocator : MonoBehaviour
{
  public List<LandmarkBone> bones = new List<LandmarkBone>(33);
  Vector3 modelForwardAxis = Vector3.right; // local axis，Mixamo默认的模型方向，试下看看对不对
  Vector3 modelUpAxis = Vector3.forward;         // local up axis
  public float lerp;
  Vector3 realLeftShoulder = Vector3.zero;
  
  //先小小尝试下两个胳膊的节点可不可以正常旋转

  public Vector3[] PoseLandmarkWorldposArray;
  //额外补充的骨骼
  public Transform Spine2;
  public Transform Neck;
  public Transform LeftArm;
  public float shouderW=0.75f;

  private int time = 0;

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
    lerp += Time.deltaTime;
    if (lerp >= 1.0f)
    {
      lerp = 0;
    }
    CalculateBoneRotations();
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
    // 世界空间的目标方向
    Vector3 targetDir = (childPos - parentPos).normalized;

    // 计算 up（避免 roll）
    // 先取一个世界空间的向上
    Vector3 worldUp = Vector3.up;
    // 避免 up 和 forward 平行
    if (Mathf.Abs(Vector3.Dot(targetDir, worldUp)) > 0.99f)
      worldUp = Vector3.right;

    // 世界空间下的目标旋转
    Quaternion targetWorldRot = Quaternion.LookRotation(
      targetDir,
      worldUp
    );

    // 把模型的 "forward" 和 "up" 轴对齐
    Quaternion boneModelRot = Quaternion.LookRotation(
      bone.TransformDirection(modelForwardAxis),
      bone.TransformDirection(modelUpAxis)
    );

    // 计算本地旋转
    Quaternion newLocalRot = Quaternion.Inverse(bone.parent.rotation) *
                             targetWorldRot *
                             Quaternion.Inverse(boneModelRot);
    

    return newLocalRot;
  }

  private void RotateBone(Vector3 pos0, Vector3 pos1, Vector3 pos3, Vector3 pos4, Transform bone1, Transform bone2)
  {
    //pos0 pos1当前骨骼方向，pos3 pos4 父骨骼方向
    var dir1 = Vector3.Normalize(pos0-pos1);
    var dir2 = Vector3.Normalize(pos3-pos4);
    // dir1 = new Vector3(0, -1, 0);
    // dir2 = new Vector3(0, 1, 0);
    Quaternion rot = Quaternion.FromToRotation(dir2,dir1);
    Quaternion rot1 = bone2.rotation;

    bone1.rotation = rot*rot1;;
    
  }





  public void CalculateBoneRotations()
{
    // 1. MediaPipe 原始点
    var l0 = PoseLandmarkWorldposArray[0];
    var l1 = PoseLandmarkWorldposArray[1];
    var l2 = PoseLandmarkWorldposArray[2];
    var l3 = PoseLandmarkWorldposArray[3];
    var l4 = PoseLandmarkWorldposArray[4];
    var l5 = PoseLandmarkWorldposArray[5];
    var l6 = PoseLandmarkWorldposArray[6];
    var l7 = PoseLandmarkWorldposArray[7];
    var l8 = PoseLandmarkWorldposArray[8];
    var l9 = PoseLandmarkWorldposArray[9];
    var l10 = PoseLandmarkWorldposArray[10];
    var l11 = PoseLandmarkWorldposArray[11]; // Left shoulder landmark (外侧)
    var l12 = PoseLandmarkWorldposArray[12]; // Right shoulder landmark
    var l13 = PoseLandmarkWorldposArray[13]; // Left elbow
    var l15 = PoseLandmarkWorldposArray[15]; // Left wrist
    var l17 = PoseLandmarkWorldposArray[17];
    var l18 = PoseLandmarkWorldposArray[18];
    var l19 = PoseLandmarkWorldposArray[19];
    var l20 = PoseLandmarkWorldposArray[20];
    var l21 = PoseLandmarkWorldposArray[21];
    var l23 = PoseLandmarkWorldposArray[23]; // Left hip
    var l24 = PoseLandmarkWorldposArray[24]; // Right hip

    // ---------------------------
    // (A) 计算胸部（真实中心点）
    // ---------------------------
    Vector3 chest = (l11 + l12 + l23 + l24) * 0.25f;

    // ---------------------------
    // (B) 估算真实肩膀位置（关节位置）
    // ---------------------------
    float shoulderWidth = Vector3.Distance(l11, l12) * 0.8f;
    shoulderWidth = shouderW;

    
    if (time <=0)
    {
      realLeftShoulder = new Vector3(l11.x, l11.y, l11.z);
      time += 1;
    }

    realLeftShoulder.z = l11.z;
    
    realLeftShoulder = chest + (l11 - chest).normalized * shoulderWidth;
    

    Vector3 realRightShoulder =
        chest + (l12 - chest).normalized * shoulderWidth;

    // ---------------------------
    // (C) 估算上臂位置（用于旋转平滑）
    // ---------------------------
    Vector3 midLeftUpperArm = Vector3.Lerp(realLeftShoulder, l13, 0.5f);
    Vector3 handCenterLeft = (l17 + l19 + l21) / 3;
    Vector3 neck = (l9 + l10 + l11 + l12) / 4;
    Vector3 mouthCenter = (l9 + l10) / 2;
    Vector3 head = l0;
    Vector3 eyeCenter = (l1+l4)/2;
    

    // ---------------------------
    // (D) 驱动 Mixamo旋转
    // ---------------------------

    // Shoulder rotation
    Quaternion qShoulder = ComputeLocalRotation(
        chest,
        l11,
        bones[11].bone,
        -Vector3.up,    // 本地 forward = Z
        Vector3.forward          // 本地 up = Y
    );
    Quaternion rot1 = bones[11].bone.transform.localRotation;
    RotateBone(l11, realLeftShoulder, chest+Vector3.up,chest , bones[11].bone,Spine2);
    //bones[11].bone.localRotation = Quaternion.Lerp(rot1,qShoulder*rot1,lerp);

    // Upper arm rotation
    Quaternion qUpperArm = ComputeLocalRotation(
        realLeftShoulder,
        midLeftUpperArm,
        LeftArm,
        -Vector3.up,
        Vector3.forward
    );
    Quaternion rot2 = LeftArm.transform.localRotation;
    RotateBone(l13, l11, l11, realLeftShoulder, LeftArm,bones[11].bone);
    //LeftArm.localRotation = Quaternion.Lerp(rot2,qShoulder*rot2,lerp);
    
    // Forearm rotation
    Quaternion qForearm = ComputeLocalRotation(
        midLeftUpperArm,
        l13,
        bones[13].bone,
        -Vector3.up,
        Vector3.forward
    );
    Quaternion rot3 = bones[13].bone.transform.localRotation;
    RotateBone(l15, l13, l13, midLeftUpperArm, bones[13].bone,LeftArm);
    //LeftArm.localRotation = Quaternion.Lerp(rot3,qShoulder*rot3,lerp);
    
    // Wrist rotationrot
    Quaternion qWrist = ComputeLocalRotation(
        l13,
        l15,
        bones[15].bone,
        -Vector3.up,
        Vector3.forward
    );
    Quaternion rot4 = bones[13].bone.transform.localRotation;
    RotateBone(handCenterLeft, l15, l15, l13, bones[15].bone,bones[13].bone);
    //bones[15].bone.localRotation = Quaternion.Lerp(rot4,qShoulder*rot4,lerp);
        
    RotateBone(eyeCenter,mouthCenter,mouthCenter,neck,bones[0].bone,Neck);
    
}


  #endregion

  private void OnDrawGizmos()
  {
    if (!Application.isPlaying) return;
    var l0 = PoseLandmarkWorldposArray[0];
    var l11 = PoseLandmarkWorldposArray[11];
    var l12 = PoseLandmarkWorldposArray[12];
    var l23 = PoseLandmarkWorldposArray[23];
    var l24 = PoseLandmarkWorldposArray[24];
    var l_neck = (l11 + l12 )/2;
    var l_spine2 = (l11 + l12 + l23 + l24)/4;
    Gizmos.DrawRay(Neck.position,l0-l_neck);
    
  }
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




