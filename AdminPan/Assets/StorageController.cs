using Firebase.Extensions;
using Firebase.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class StorageController : MonoBehaviour
{
    public static StorageController Instance;
    FirebaseStorage storage;
    StorageReference storageRef;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        storage = FirebaseStorage.DefaultInstance;
        storageRef = storage.RootReference;
    }



    public void GetDownloadURL(string bucket, Action<string> data)
    {
        string url = "";
        StorageReference pathReference = storage.GetReference(bucket);
         pathReference.GetDownloadUrlAsync().ContinueWithOnMainThread(task => {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                url = task.Result.ToString();

                Debug.LogWarning(url);
                data.Invoke(url);
            }
        });
    }


    public void UploadImage(string uploadPath, string localPath)
    {
        StartCoroutine(_UploadImage(uploadPath,localPath));
    }
    IEnumerator _UploadImage(string uploadPath, string localPath)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            localPath = "file://" + localPath;
        }

        Debug.LogWarning("LocalPath: " + localPath);

        StorageReference riversRef = storageRef.Child($"images/levels/{uploadPath}");

        yield return riversRef.PutFileAsync(localPath).ContinueWithOnMainThread((Task<StorageMetadata> task) => {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log(task.Exception.ToString());
                    // Uh-oh, an error occurred!
                }
                else
                {
                    // Metadata contains file metadata such as size, content-type, and download URL.
                    StorageMetadata metadata = task.Result;
                    string md5Hash = metadata.Md5Hash;
                    Debug.Log("Finished uploading...");
                    Debug.Log("md5 hash = " + md5Hash);
                    Debug.Log("Bucket = " + metadata.Reference);
                    Debug.Log("Bucket = " + metadata.Path);
                    Global.level.imageURL = metadata.Path;

                    if (!Global.isEditing)
                        Controller.instance.databaseController.UploadLevel();
                    else
                        Controller.instance.databaseController.UpdateLevel(Global.selectedLevel.id);

                }
            });
    }

    public void Delete(string bucket)
    {
        StartCoroutine(DeleteImage(bucket));
    }
    IEnumerator DeleteImage(string bucket)
    {
        StorageReference pathReference = storageRef.Child(bucket);
        yield return pathReference.DeleteAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log(task.Exception.ToString());
            }
            else
            {

            }
        });
    }
}
