﻿using System;
using System.Collections;
using DefaultNamespace;
using TMPro;
using UnityEngine;

public class TextWriter : MonoBehaviour, ISceneLoad
{
    [SerializeField, Range(0, 10)] private float timeBetweenWord;
    [SerializeField] private TextMeshProUGUI textContainer;

    [SerializeField] private TextInGame textScriptable;
    [SerializeField] private GameObject master;
    private Coroutine _cWriting = null;

    public static TextWriter SingleInstance;

    private void Awake()
    {
        OnSceneLoadEvent.AddNotifier(this);
        if (SingleInstance == null)
        {
            SingleInstance = this;
        }
    }


    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        if (_cWriting == null)
        {
            master.SetActive(false);
        }
        else
        {
            StopCoroutine(_cWriting);
            _cWriting = null;
            textContainer.text = textScriptable.text;
        }
    }


    IEnumerator WriteText()
    {
        textContainer.text = "";

        var counter = 0;
        while (counter < textScriptable.text.Length)
        {
            textContainer.text = String.Concat(textContainer.text, textScriptable.text[counter]);
            yield return new WaitForSecondsRealtime(timeBetweenWord);
            counter++;
        }

        textContainer.text = textScriptable.text;

        _cWriting = null;
    }

    private void OnEnable()
    {
        _cWriting = StartCoroutine(WriteText());
        Time.timeScale = 0;
    }


    private void OnDisable()
    {
        Time.timeScale = 1;
    }

    public void NotifySceneLoad()
    {
        master.SetActive(false);
        StopAllCoroutines();
        _cWriting = null;
    }
}