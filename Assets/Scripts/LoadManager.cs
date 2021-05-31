using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class LoadManager : MonoBehaviour
{

    public void PlayGame(){

        SceneManager.LoadScene("Game");
    }
}
