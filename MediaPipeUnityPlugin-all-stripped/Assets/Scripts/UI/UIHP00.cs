using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIHP00 : Singleton<UIHP00>
{
  public TMP_Text name;
  private string currentInput = "";

  private void Start()
  {
    StartCoroutine(WaitForEnter());
  }

  private void Update()
  {
    // 本帧输入的字符（可能是2个，比如"aa"）
    string frameInput = Input.inputString;
    

    if (!string.IsNullOrEmpty(frameInput))
    {
      foreach (char c in frameInput)
      {
        if (c == '\b')   // 删除键
        {
          if (currentInput.Length > 0)
            currentInput = currentInput.Substring(0, currentInput.Length - 1);
        }
        else if (c == '\n' || c == '\r') // Enter（两种情况）
        {
          SetName(currentInput);   // 用你的名字检测方法
          Debug.Log("Enter pressed, name submitted.");
        }
        else
        {
          currentInput += c;   // 普通字符
        }
      }

      // 实时更新到 UI（可选）
      SetName(currentInput);
    }
  }

  /// <summary>
  /// 设置名字，自动检查中英文长度限制（中文 <=5，英文 <=10）
  /// </summary>
  public void SetName(string input)
  {
    if (string.IsNullOrEmpty(input))
    {
      name.text = "";
      return;
    }

    int length = GetMixedLength(input);

    if (HasChinese(input))
    {
      if (length > 5)
      {
        Debug.Log("名字（中文）不得超过 5 个字");
        return;
      }
    }
    else // 纯英文或其它字符
    {
      if (length > 10)
      {
        Debug.Log("名字（英文）不得超过 10 个字符");
        return;
      }
    }

    name.text = input;
 
    //这里少名字是否存在的判断
    // if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))&&name.text.Length>0)
    // {
    //   GameManager.Instance.PlayerEnter();
    //   GameManager.Instance.MoveToNextUI(this.gameObject,GameManager.Instance.uIPrefabs.UI_Progress);
    // }
  }
  
  IEnumerator WaitForEnter()
  {
    while (true)
    {
      if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
          && name.text.Length > 0)
      {
        GameManager.Instance.playerName = name.text;
        GameManager.Instance.PlayerEnter();
        Debug.Log("玩家的名字是"+name.text);
        GameManager.Instance.MoveToNextUI(this.gameObject, GameManager.Instance.uIPrefabs.UI_Progress);
        yield break; //协程结束
      }

      yield return null;
    }
  }

  /// <summary>
  /// 中文按 1，英文字符按 1
  /// </summary>
  private int GetMixedLength(string text)
  {
    int count = 0;
    foreach (char c in text)
    {
      count++;
    }
    return count;
  }

  /// <summary>
  /// 判断是否包含中文
  /// </summary>
  private bool HasChinese(string s)
  {
    foreach (char c in s)
    {
      if (c >= 0x4E00 && c <= 0x9FFF)
        return true;
    }
    return false;
  }
}
