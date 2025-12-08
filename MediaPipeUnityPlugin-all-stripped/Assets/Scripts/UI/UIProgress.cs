using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIProgress : Singleton<UIProgress>
{
  private float progress = 0;
  public float duration = 3;
  public Slider slider;

  private void Start()
  { 
    slider.value = 0;
    StartCoroutine(Proceeding(duration));
  }

  IEnumerator Proceeding(float duration)
  {
    for (float t = 0f; t < duration; t += Time.deltaTime)
    {
      progress = Mathf.Clamp01(t / duration);
      slider.value = progress;
      yield return null;
    }

    progress = 1f;
    slider.value = progress;

    GameManager.Instance.LoadLearningScene();
    GameManager.Instance.MoveToNextUI(this.gameObject,GameManager.Instance.uIPrefabs.UI_Learing);
  }

}
