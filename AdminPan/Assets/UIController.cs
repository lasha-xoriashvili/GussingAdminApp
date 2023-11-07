using SimpleFileBrowser;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public string imagePath;
    public string ext;
    public Image previewImage;
    Sprite mySprite;
    public LevelView levelView;
    public Transform levelHolder;
    public TMP_InputField questionIF;
    public TMP_InputField answerIF;
    public TMP_InputField dateIF;
    public TMP_Text infoText;
    public Color normalColor;
    public GameObject levelAddMenu;
    public GameObject levelSave;
    public GameObject levelUpdate;


    public void InstantiateLevel(LevelInfo level)
    {
        Instantiate(levelView, levelHolder).SetUp(level);
    }

    public void SetEdit(LevelInfo level, Sprite img)
    {
        Global.selectedLevel = level;
        answerIF.text = level.answer;
        questionIF.text = level.question;
        levelAddMenu.SetActive(true);
        levelSave.SetActive(false);
        levelUpdate.SetActive(true);
        previewImage.sprite = img;
    }
    public void LevelAdd()
    {
        levelAddMenu.SetActive(true);
        levelSave.SetActive(true);
        levelUpdate.SetActive(false);
    }

    private void Start()
    {
        FileBrowser.SetFilters(false, new FileBrowser.Filter("Images", ".jpg", ".png"));

    }

    public void UploadDate()
    {
        if (string.IsNullOrEmpty(dateIF.text) || string.IsNullOrWhiteSpace(dateIF.text))
        {
            StartCoroutine(ShowInfo("Please enter the date", 2f));
            return;
        }

        Controller.instance.databaseController.UploadDate(dateIF.text.Trim());
    }

    public void UploadLevel()
    {
        if(string.IsNullOrEmpty(ext) || string.IsNullOrWhiteSpace(ext)||
            string.IsNullOrEmpty(imagePath) || string.IsNullOrWhiteSpace(imagePath))
        {
            StartCoroutine(ShowInfo("Please select an image", 2f));
            return;
        }

        if(string.IsNullOrEmpty(questionIF.text) || string.IsNullOrWhiteSpace(questionIF.text))
        {
            StartCoroutine(ShowInfo("Please enter the question", 2f));
            return;
        }

        if (string.IsNullOrEmpty(answerIF.text) || string.IsNullOrWhiteSpace(answerIF.text))
        {
            StartCoroutine(ShowInfo("Please enter the answer", 2f));
            return;
        }

        LoadingScreen.instance.Show(true);

        Global.level.answer = answerIF.text.Trim();
        Global.level.question = questionIF.text.Trim();
        Global.level.imageURL = imagePath;

        Controller.instance.storageController.UploadImage(ext, imagePath);
    }

    public void UpdateLevel()
    {
        if (!string.IsNullOrEmpty(ext) || !string.IsNullOrWhiteSpace(ext) ||
           !string.IsNullOrEmpty(imagePath) || !string.IsNullOrWhiteSpace(imagePath))
        {

            if (string.IsNullOrEmpty(questionIF.text) || string.IsNullOrWhiteSpace(questionIF.text))
            {
                StartCoroutine(ShowInfo("Please enter the question", 2f));
                return;
            }

            if (string.IsNullOrEmpty(answerIF.text) || string.IsNullOrWhiteSpace(answerIF.text))
            {
                StartCoroutine(ShowInfo("Please enter the answer", 2f));
                return;
            }

            LoadingScreen.instance.Show(true);

            Global.level.answer = answerIF.text.Trim();
            Global.level.question = questionIF.text.Trim();
            Global.level.imageURL = imagePath;
            Global.level.id = Global.selectedLevel.id;
            Controller.instance.storageController.UploadImage(ext, imagePath);
        }
        else
        {
            if (string.IsNullOrEmpty(questionIF.text) || string.IsNullOrWhiteSpace(questionIF.text))
            {
                StartCoroutine(ShowInfo("Please enter the question", 2f));
                return;
            }

            if (string.IsNullOrEmpty(answerIF.text) || string.IsNullOrWhiteSpace(answerIF.text))
            {
                StartCoroutine(ShowInfo("Please enter the answer", 2f));
                return;
            }

            LoadingScreen.instance.Show(true);
            Global.level.answer = answerIF.text.Trim();
            Global.level.question = questionIF.text.Trim();
            Global.level.imageURL = Global.selectedLevel.imageURL;
            Global.level.id = Global.selectedLevel.id;
            Controller.instance.databaseController.UpdateLevel(Global.selectedLevel.id);
        }
    }

    public void PickLevelImage()
    {
        OpenExplorer(1024, previewImage);
    }

    private void PickImage(int maxSize, Image img)
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
            Debug.Log("Image path: " + path);
            if (path != null)
            {
                // Create Texture from selected image
                Texture2D texture = NativeGallery.LoadImageAtPath(path, maxSize);
                if (texture == null)
                {
                    Debug.Log("Couldn't load texture from " + path);
                    return;
                }

                Global.level.id = Controller.instance.databaseController.reference.Push().Key;
                string[] splitedPath = path.Split('/');
                string[] _name = splitedPath[splitedPath.Length - 1].Split('.');
                ext = Global.level.id +"."+ _name[_name.Length-1];



                mySprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                img.sprite = mySprite;
                img.color = Color.white;
                imagePath = path;
            }
        });
        Debug.Log("Permission result: " + permission);
    }

    void OpenExplorer(int maxSize, Image img)
    {
        StartCoroutine(ShowLoadDialogCoroutine(maxSize,img));
    }

    IEnumerator ShowLoadDialogCoroutine(int maxSize, Image img)
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: both, Allow multiple selection: true
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Load File", Submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, true, null, null, "Load Files and Folders", "Load");

        // Dialog is closed
        // Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {
            // Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
            for (int i = 0; i < FileBrowser.Result.Length; i++)
                Debug.Log(FileBrowser.Result[i]);


            Texture2D texture = NativeGallery.LoadImageAtPath(FileBrowser.Result[0], maxSize);
            string path = FileBrowser.Result[0];
            Global.level.id = Controller.instance.databaseController.reference.Push().Key;
            string[] splitedPath = path.Split('/');
            string[] _name = splitedPath[splitedPath.Length - 1].Split('.');
            ext = Global.level.id + "." + _name[_name.Length - 1];



            mySprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            img.sprite = mySprite;
            img.color = Color.white;
            imagePath = path;
            // Read the bytes of the first file via FileBrowserHelpers
            // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
            //byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);

            // Or, copy the first file to persistentDataPath
            //string destinationPath = Path.Combine(Application.persistentDataPath, FileBrowserHelpers.GetFilename(FileBrowser.Result[0]));
            //FileBrowserHelpers.CopyFile(FileBrowser.Result[0], destinationPath);
        }
    }

    IEnumerator ShowInfo(string msg, float dur)
    {
        infoText.gameObject.SetActive(true);
        infoText.text = msg;
        yield return new WaitForSeconds(dur);
        infoText.gameObject.SetActive(false);
    }

    public void Reset()
    {
        previewImage.color = normalColor;
        previewImage.sprite = null;
        answerIF.text = null;
        questionIF.text = null;
        Global.isEditing = false;
        Global.selectedLevel = new LevelInfo();
        Global.level = new LevelInfo();
    }

}
