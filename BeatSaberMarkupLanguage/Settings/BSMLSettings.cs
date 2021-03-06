﻿using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using IPA.Utilities;
using Polyglot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;

namespace BeatSaberMarkupLanguage.Settings
{
    public class BSMLSettings : MonoBehaviour
    {
        private bool isInitialized;
        private Button button;
        private static BSMLSettings _instance = null;

        private ModSettingsFlowCoordinator flowCoordinator;

        public List<CustomCellInfo> settingsMenus = new List<CustomCellInfo>();

        [UIComponent("language-dropdown")]
        protected DropDownListSetting dropdown;

        [UIValue("languages")]
        public List<object> languageOptions = new List<object>();

        [UIValue("selected-language")]
        public string SelectedLanguage
        {
            get => Plugin.config.SelectedLanguage.ToString();
            set
            {
                Language lng = (Language)Enum.Parse(typeof(Language), value);
                Plugin.config.SelectedLanguage = lng;
                Localization.Instance.SelectedLanguage = lng;
            }
        }

        public static BSMLSettings instance
        {
            get
            {
                if (!_instance)
                    _instance = new GameObject("BSMLSettings").AddComponent<BSMLSettings>();

                return _instance;
            }
            private set => _instance = value;
        }

        internal void Setup()
        {
            StopAllCoroutines();
            if(button == null)
                StartCoroutine(AddButtonToMainScreen());
            foreach (SettingsMenu settingsMenu in settingsMenus)
            {
                settingsMenu.Setup();
            }
            SetupLanguageList();
            isInitialized = true;
        }

        private void Awake() => DontDestroyOnLoad(this.gameObject);

        private void Start()
        {
            //Localization.Instance.Localize.AddListener(SetupLanguageList);
        }

        private void SetupLanguageList()
        {
            languageOptions.Clear();
            languageOptions.AddRange(Localization.Instance.SupportedLanguages.Select(x => x.ToString()));
            dropdown?.UpdateChoices();
            Localization.Instance.SelectedLanguage = Plugin.config.SelectedLanguage;
        }

        private void OnDestroy()
        {
            //Localization.Instance.Localize.RemoveListener(SetupLanguageList);
        }
        
        public void AddSettingsMenu(string name, string resource, object host)
        {
            if (settingsMenus.Any(x => x.text == name))
                return;

            if (settingsMenus.Count == 0)
            {
                SetupLanguageList();
                settingsMenus.Add(new SettingsMenu("About", "BeatSaberMarkupLanguage.Views.settings-about.bsml", this, Assembly.GetExecutingAssembly()));
            }
            SettingsMenu settingsMenu = new SettingsMenu(name, resource, host, Assembly.GetCallingAssembly());
            settingsMenus.Add(settingsMenu);
            if(isInitialized)
                settingsMenu.Setup();
            button?.gameObject.SetActive(true);
        }

        public void RemoveSettingsMenu(object host)
        {
            IEnumerable<CustomCellInfo> menu = settingsMenus.Where(x => (x as SettingsMenu).host == host);
            if (menu.Count() > 0)
                settingsMenus.Remove(menu.FirstOrDefault());
        }

        private IEnumerator AddButtonToMainScreen()
        {
            OptionsViewController optionsViewController = null;
            while (optionsViewController == null)
            {
                optionsViewController = Resources.FindObjectsOfTypeAll<OptionsViewController>().FirstOrDefault();
                yield return new WaitForFixedUpdate();
            }
            button = Instantiate(optionsViewController.GetField<Button, OptionsViewController>("_settingsButton"), optionsViewController.transform.Find("Wrapper"));
            button.GetComponentInChildren<LocalizedTextMeshProUGUI>().Key = "Mod Settings";
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(PresentSettings);

            if (settingsMenus.Count == 0)
                button.gameObject.SetActive(false);
        }

        private void PresentSettings()
        {
            if (flowCoordinator == null)
                flowCoordinator = BeatSaberUI.CreateFlowCoordinator<ModSettingsFlowCoordinator>();
            flowCoordinator.isAnimating = true;
            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(flowCoordinator, new Action(delegate{
                flowCoordinator.ShowInitial();
                flowCoordinator.isAnimating = false;
            }));
        }

    }
}
