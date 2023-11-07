using Firebase.Database;
using Firebase.Extensions;
using Google.MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DatabaseController : MonoBehaviour
{
   public static DatabaseController instance;
    FirebaseDatabase database;
    public DatabaseReference reference;
    public TMP_Text noLevel;
    public TMP_Text dateText;
    public List<LevelInfo> levelInfo;

    void Awake()
    {
        instance = this;
        // Get the root reference location of the database.
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        database = FirebaseDatabase.DefaultInstance;
    }

    private void Start()
    {
        GetProduct();
        GetWeeklyDate();
    }

    public void UploadLevel()
    {
        StartCoroutine(_UploadLevel());
    }
    IEnumerator _UploadLevel()
    {
        string json = JsonUtility.ToJson(Global.level);
       yield return reference.Child($"levels").Child(Global.level.id).SetRawJsonValueAsync(json);
        LoadingScreen.instance.Show(false);
        Controller.instance.uiController.Reset();
    }

    public void UpdateLevel (string key)
    {
        StartCoroutine(_UpdateLevel(key));
    }
    IEnumerator _UpdateLevel(string key)
    {
        string json = JsonUtility.ToJson(Global.level);
        yield return reference.Child("levels/{key}").SetRawJsonValueAsync(json);
        LoadingScreen.instance.Show(false);
        Controller.instance.uiController.Reset();
    }

    public void UploadDate(string date)
    {
        StartCoroutine(_UploadData(date));
    }
    IEnumerator _UploadData(string date)
    {
        LoadingScreen.instance.Show(true);
        DateTime targetDate = DateTime.ParseExact(date + " 11:59:59", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);
        string json = JsonUtility.ToJson(targetDate);

        Debug.Log(json);
        yield return reference.Child($"weeklyDate").SetValueAsync(targetDate.ToString()).ContinueWithOnMainThread(task =>
        {
            LoadingScreen.instance.Show(false);

            if (task.IsFaulted)
            {
                // Handle the error...
                Debug.LogError(task.Exception);
            }
            else if (task.IsCompleted)
            {
                dateText.text = "Current Date: " + targetDate.ToString();
            }
        });
    }

    public void GetWeeklyDate()
    {

        StartCoroutine(_GetWeeklyDate());

    }
    IEnumerator _GetWeeklyDate()
    {
       yield return database.GetReference($"weeklyDate")
        .GetValueAsync().ContinueWithOnMainThread(task => {
        if (task.IsFaulted)
        {
            // Handle the error...
            Debug.LogError(task.Exception);

        }
        else if (task.IsCompleted)
        {

            DataSnapshot snapshot = task.Result;
            //Controller.self.loginController.CompleteLogin();
            Debug.LogError("Good to go" + snapshot.ChildrenCount);

            foreach (var data in snapshot.Children)
            {
                Debug.Log(data.GetRawJsonValue());
            }

            if (snapshot.ChildrenCount == 0)
                dateText.text = "Current Date: Not Set Yet";

        }

    });
    }
    public void Delete(string key,string Url)
    {
        StartCoroutine(_Delete(key,Url));
    }
    IEnumerator _Delete(string key, string U)
    {
       yield return reference.Child($"levels/{key}").RemoveValueAsync();
       StorageController.Instance.Delete(U);
    }
    public void GetProduct()
    {
        StartCoroutine(_GetProduct());
      
    }
    IEnumerator _GetProduct()
    {
        //if (!Global.loggedIn) return;

     yield return database.GetReference($"levels")
      .GetValueAsync().ContinueWithOnMainThread(task => {
          if (task.IsFaulted)
          {
              // Handle the error...
              Debug.LogError(task.Exception);

          }
          else if (task.IsCompleted)
          {
              Debug.Log("Good to go");

              DataSnapshot snapshot = task.Result;
              //Controller.self.loginController.CompleteLogin();

              Controller.instance.uiController.levelHolder.gameObject.KillAllChild();
              foreach (var data in snapshot.Children)
              {
                  LevelInfo level = JsonUtility.FromJson<LevelInfo>(data.GetRawJsonValue());
                  Controller.instance.uiController.InstantiateLevel(level);
                  //Products.instance.AddToAllProduct(product);
                  levelInfo.Add(level);
                  Debug.Log(data.GetRawJsonValue());
              }

              if (snapshot.ChildrenCount == 0)
                  noLevel.gameObject.SetActive(true);
              else
                  noLevel.gameObject.SetActive(false);
          }
      });
    }
}

[Serializable]
public class LevelInfo
{
    public string id;
    public string question;
    public string answer;
    public string imageURL;

    public LevelInfo()
    {
        
    }
}
