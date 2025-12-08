using System.Collections;
using System.Threading.Tasks;
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
  private int currentMove = 0;
  private int currentPose = 0;
  
  

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
  }

  public void MotionChecked()
  {
    if (isInPlace == false)
    {
      isInPlace = true;
      SendOSC.Instance.SendOSCMessage(playerName+":完成录制_"+moveSos[currentMove]+"_0");
      return;
    }
    else
    {
      if (currentPose < moveSos[currentMove].poseCount-1)
      {
        SendOSC.Instance.SendOSCMessage(playerName+":完成录制_"+moveSos[currentMove]+"_"+(currentPose+1));
        currentPose += 1;
        StartCoroutine(PrepairingNextMotion(2));
      }
      else
      {
        SendOSC.Instance.SendOSCMessage(playerName+":完成录制_"+moveSos[currentMove]+"_"+(currentPose+1));
        currentMove += 1;
        currentPose = 0;
        isInPlace = false;
      }
    }
  }

  IEnumerator PrepairingNextMotion(float time)
  {
    // 等待
    yield return new WaitForSeconds(time);

    //再触发 OSC 消息
    SendOSC.Instance.SendOSCMessage(
      playerName + ":开始录制_" + moveSos[currentMove] + "_" + (currentPose + 1)
    );
  }


  public void RecordStart()
  {
    
  }
  
  public void RecordEnd()
  {
    
  }
  #endregion
  
  
  
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
