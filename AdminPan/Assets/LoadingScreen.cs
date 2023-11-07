using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    public GameObject loadingBar;
    public static LoadingScreen instance;

    private void Awake()
    {
        if(instance == null) instance= this;
        else Destroy(this);
    }

    public void Show(bool show)
    {
        loadingBar.SetActive(show);
    }
   
}
