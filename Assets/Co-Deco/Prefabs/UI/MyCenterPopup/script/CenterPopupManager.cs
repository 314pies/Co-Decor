using Michsky.UI.ModernUIPack;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using TMPro;


namespace CoDeco.UI
{
    public class CenterPopupManager : MonoBehaviour
    {

        public static CenterPopupManager Singleton { get; private set; } = null;
        // Start is called before the first frame update
        void Start()
        {
            if (Singleton != null)
            {               
                return;
            }
            Singleton = this;
            manimation = gameObject.GetComponent<Animation>();
        }

        [Header("UI Components")]
        public Image windowIcon;
        public TextMeshProUGUI windowTitle;
        public TextMeshProUGUI windowDescription;
        public Button confirmButton;
        public Button cancelButton;
        public GameObject LoadingEffect;
        Animation manimation;


        [Header("Debug")]
        public bool isOn = false;

        [ShowInInspector]
        public void OpenWindow()
        {
            if (isOn == false)
            {
                manimation.Play("Fade-in");
                isOn = true;
            }
        }

        [ShowInInspector]
        public void CloseWindow()
        {
            if (isOn == true)
            {
                manimation.Play("Fade-out");
                isOn = false;
            }
        }

        [Button]
        public void PopupLoading(string title, string description)
        {
            windowTitle.text = title;
            windowDescription.text = description;
            OpenWindow();
            confirmButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);
            LoadingEffect.SetActive(true);
        }




        [ShowInInspector]
        public void PopupInfo(string title, string description, Action onClickEvent = null)
        {
            windowTitle.text = title;
            windowDescription.text = description;

            UnityAction _action = () =>
            {
                confirmButton.onClick.RemoveAllListeners();
                try
                {
                    CloseWindow();
                    if (onClickEvent != null)
                        onClickEvent();
                }
                catch (Exception exp)
                {
                    Debug.Log("CenterPopupManager.PopupInfo(), Error with custon onClickEvent: " + exp);
                }
            };

            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(_action);
            confirmButton.gameObject.SetActive(true);
            cancelButton.gameObject.SetActive(false);
            LoadingEffect.SetActive(false);
            OpenWindow();
        }

        [ShowInInspector]
        public void PopupInfoWithTwoButtom(string title, string description, Action onConfirmEvent = null, Action onCancelEvent = null)
        {
            windowTitle.text = title;
            windowDescription.text = description;

            UnityAction _confirmAction = () =>
            {
                confirmButton.onClick.RemoveAllListeners();
                try
                {
                    CloseWindow();
                    if (onConfirmEvent != null) {onConfirmEvent();}
                }
                catch (Exception exp)
                {
                    Debug.Log("CenterPopupManager.PopupInfoWithTwoButtom(), Error with custon onConfirmEvent: " + exp);
                }
            };

            UnityAction _cancelAction = () =>
            {
                cancelButton.onClick.RemoveAllListeners();
                try
                {
                    CloseWindow();
                    if (onCancelEvent != null) { onCancelEvent(); }
                }
                catch (Exception exp)
                {
                    Debug.Log("CenterPopupManager.PopupInfoWithTwoButtom(), Error with custon onCancelEvent: " + exp);
                }
            };

            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(_confirmAction);
            confirmButton.gameObject.SetActive(true);
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(_cancelAction);
            cancelButton.gameObject.SetActive(true);
            LoadingEffect.SetActive(false);
            OpenWindow();
        }
    }
}