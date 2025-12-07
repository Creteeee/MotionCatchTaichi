using System;
using System.Collections;
using System.Collections.Generic;
using Mediapipe;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BoneRelocator : MonoBehaviour
{
  
  public Vector3[] PoseLandmarkWorldposArray;
  public Vector3 hipsBasePos;
  public Vector3 baseHipCenter;
  public bool isInitialized = false;
  private Vector3 lastDelta;
  public GameObject footPlane;
  

  [Header("Fluid Motion")] public ParticleSystem chiBall;
  private float initialHandDistances;
  private Vector3 initialChiBallLocalscale;
  private float startSize;
  
  



  //整理
  public Bones bt;//bonesTransforms

  private void Start()
  {
    hipsBasePos = bt.Hips.localPosition;
    baseHipCenter = PoseLandmarkWorldposArray[11] * 0.08f + PoseLandmarkWorldposArray[12] * 0.08f + 
                    PoseLandmarkWorldposArray[23] * 0.42f + PoseLandmarkWorldposArray[24] * 0.42f;//可能有生命周期问题
    initialHandDistances = Vector3.Distance(bt.LeftHand.position, bt.RightHand.position);
    initialChiBallLocalscale = chiBall.transform.localScale;
    startSize = chiBall.main.startSize.constant;



  }

  private void Update()
  {
    CalculateBoneRotations();
    
    //气体运动
    Vector3 ballPos = bt.LeftHand.position * 0.5f + bt.RightHand.position * 0.5f;
    chiBall.transform.parent.position = ballPos;
    float currentHandDistances = Vector3.Distance(bt.LeftHand.position, bt.RightHand.position);
    float handDistanceFactor = currentHandDistances/initialHandDistances;
    handDistanceFactor = Mathf.Sqrt(handDistanceFactor);
    //chiBall.transform.localScale = handDistanceFactor * initialChiBallLocalscale;
    ParticleSystem.NoiseModule noiseModule = chiBall.noise;
    ParticleSystem.MainModule mainModule = chiBall.main;
    mainModule.startSize = startSize*handDistanceFactor*1.2f;
    noiseModule.strength = Mathf.Lerp(0,2,handDistanceFactor);



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
    var dir1 = Vector3.Normalize(pos0 - pos1);
    var dir2 = Vector3.Normalize(pos3 - pos4);
    Quaternion rot = Quaternion.FromToRotation(dir2, dir1);
    Quaternion rot1 = bone2.rotation;
    bone1.rotation = rot * rot1;
  }

  public void CalculateBoneRotations()
  {
    if (PoseLandmarkWorldposArray == null || PoseLandmarkWorldposArray.Length < 33)
    {
      return;
    }
    
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
    var l16 = PoseLandmarkWorldposArray[16];
    var l17 = PoseLandmarkWorldposArray[17];
    var l18 = PoseLandmarkWorldposArray[18];
    var l19 = PoseLandmarkWorldposArray[19];
    var l20 = PoseLandmarkWorldposArray[20];
    var l21 = PoseLandmarkWorldposArray[21];
    var l22 = PoseLandmarkWorldposArray[22];
    var l23 = PoseLandmarkWorldposArray[23]; // Left hip
    var l24 = PoseLandmarkWorldposArray[24]; // Right hip
    var l25 = PoseLandmarkWorldposArray[25];
    var l26 = PoseLandmarkWorldposArray[26];
    var l27 = PoseLandmarkWorldposArray[27];
    var l28 = PoseLandmarkWorldposArray[28];
    var l29 = PoseLandmarkWorldposArray[29];
    var l30 = PoseLandmarkWorldposArray[30];
    var l31 = PoseLandmarkWorldposArray[31];
    var l32 = PoseLandmarkWorldposArray[32];

    if (!isInitialized)
    {
      baseHipCenter = PoseLandmarkWorldposArray[11] * 0.08f + PoseLandmarkWorldposArray[12] * 0.08f + 
                      PoseLandmarkWorldposArray[23] * 0.42f + PoseLandmarkWorldposArray[24] * 0.42f;
      isInitialized = true;
    }

    Vector3 l_hips = l11 * 0.08f + l12 * 0.08f + l23 * 0.42f + l24 * 0.42f;
    Vector3 l_spine = l11 * 0.15f + l12 * 0.15f + l23 * 0.35f + l24 * 0.35f;
    Vector3 l_spine1 = (l11 + l12 + l23 + l24) * 0.25f;
    Vector3 l_spine2 = l11 * 0.35f + l12 * 0.35f + l23 * 0.15f + l24 * 0.15f;

    float shoulderWidth = Vector3.Distance(l11, l12) * 0.5f;
    Vector3 l_LeftShoulder = l_spine2 + (l11 - l_spine2).normalized * shoulderWidth;
    Vector3 l_RightShoulder = l_spine2 + (l12 - l_spine2).normalized * shoulderWidth;


    Vector3 l_handCenterLeft = (l17 + l19 + l15) / 3;
    Vector3 l_handCenterRight = (l18 + l20 + l16) / 3;
    Vector3 l_neck = (l9 + l10 + l11 + l12) / 4;
    Vector3 l_head = (l0 + l1 + l2 + l3 + l4 + l5 + l6 + l7 + l8 + l9 + l10) / 11;
    Vector3 l_eyeCenter = (l1 + l4) / 2;
    Vector3 l_leftFootCenter = (l29 + l31) / 2;
    Vector3 l_rightFootCenter = (l30 + l32) / 2;


    //Hips,平移移动Armature
    RotateBone(l_spine, l_hips, l_hips, l_hips - Vector3.up, bt.Hips, bt.Armature);
    //Legs Rotation
    RotateBone(l25, l23, l_spine, l_hips, bt.LeftUpLeg, bt.Hips);
    RotateBone(l27, l25, l25, l23, bt.LeftLeg, bt.LeftUpLeg);
    RotateBone(l_leftFootCenter, l27, l27, l25, bt.LeftFoot, bt.LeftLeg);
    RotateBone(l26, l24, l_spine, l_hips, bt.RightUpLeg, bt.Hips);
    RotateBone(l28, l26, l26, l24, bt.RightLeg, bt.RightUpLeg);
    RotateBone(l_rightFootCenter, l28, l28, l26, bt.RightFoot, bt.RightLeg);

    //Spines
    RotateBone(l_spine1, l_spine, l_spine, l_hips, bt.Spine, bt.Hips);
    RotateBone(l_spine2, l_spine1, l_spine1, l_spine, bt.Spine1, bt.Spine);
    RotateBone(l_neck, l_spine2, l_spine2, l_spine1, bt.Spine2, bt.Spine1);

    //Shoulders
    RotateBone(l11, l_LeftShoulder, l_neck, l_spine2, bt.LeftShoulder, bt.Spine2);
    RotateBone(l12, l_RightShoulder, l_neck, l_spine2, bt.RightShoulder, bt.Spine2);

    // Upper arm rotation
    RotateBone(l13, l11, l11, l_LeftShoulder, bt.LeftArm, bt.LeftShoulder);
    RotateBone(l14, l12, l12, l_RightShoulder, bt.RightArm, bt.RightShoulder);

    // Forearm rotation
    RotateBone(l15, l13, l13, l11, bt.LeftForeArm, bt.LeftArm);
    RotateBone(l16, l14, l14, l12, bt.RightForeArm, bt.RightArm);

    // Wrist rotationrot
    RotateBone(l_handCenterLeft, l15, l15, l13, bt.LeftHand, bt.LeftForeArm);
    RotateBone(l_handCenterRight, l16, l16, l14, bt.RightArm, bt.RightArm);

    //Head
    RotateBone(l_eyeCenter, l_head, l_head, l_neck, bt.Head, bt.Neck);
    
    //计算Hip相对移动
    
    //确保脚趾贴地
    
    Vector3 
      delta = l_hips - baseHipCenter;
    float referenceShoulderWidth = Vector3.Distance(bt.LeftArm.position, bt.RightArm.position);
    float scaleFactor = referenceShoulderWidth / Vector3.Distance(l11, l12);
    
    
    delta = delta*scaleFactor;
    delta.z = -delta.z * 0f;//z暂时不动了
   // Debug.Log("deltaY"+deltaY);
    if (!float.IsFinite(delta.y))
    {
      delta = Vector3.zero;
    }

    
   //Debug.Log("和上一帧的差距"+Vector3.Distance(lastDelta,delta));
    if (Vector3.Distance(lastDelta,delta)>=1)
    {
      return;
    }
    float footLow = Mathf.Min(l_leftFootCenter.y, l_rightFootCenter.y);
    lastDelta = delta;
    bt.Hips.localPosition = hipsBasePos+delta;
    footPlane.transform.position = new Vector3(footPlane.transform.position.x, footLow, footPlane.transform.position.z);

  }

  #endregion

  [System.Serializable]
  public class Bones
  {
    public Transform Armature;

    public Transform Hips;
    public Transform LeftUpLeg;
    public Transform LeftLeg;
    public Transform LeftFoot;
    public Transform RightUpLeg;
    public Transform RightLeg;
    public Transform RightFoot;

    public Transform Spine;
    public Transform Spine1;
    public Transform Spine2;
    public Transform LeftShoulder;
    public Transform LeftArm;
    public Transform LeftForeArm;
    public Transform LeftHand;
    public Transform RightShoulder;
    public Transform RightArm;
    public Transform RightForeArm;
    public Transform RightHand;

    public Transform Neck;
    public Transform Head;
  }
}






