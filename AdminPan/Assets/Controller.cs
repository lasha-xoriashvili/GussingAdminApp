using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public static Controller instance;
    public DatabaseController databaseController;
    public StorageController storageController;
    public UIController uiController;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(instance);
    }
}
