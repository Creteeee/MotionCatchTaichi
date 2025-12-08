using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHelpers : MonoBehaviour
{
  public void CallCheckUI()
  {
    GameManager.Instance.MotionChecked();
  }

  public void CallRestartUI()
  {
    UILearning.Instance.trickButton.SetActive(false);
    StartCoroutine(GameManager.Instance.RestartGame());
  }
}
