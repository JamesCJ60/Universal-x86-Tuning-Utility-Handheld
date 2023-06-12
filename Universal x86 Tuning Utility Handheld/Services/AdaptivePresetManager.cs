using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universal_x86_Tuning_Utility_Handheld.Services
{
    public class AdaptivePreset
    {
        public bool _isTemp { get; set; }
        public int tempLimit { get; set; }
        public bool _isPower { get; set; }
        public int powerLimit { get; set; }
        public bool _isUndervolt { get; set; }
        public int underVolt { get; set; }
        public bool _isMaxClock { get; set; }
        public int maxClock { get; set; }
        public bool _isIGPUClock { get; set; }
        public int iGPUClock { get; set; }
        public bool _isEPP { get; set; }
        public int _EPP { get; set; }
        public bool _isRSR { get; set; }
        public int _RSR { get; set; }
    }

    public class AdaptivePresetManager
    {
        private string _filePath;
        private Dictionary<string, AdaptivePreset> _presets;

        public AdaptivePresetManager(string filePath)
        {
            _filePath = filePath;
            _presets = new Dictionary<string, AdaptivePreset>();
            LoadPresets();
        }

        public IEnumerable<string> GetPresetNames()
        {
            return _presets.Keys;
        }

        public AdaptivePreset GetPreset(string presetName)
        {
            if (_presets.ContainsKey(presetName))
            {
                return _presets[presetName];
            }
            else
            {
                return null;
            }
        }

        public void SavePreset(string name, AdaptivePreset preset)
        {
            _presets[name] = preset;
            SavePresets();
        }

        public void DeletePreset(string name)
        {
            _presets.Remove(name);
            SavePresets();
        }

        private void LoadPresets()
        {
            if (File.Exists(_filePath))
            {
                string json = File.ReadAllText(_filePath);
                _presets = JsonConvert.DeserializeObject<Dictionary<string, AdaptivePreset>>(json);
            }
            else
            {
                _presets = new Dictionary<string, AdaptivePreset>();
            }
        }


        private void SavePresets()
        {
            string json = JsonConvert.SerializeObject(_presets, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }
    }
}

