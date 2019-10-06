﻿using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRunner : Singleton<GameRunner>
{
    public static List<string> mapNames = Maps.mapNames;
    public static int iterator = 0;
    public static int initialScore = 500;
    public static int scoreDecrementAmount = 10;
    public static bool isCountingScore = true;
    public MenuDisplayer MenuDisplayer;
    public static int waitTime = 3;
    public static int countDown;

    public void LoadNextLevel()
    {
        isCountingScore = false;
        if (iterator == mapNames.Count)
        {
            Debug.Log("The End!");
            DOTween.Sequence()
                .SetUpdate(true)
                .AppendCallback((() => Time.timeScale = 0.0f))
                .Append(Player.Instance._darkness.DOFade(1.0f, 0.6f))
                .AppendInterval(1.0f)
                .AppendCallback((() => Time.timeScale = 1.0f))
                .AppendCallback(() => SceneManager.LoadScene("HighscoreInputScene"));
            return;
        }

        Debug.Log("GameRunner: LoadNextLevel...");

        DOTweenSequnceBetweenLevels(() =>
        {
            LevelChange();
            isCountingScore = true;
            StartCoroutine(LevelStartWaitTime());
            StartCoroutine(CountDown());
        });
    }

    void Start()
    {
        Debug.Log("Gamerunner Started");
        isCountingScore = false;
        LevelChange();
        StartCoroutine(DecrementScore());
        StartCoroutine(LevelStartWaitTime());
        StartCoroutine(CountDown());
    }

    private static void LevelChange()
    {
        WaitTimeCamera.Instance.SetCameraPriority(-1);
        countDown = waitTime;
        MapLoader.Instance.currentMap = mapNames[iterator++];
        MapLoader.Instance.DestroyTileMap();
        MapLoader.Instance.LoadNextLevel();
        Score.Instance.IncrementScore(initialScore);
        LogLevelLoaded();
    }

    private static void PauseActions()
    {
        WaitTimeCamera.Instance.SetCameraPriority(110);
        Player.Instance.isWaiting = true;
        isCountingScore = false;
    }

    private static void UnpauseActions()
    {
        WaitTimeCamera.Instance.SetCameraPriority(-1);
        Player.Instance.isWaiting = false;
        isCountingScore = true;
    }

    private static IEnumerator LevelStartWaitTime()
    {
        PauseActions();
        yield return new WaitForSeconds(waitTime);
        UnpauseActions();
    }

    private static IEnumerator CountDown()
    {
        var ii = countDown;
        for (int i = 0; i < ii; i++)
        {
            yield return new WaitForSeconds(1);
            countDown--;
        }
    }

    private static IEnumerator DecrementScore(int decrement = 10)
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (isCountingScore)
            {
                Score.Instance.DecrementScore(decrement);
            }
        }
    }

    private static void LogLevelLoaded()
    {
        Debug.Log("Current Map: " + MapLoader.Instance.currentMap);
        Debug.Log("GameRunner: LoadNextLevel - Loaded!");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MenuDisplayer.SetVisible();
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            LoadNextLevel();
        }
    }

    public void DOTweenSequnceBetweenLevels(TweenCallback callBackFunction)
    {
        DOTween.Sequence()
            .SetUpdate(true)
            .AppendCallback((() => Time.timeScale = 0.0f))
            .Append(Player.Instance._darkness.DOFade(1.0f, 0.2f))
            .AppendInterval(0.1f)
            .AppendCallback(callBackFunction)
            .AppendCallback((() => Time.timeScale = 1.0f))
            .AppendInterval(0.2f)
            .Append(Player.Instance._darkness.DOFade(0.0f, 0.4f))
            .AppendInterval(0.2f)
            .Play();
    }

    public void PlayerOutOfBoundsReset()
    {
        DOTweenSequnceBetweenLevels(() =>
        {
            Player.Instance.transform.position = new Vector3(MapLoader.Instance.playerStartPosX, MapLoader.Instance.playerStartPosY, 0);
            Player.Instance.Velocity = Vector3.zero;
            Player.Instance.Velocity.x = Player.Instance._moveSpeed;
            Score.Instance.DecrementScore(25);
            countDown = waitTime;
            StartCoroutine(LevelStartWaitTime());
            StartCoroutine(CountDown());
        });
    }
}