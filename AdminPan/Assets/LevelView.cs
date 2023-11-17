using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelView : MonoBehaviour
{
    public TMP_Text levelName;
    public TMP_Text levelQuestion;
    public TMP_Text levelAnswer;
    public Image levelImage;

    public LevelInfo level;

    public void SetUp(LevelInfo level)
    {
        this.level = level;
        levelQuestion.text = $"שאלה: {level.question}";
        levelAnswer.text = $"שאלה: {level.answer}";

        Controller.instance.storageController.GetDownloadURL(level.imageURL, url =>
        {
            Debug.LogWarning(url);
           StartCoroutine(downloadImage(url));
            levelName.text = $"שלב - {returns((transform.GetSiblingIndex()+1).ToString())}";
        });
    }
    public string returns(string number)
    {
        string d = "";
        for (int i = number.Length-1; i >= 0; i--)
        {
            d += number[i];
        }
        return d;
    }
    IEnumerator downloadImage(string url)
    {
        WWW w = new WWW(url);
        yield return w;
        Texture2D tex = new Texture2D(1, 1);
        w.LoadImageIntoTexture(tex);
        tex.Apply();
        Sprite spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),new Vector2(0.5f,0.5f));
        levelImage.sprite = spr;
    }

    public void UpdateLevel()
    {
        Controller.instance.uiController.SetEdit(level, levelImage.sprite);
    }

    public void Delete()
    {
        Controller.instance.databaseController.Delete(level.id,level.imageURL);
        Destroy(gameObject);
    }
}
