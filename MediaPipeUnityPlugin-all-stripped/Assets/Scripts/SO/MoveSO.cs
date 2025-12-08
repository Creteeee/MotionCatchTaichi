using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMoveSO", menuName = "Moves/MoveSO")]
public class MoveSO : ScriptableObject
{
  public string nameIndex;
  public string name;
  public MoveType moveType;
  public int poseCount;
  public AnimationClip beginPose;
  public AnimationClip[] poseClips;
}

public enum MoveType
{
  soft,middle,hard
}
