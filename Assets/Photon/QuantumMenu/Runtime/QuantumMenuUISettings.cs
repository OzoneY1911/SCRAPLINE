namespace Quantum.Menu {
  using System;
  using System.Collections.Generic;
#if QUANTUM_ENABLE_TEXTMESHPRO
  using Dropdown = TMPro.TMP_Dropdown;
  using InputField = TMPro.TMP_InputField;
  using Text = TMPro.TMP_Text;
#else
  using Dropdown = UnityEngine.UI.Dropdown;
  using InputField = UnityEngine.UI.InputField;
  using Text = UnityEngine.UI.Text;
#endif
  using UnityEngine;
  using UnityEngine.UI;

  public partial class QuantumMenuUISettings : QuantumMenuUIScreen
    {
        
        [Header("General Settings")]
        [SerializeField] protected UnityEngine.UI.Button _backButton;

        [Header("App Settings")]
        [SerializeField] protected Dropdown _uiAppVersion;
        [SerializeField] protected GameObject _goAppVersion;
        [SerializeField] protected Dropdown _uiRegion;
        [SerializeField] protected GameObject _goRegion;
        [SerializeField] protected InputField _uiMaxPlayers;
        [SerializeField] protected Text _sdkLabel;

        [Header("Video Settings")]
        [SerializeField] protected Toggle _uiFullscreen;
        [SerializeField] protected GameObject _goFullscreenn;
        [SerializeField] protected Dropdown _uiFramerate;
        [SerializeField] protected Dropdown _uiGraphicsQuality;
        [SerializeField] protected Dropdown _uiResolution;
        [SerializeField] protected GameObject _goResolution;
        [SerializeField] protected Toggle _uiVSyncCount;

        [Header("Audio Settings")]
        [SerializeField] protected Slider _uiMasterVolume;
        [SerializeField] protected Slider _uiMusicVolume;
        [SerializeField] protected Slider _uiSFXVolume;
        [SerializeField] protected Slider _uiVoiceVolume;

        protected QuantumMenuSettingsEntry<string> _entryRegion;
        protected QuantumMenuSettingsEntry<string> _entryAppVersion;
        protected QuantumMenuSettingsEntry<int> _entryFramerate;
        protected QuantumMenuSettingsEntry<int> _entryResolution;
        protected QuantumMenuSettingsEntry<int> _entryGraphicsQuality;
        protected QuantumMenuGraphicsSettings _graphicsSettings;
        protected List<string> _appVersions;

        partial void AwakeUser();
        partial void InitUser();
        partial void ShowUser();
        partial void HideUser();
        partial void SaveChangesUser();

        public override void Awake() {
            base.Awake();

            _appVersions = new List<string>();
            if (Config.MachineId != null) {
            _appVersions.Add(Config.MachineId);
            }
            _appVersions.AddRange(Config.AvailableAppVersions);

            _entryRegion = new QuantumMenuSettingsEntry<string>(_uiRegion, SaveChanges);
            _entryAppVersion = new QuantumMenuSettingsEntry<string>(_uiAppVersion, SaveChanges);
            _entryFramerate = new QuantumMenuSettingsEntry<int>(_uiFramerate, SaveChanges);
            _entryResolution = new QuantumMenuSettingsEntry<int>(_uiResolution, SaveChanges);
            _entryGraphicsQuality = new QuantumMenuSettingsEntry<int>(_uiGraphicsQuality, SaveChanges);

            _uiMaxPlayers.onEndEdit.AddListener(s => {
            if (Int32.TryParse(s, out var maxPlayers) == false || maxPlayers <= 0 || maxPlayers > Config.MaxPlayerCount) {
                maxPlayers = Math.Clamp(maxPlayers, 1, Config.MaxPlayerCount);
                _uiMaxPlayers.text = maxPlayers.ToString();
            }
            SaveChanges();
            });


            _uiVSyncCount.onValueChanged.AddListener(_ => SaveChanges());
            _uiFullscreen.onValueChanged.AddListener(_ => SaveChanges());

            _graphicsSettings = new QuantumMenuGraphicsSettings();

            _goAppVersion.SetActive(Config.AvailableAppVersions.Count > 0);
            _goRegion.SetActive(Config.AvailableRegions.Count > 0);

#if UNITY_IOS || UNITY_ANDROID
            _goResolution.SetActive(false);
            _goFullscreenn.SetActive(false);
#endif

            _uiMasterVolume.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
            _uiMusicVolume.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
            _uiSFXVolume.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
            _uiVoiceVolume.onValueChanged.AddListener(AudioManager.Instance.SetVoiceVolume);

            AwakeUser();
        }

        public override void Init() {
            base.Init();
            InitUser();
        }

        public override void Show() {
            base.Show();

            _entryRegion.SetOptions(Config.AvailableRegions, ConnectionArgs.PreferredRegion, s => string.IsNullOrEmpty(s) ? "Best" : s);
            _entryAppVersion.SetOptions(_appVersions, ConnectionArgs.AppVersion, s => s.Equals(Config.MachineId) ? $"Build ({Config.MachineId})" : s);
            _entryFramerate.SetOptions(_graphicsSettings.CreateFramerateOptions, _graphicsSettings.Framerate, s => (s == -1 ? "Platform Default" : s.ToString()));
            _entryResolution.SetOptions(_graphicsSettings.CreateResolutionOptions, _graphicsSettings.Resolution, s =>
    #if UNITY_2022_2_OR_NEWER
            $"{Screen.resolutions[s].width} x {Screen.resolutions[s].height} @ {Mathf.RoundToInt((float)Screen.resolutions[s].refreshRateRatio.value)}");
    #else
            Screen.resolutions[s].ToString());
    #endif
            _entryGraphicsQuality.SetOptions(_graphicsSettings.CreateGraphicsQualityOptions, _graphicsSettings.QualityLevel, s => QualitySettings.names[s]);
            _uiMaxPlayers.SetTextWithoutNotify(Math.Clamp(ConnectionArgs.MaxPlayerCount, 1, Config.MaxPlayerCount).ToString());
            _uiFullscreen.isOn = _graphicsSettings.Fullscreen;
            _uiVSyncCount.isOn = _graphicsSettings.VSync;
            
            _uiMasterVolume.SetValueWithoutNotify(AudioManager.Instance.GetMasterVolume());
            _uiMusicVolume.SetValueWithoutNotify(AudioManager.Instance.GetMusicVolume());
            _uiSFXVolume.SetValueWithoutNotify(AudioManager.Instance.GetSFXVolume());
            _uiVoiceVolume.SetValueWithoutNotify(AudioManager.Instance.GetVoiceVolume());

            ShowUser();
        }

        public override void Hide() {
            base.Hide();
            HideUser();
        }

        /// <summary>
        /// Saving changes callbacks are registered to all ui elements during <see cref="Show()"/>.
        /// If defined the partial SaveChangesUser() is also called in the end.
        /// </summary>
        protected virtual void SaveChanges() {
            if (IsShowing == false) {
            // Screen not enabled, yet. Bail here to work around race conditions with triggering UI fields
            return;
            }

            if (Int32.TryParse(_uiMaxPlayers.text, out var maxPlayers)) {
            ConnectionArgs.MaxPlayerCount = Math.Clamp(maxPlayers, 1, Config.MaxPlayerCount);
            _uiMaxPlayers.SetTextWithoutNotify(ConnectionArgs.MaxPlayerCount.ToString());
            }

            ConnectionArgs.PreferredRegion = _entryRegion.Value;
            ConnectionArgs.AppVersion = _entryAppVersion.Value;

            _graphicsSettings.Fullscreen = _uiFullscreen.isOn;
            _graphicsSettings.Framerate = _entryFramerate.Value;
            _graphicsSettings.Resolution = _entryResolution.Value;
            _graphicsSettings.QualityLevel = _entryGraphicsQuality.Value;
            _graphicsSettings.VSync = _uiVSyncCount.isOn;
            _graphicsSettings.Apply();

            SaveChangesUser();

            ConnectionArgs.SaveToPlayerPrefs();
        }

        protected virtual void OnBackButtonPressed()
        {
            if (Controller.Get<QuantumMenuUIGameplay>().IsShowing)
            {
                Hide();
            }
            else
            {
                Controller.Show<QuantumMenuUIMain>();
            }
        }
    }
}
