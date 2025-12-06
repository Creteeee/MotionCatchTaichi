using System;
using System.Collections;
using System.Collections.Generic;
using Mediapipe;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BoneRelocator : MonoBehaviour
{
  public List<LandmarkBone> bones = new List<LandmarkBone>(33);
  public float lerp;
  
  //先小小尝试下两个胳膊的节点可不可以正常旋转

  public Vector3[] PoseLandmarkWorldposArray;
  //额外补充的骨骼
  public Transform Spine2;
  public Transform Neck;
  public Transform LeftArm;
  public Transform RightArm;
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
    lerp += Time.deltaTime*10;
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
  /// <param name="pos0"></param>
  /// <param name="pos1"></param>
  /// <param name="pos3"></param>
  /// <param name="pos4"></param>
  /// <param name="bone1"></param>
  /// <param name="bone2"></param>
  private void RotateBone(Vector3 pos0, Vector3 pos1, Vector3 pos3, Vector3 pos4, Transform bone1, Transform bone2)
  {
    //pos0 pos1当前骨骼方向，pos3 pos4 父骨骼方向
    var dir1 = Vector3.Normalize(pos0-pos1);
    var dir2 = Vector3.Normalize(pos3-pos4);
    Quaternion rot = Quaternion.FromToRotation(dir2,dir1);
    Quaternion rot1 = bone2.rotation;
    bone1.rotation = rot*rot1;
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
    var l14 = PoseLandmarkWorldposArray[14];
    var l15 = PoseLandmarkWorldposArray[15]; // Left wrist
    var l16 =  PoseLandmarkWorldposArray[16];
    var l17 = PoseLandmarkWorldposArray[17];
    var l18 = PoseLandmarkWorldposArray[18];
    var l19 = PoseLandmarkWorldposArray[19];
    var l20 = PoseLandmarkWorldposArray[20];
    var l21 = PoseLandmarkWorldposArray[21];
    var l22 = PoseLandmarkWorldposArray[22];
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
    
    Vector3 realLeftShoulder = chest + (l11 - chest).normalized * shoulderWidth;
    Vector3 realRightShoulder = chest + (l12 - chest).normalized * shoulderWidth;

    // ---------------------------
    // (C) 估算上臂位置（用于旋转平滑）
    // ---------------------------
    Vector3 midLeftUpperArm = Vector3.Lerp(realLeftShoulder, l13, 0.5f);
    Vector3 midRightUpperArm = Vector3.Lerp(realRightShoulder, l14, 0.5f);
    Vector3 handCenterLeft = (l17 + l19 + l21) / 3;
    Vector3 handCenterRight = (l18 + l20 +l22) / 3;
    Vector3 neck = (l9 + l10 + l11 + l12) / 4;
    Vector3 mouthCenter = (l9 + l10) / 2;
    Vector3 head = l0;
    Vector3 eyeCenter = (l1+l4)/2;
    
  
    // Shoulder rotation
    RotateBone(l11, realLeftShoulder, chest+Vector3.up,chest , bones[11].bone,Spine2);
    RotateBone(l12,realRightShoulder,chest+Vector3.up,chest , bones[12].bone,Spine2);

    // Upper arm rotation
    RotateBone(l13, l11, l11, realLeftShoulder, LeftArm,bones[11].bone);
    RotateBone(l14, l12, l12, realRightShoulder, RightArm,bones[12].bone);

    // Forearm rotation
    RotateBone(l15, l13, l13, midLeftUpperArm, bones[13].bone,LeftArm);
    RotateBone(l16, l14, l14, midRightUpperArm, bones[14].bone,RightArm);
  
    // Wrist rotationrot
    RotateBone(handCenterLeft, l15, l15, l13, bones[15].bone,bones[13].bone);
    RotateBone(handCenterRight,l16,l16,l14, bones[14].bone,bones[14].bone);
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




