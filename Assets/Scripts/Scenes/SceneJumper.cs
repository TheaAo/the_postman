using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneJumper: MonoBehaviour
{
  [Header("Setup")]
  public string destination = "StartScene";
  public string message = "";

  public void LoadNextScene()
  {
    Debug.Log($"Loading scene: {destination} with message: {message}");

    SceneTransitionManager.Instance.LoadScene(destination, message);
  }

}