using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mediapipe.Unity.Sample.Holistic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
  [Header("UI")]
  private static GameObject activeUI;//当前激活的UI
  public Transform UIRoot;//UICanvas
  public UIPrefabs uIPrefabs;
  [Header("Scene")]
  public SceneAsset learningScene;
  [Header("UserInfo")]
  public string playerName;
  [Header("动作库")]
  public MoveSO[] moveSos;

  private bool isInPlace = false;//因为就位动作单独拿出来不占数量所以加个bool
  public int currentMove = 0;
  public int currentPose = 0;

  [Header("姿态检测设置")] private string poseBaseFolder = "Assets/Data/CorrectMoves";//姿态库根目录
  private List<Vector3> dirList = new List<Vector3>();// Gizmos画线用
  bool isCorrect = false;//动作是否做对
  private static Vector3[] correctLandmarkPoses;
  public Texture2D dotTex;
  public Vector3 debugOffest = new Vector3(0.1f, 0, 0);

  private void Update()
  {
    if (SceneManager.GetActiveScene().name=="Holistic" )
    {
      
      CheckingMotion();
    }
  }


  #region UI
  /// <summary>
  /// 切换UI
  /// </summary>
  /// <param name="UI1"></param>
  /// <param name="UI2"></param>
  public void MoveToNextUI(GameObject UI1, GameObject UI2)
  {
    activeUI = Instantiate(UI2, UIRoot);
    Destroy(UI1);
  }
  #endregion
  
  #region SceneManagement
  public async  void LoadLearningScene()
  {
    var op = SceneManager.LoadSceneAsync(learningScene.name, LoadSceneMode.Additive);
    while (!op.isDone)
    {
      await Task.Yield();
    }
    Scene loadedScene = SceneManager.GetSceneByName(learningScene.name);
    SceneManager.SetActiveScene(loadedScene);
   
  }

  public void UnloadLearningScene()
  {
    SceneManager.UnloadSceneAsync(learningScene.name);
  }

  public IEnumerator RestartGame()
  {
    UILearning.Instance.nextPoseText.GetComponent<TMP_Text>().text = "正在退出游戏";
    yield return new WaitForSeconds(2f);
    SceneManager.UnloadSceneAsync(learningScene.name);
    MoveToNextUI(UIRoot.transform.GetChild(1).gameObject, uIPrefabs.UI_HP_00);
    SendOSC.Instance.SendOSCMessage(playerName+":GameEnd");
    
  }
  #endregion
  #region 游戏进程

  /// <summary>
  /// 玩家进入游戏
  /// </summary>
  public void PlayerEnter()
  {
    
    //SendOSC.Instance.SendOSCMessage("Player:"+playerName);
    SendOSC.Instance.SendOSCMessage(playerName+":GameStart");
  }

  /// <summary>
  /// 玩家结束游戏
  /// </summary>
  public void PlayerExit()
  {
    SendOSC.Instance.SendOSCMessage(playerName+":GameEnd");
    
    playerName =  null;
  }


  void CheckingMotion()
  {
    CorrectMoveSO correctSO = new CorrectMoveSO();
    if (isInPlace)
    {
      correctSO= LoadCorrectPose(currentMove, currentPose+1);
    }
    if(!isInPlace)
    {
      correctSO= LoadCorrectPose(currentMove, 0);
    }
     
     if (correctSO == null)
     {
       return;
     }
     
     Vector3[] realtimeLandmarkPoses = AvatarBonePositionTest._currentPostWorldPosArray;
     if (!IsLandmarkValid(realtimeLandmarkPoses))
     {
       Debug.Log("Landmark 未准备好，等待中...");
       return;
     }
     if (realtimeLandmarkPoses.Length!=33)
     {
       return;
     }
     Vector3[] localCorrecctLandmarkPoses = correctSO.poseLandmarks;
     correctLandmarkPoses = localCorrecctLandmarkPoses;
     Debug.Log("当前的SO是"+correctSO.name);
     Debug.Log("正确答案的第一个点是");
    
     isCorrect = PoseDetect(realtimeLandmarkPoses, localCorrecctLandmarkPoses, out dirList);
     if (isCorrect)
     {
       MotionChecked();
       isCorrect = false;
     }
  }
  bool IsLandmarkValid(Vector3[] poses)
  {
    if (poses == null || poses.Length < 33)
      return false;

    // 如果全部坐标一样，也算无效
    for (int i = 1; i < poses.Length; i++)
    {
      if (poses[i] != poses[0])
        return true;   // 发现不同点 → 有效
    }

    return false;
  }

  public void MotionChecked()
  {
    var text = UILearning.Instance.nextPoseText.GetComponent<TMP_Text>();
    
    if (isInPlace == false)
    {
      isInPlace = true;
      text.text = "准备动作已就绪!进入第1个动作";
      StartCoroutine(PrepairingNextPose(2));
      UILearning.Instance.UpdateProgressCircles(currentPose);
      return;
    }
    else
    {
      if (currentPose < moveSos[currentMove].poseCount-1)
      {
        
        SendOSC.Instance.SendOSCMessage(playerName+":RecordEnd_"+moveSos[currentMove].nameIndex+"_"+(currentPose+1));
        currentPose += 1;
        text.text = "进入第"+(currentPose+1)+"个动作";
        StartCoroutine(PrepairingNextPose(2));
        UILearning.Instance.UpdateProgressCircles(currentPose);

      }
      else
      {
        if (currentMove == 5 && currentPose == moveSos[currentMove].poseCount - 1)
        {
          StartCoroutine(RestartGame());
          return;
        }
        SendOSC.Instance.SendOSCMessage(playerName+":RecordEnd_"+moveSos[currentMove].nameIndex+"_"+(currentPose+1));
        currentMove += 1;
        currentPose = 0;
        isInPlace = false;
        StartCoroutine(UILearning.Instance.ShowMoveSuggestUI());
      }
    }
  }

  public IEnumerator PrepairingNextPose(float time)
  {
    UILearning.Instance.nextPoseText.SetActive(true);
    UILearning.Instance.trickButton.SetActive(false);
    // 等待
    yield return new WaitForSeconds(time);
    //再触发 OSC 消息
    SendOSC.Instance.SendOSCMessage(
      playerName + ":RecordStart_" + moveSos[currentMove].nameIndex + "_" + (currentPose + 1)
    );
    UILearning.Instance.nextPoseText.SetActive(false);
    UILearning.Instance.trickButton.SetActive(true);
  }
  #endregion
  #region 读取姿态并检测
  /// <summary>
  /// 加载动作库文件
  /// </summary>
  /// <param name="currentMove"></param>
  /// <param name="currentPose"></param>
  /// <returns></returns>
  CorrectMoveSO LoadCorrectPose(int currentMove, int currentPose)
  {
    // currentMove: 0~24
    // currentPose: 0 准备姿势；1~n 正式动作

    string index = moveSos[currentMove].nameIndex;
    string fileName = $"Move_{index}_{currentPose}";
    string path = $"CorrectMoves/{fileName}";

    CorrectMoveSO so = Resources.Load<CorrectMoveSO>(path);

    if (so == null)
    {
      Debug.LogWarning($"CorrectMoveSO 未找到：{path}");
      return null;
    }

    return so;
  }

  bool PoseDetect(Vector3[] realtimeLandmarkPoses, Vector3[] correctLandmarkPoses, out List<Vector3> directionList)
  {
    Vector3 dirC_13_11 = Vector3.Normalize(correctLandmarkPoses[13] - correctLandmarkPoses[11]);
    Vector3 dirC_15_13 = Vector3.Normalize(correctLandmarkPoses[15] - correctLandmarkPoses[13]);
    Vector3 dirC_25_23 = Vector3.Normalize(correctLandmarkPoses[25] - correctLandmarkPoses[23]);
    
    Vector3 dirC_14_12 = Vector3.Normalize(correctLandmarkPoses[14] - correctLandmarkPoses[12]);
    Vector3 dirC_16_14 = Vector3.Normalize(correctLandmarkPoses[16] - correctLandmarkPoses[14]);
    Vector3 dirC_26_24 = Vector3.Normalize(correctLandmarkPoses[26] - correctLandmarkPoses[24]);
    
    Vector3 dirR_13_11 = Vector3.Normalize(realtimeLandmarkPoses[13] - realtimeLandmarkPoses[11]);
    Vector3 dirR_15_13 = Vector3.Normalize(realtimeLandmarkPoses[15] - realtimeLandmarkPoses[13]);
    Vector3 dirR_25_23 = Vector3.Normalize(realtimeLandmarkPoses[25] - realtimeLandmarkPoses[23]);
    
    Vector3 dirR_14_12 = Vector3.Normalize(realtimeLandmarkPoses[14] - realtimeLandmarkPoses[12]);
    Vector3 dirR_16_14 = Vector3.Normalize(realtimeLandmarkPoses[16] - realtimeLandmarkPoses[14]);
    Vector3 dirR_26_24 = Vector3.Normalize(realtimeLandmarkPoses[26] - realtimeLandmarkPoses[24]);

    float angle_13_11 = Vector3.Angle(dirC_13_11, dirR_13_11);
    float angle_15_13 = Vector3.Angle(dirC_15_13, dirR_15_13);
    float angle_25_23 = Vector3.Angle(dirC_25_23, dirR_25_23);
    
    float angle_14_12 = Vector3.Angle(dirC_14_12, dirR_14_12);
    float angle_16_14 = Vector3.Angle(dirC_16_14, dirR_16_14);
    float angle_26_24 = Vector3.Angle(dirC_26_24, dirR_26_24);
    
    List<Vector3> posList = new List<Vector3>();
    posList.Add(dirC_13_11);
    posList.Add(dirC_15_13);
    posList.Add(dirC_25_23);
    posList.Add(dirC_14_12);
    posList.Add(dirC_16_14);
    posList.Add(dirC_26_24);
    directionList = posList;
    
    
    bool b_13_11 = angle_13_11 <= 25 ? true : false;
    bool b_15_13 = angle_15_13 <= 25 ? true : false;
    bool b_25_23 = angle_25_23 <= 25 ? true : false;
    bool b_14_12 = angle_14_12 <= 25 ? true : false;
    bool b_16_14 = angle_16_14 <= 25 ? true : false;
    bool b_26_24 = angle_26_24 <= 25 ? true : false;

    // Debug.Log("左上臂"+angle_13_11);
    // Debug.Log("左下臂"+angle_15_13);
    // Debug.Log("右上臂"+angle_25_23);
    // Debug.Log("右下臂"+angle_14_12);
    //Debug.Log(angle_16_14);
    //Debug.Log(angle_26_24);
    
    if (b_13_11 &&  b_15_13 && b_14_12 && b_16_14)//先不算下肢
    {
      return true;
    }
    
    return false;
  }
  #endregion
  
  # region GUI
  private void OnGUI()
  {
    if (correctLandmarkPoses == null) return;

    // 使用主摄像机将3D点转换到屏幕空间
    Camera cam = Camera.main;//这里之后改下
    if (cam == null) return;

    // 画几个重要关节点
    DrawBone(cam, 11, 13, 15, Color.yellow); // 左臂 11-13-15
    DrawBone(cam, 12, 14, 16, Color.yellow); // 右臂 12-14-16

    DrawBone(cam, 23, 25, Color.cyan);       // 左腿 23-25
    DrawBone(cam, 24, 26, Color.cyan);       // 右腿 24-26
    //DrawDot(cam, correctLandmarkPoses[0]+debugOffest, 10);
  }
  
 

  void DrawDot(Camera cam,Vector3 p, float size = 10f)
  {
    p = cam.WorldToScreenPoint(p);
    GUI.color = Color.red;
    GUI.DrawTexture(new Rect(p.x - size/2f, p.y - size/2f, size, size), dotTex);
    GUI.color = Color.white;
  }
  


  private void DrawBone(Camera cam, int a, int b, int c, Color col)
  {
    Vector3 A = cam.WorldToScreenPoint(correctLandmarkPoses[a])+debugOffest;
    Vector3 B = cam.WorldToScreenPoint(correctLandmarkPoses[b])+debugOffest;
    Vector3 C = cam.WorldToScreenPoint(correctLandmarkPoses[c])+debugOffest;

    A.y = Screen.height - A.y;
    B.y = Screen.height - B.y;
    C.y = Screen.height - C.y;

    // AB
    DrawLine(A, B, col, 10f);
    // BC
    DrawLine(B, C, col, 10f);

    GUI.Label(new Rect(A.x, A.y, 1000, 200), a.ToString());
    GUI.Label(new Rect(B.x, B.y, 1000, 200), b.ToString());
    GUI.Label(new Rect(C.x, C.y, 1000, 200), c.ToString());
  }

  private void DrawBone(Camera cam, int a, int b, Color col)
  {
    Vector3 A = cam.WorldToScreenPoint(correctLandmarkPoses[a])+debugOffest;
    Vector3 B = cam.WorldToScreenPoint(correctLandmarkPoses[b])+debugOffest;

    A.y = Screen.height - A.y;
    B.y = Screen.height - B.y;

    DrawLine(A, B, col, 3f);

    GUI.Label(new Rect(A.x, A.y, 100, 20), a.ToString());
    GUI.Label(new Rect(B.x, B.y, 100, 20), b.ToString());
  }



  void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
  {
    Matrix4x4 matrix = GUI.matrix;

    Color savedColor = GUI.color;
    GUI.color = color;

    float angle = Vector3.Angle(pointB - pointA, Vector2.right);
    if (pointA.y > pointB.y) angle = -angle;

    float length = (pointB - pointA).magnitude;

    GUIUtility.RotateAroundPivot(angle, pointA);
    GUI.DrawTexture(new Rect(pointA.x, pointA.y, length, width), Texture2D.whiteTexture);

    GUI.matrix = matrix;
    GUI.color = savedColor;
  }

  # endregion
  
}

[System.Serializable]
public class UIPrefabs
{
  //UI都制作成prefab的格式，需要的时候实例化，逻辑各自写
  public GameObject UI_HP_00;
  public GameObject UI_HP_01;
  public GameObject UI_Progress;
  public GameObject UI_Learing;//学习时的UI
}
