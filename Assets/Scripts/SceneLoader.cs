﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : Singleton<SceneLoader> {

    // 일단 씬은 한 번에 하나만 로드된다고 가정한다.
    // 매니저 씬 제외.
    string currentLoadedScene;

    private void Start()
    {

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (!SceneManager.GetSceneAt(i).Equals(gameObject.scene))
            {
                currentLoadedScene = SceneManager.GetSceneAt(i).name;
            }
        }
    }

    public void LoadScene(string name)
    {
        CleanUp();
        currentLoadedScene = name;
        SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
    }

	public void ReloadScene()
    {
        LoadScene(currentLoadedScene);
    }

    void CleanUp()
    {
        var chs = PlayerController.groupCenter.characters;
        foreach (var ch in chs)
        {
            ch.transform.position = PlayerController.groupCenter.transform.position;
        }
        SceneManager.UnloadSceneAsync(currentLoadedScene);
    }
}
