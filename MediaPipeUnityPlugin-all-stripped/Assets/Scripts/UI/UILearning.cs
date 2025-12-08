using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILearning : Singleton<UILearning>
{
  public GameObject moveSuggestUI;
  public GameObject trickButton;
  private CanvasGroup moveSuggestUICanvasGroup;
  private TMP_Text moveSuggestUIText;
  public GameObject nextPoseText; 
  public Transform progressCircleGroup;
  public GameObject progressCircle;
  public List<GameObject> progressCircles;

  void Start()
  {
    moveSuggestUICanvasGroup = moveSuggestUI.GetComponent<CanvasGroup>();
    moveSuggestUIText = moveSuggestUI.transform.GetChild(0).GetComponent<TMP_Text>();
    StartCoroutine(ShowMoveSuggestUI());
  }

  public IEnumerator ShowMoveSuggestUI()
  {
    
    
    moveSuggestUICanvasGroup.alpha = 1;
    moveSuggestUICanvasGroup.blocksRaycasts = true;
    moveSuggestUIText.text = "当前招式:" + GameManager.Instance.moveSos[GameManager.Instance.currentMove].name;
    yield return new WaitForSeconds(2); 
    moveSuggestUICanvasGroup.DOFade(0, 2).OnComplete(() => {
      moveSuggestUICanvasGroup.blocksRaycasts = false;trickButton.SetActive(true);
    });
    foreach (GameObject go in progressCircles)
    {
      Destroy(go);
    }
    progressCircles.Clear();
    for (int i = 0; i < GameManager.Instance.moveSos[GameManager.Instance.currentMove].poseCount; i++)
    {
      var c = Instantiate(progressCircle, progressCircleGroup);
      progressCircles.Add(c);
    }
    yield return null;
  }

  public void UpdateProgressCircles(int index)
  {
    for (int i = 0; i < index+1; i++)
    {
      progressCircles[i].GetComponent<Image>().color = Color.white;
    }
  }
  
}
