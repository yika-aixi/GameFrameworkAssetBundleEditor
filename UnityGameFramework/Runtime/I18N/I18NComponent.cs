using Icarus.GameFramework.I18N;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Icarus.UnityGameFramework.Runtime.I18N;
using UnityEngine;

namespace Icarus.UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/I18N")]
    public partial class I18NComponent : MonoBehaviour
    {
        private I18NManager _manager = new I18NManager();
        public I18NManager I18NManager => _manager;
        [SerializeField]
        private string _defaultLanguage;
        public string DefaultLanguage => _defaultLanguage;
        /// <summary>
        /// 查找本地,默认为查找
        /// </summary>
        public bool IsFindLocal { get; set; } = true;

        /// <summary>
        /// 查找目录的名字
        /// </summary>
        public string DirectoryName { get; set; } = "I18N";

        /// <summary>
        /// 文件后缀
        /// </summary>
        public string SuffixName { get; set; } = ConstTable.SuffixName;

        /// <summary>
        /// 嵌入式语言表
        /// </summary>
        [SerializeField]
        public Dictionary<string, Dictionary<string, string>> ConvenienceLanguageTable
        {
            get;
            set;
        } = new Dictionary<string, Dictionary<string, string>>();

        void Awake()
        {
            GameEntry.RegisterComponent(this);
            if (string.IsNullOrWhiteSpace(DefaultLanguage))
            {
                I18NManager.SetCurrentLanguage(DefaultLanguage);
            }
        }

        void Start()
        {
            foreach (var pair in ConvenienceLanguageTable)
            {
                I18NManager.AddLanguageTable(pair.Key, pair.Value);
            }

            if (IsFindLocal)
            {
                //todo persistentDataPath 目录下的某目录查找
                var path = GameFramework.Utility.Path.GetCombinePath(Application.persistentDataPath, DirectoryName);
                var files = Directory.GetFiles(path, $"*.{SuffixName}", SearchOption.AllDirectories);
                if (files.Length == 0)
                {
                    return;
                }

                _readFiles(files);

            }
        }

        private void _readFiles(string[] files)
        {
            foreach (var path in files)
            {
                var table = File.ReadAllLines(path);
                Dictionary<string, Dictionary<string, string>> tempTable = new Dictionary<string, Dictionary<string, string>>();
                tempTable.Add(table[0], new Dictionary<string, string>());

                for (var i = 1; i < table.Length; i++)
                {
                    var csv = table[i].Split('\t');
                    tempTable[table[0]].Add(csv[0], csv[1]);
                }
            }
        }


        public void AddLanguageChangeEvent(EventHandler<LanguageChangeEventArgs> handle)
        {
            _manager.LanguageChange += handle;
        }

        public void RemoveLanguageChangeEvent(EventHandler<LanguageChangeEventArgs> handle)
        {
            _manager.LanguageChange -= handle;
        }

        public void SetCurrentLanguage(string language)
        {
            _manager.SetCurrentLanguage(language);
        }

        public string GetValue(string key)
        {
            return _manager.GetValue(key);
        }

        public IEnumerable<string> GetLanguges()
        {
            var names = _manager.GetLanguges();
            var keys = ConvenienceLanguageTable.Keys;
            return names.Union(keys);
        }
        public string CurrentLanguage => _manager.CurrentLanguage;
    }

    [Serializable]
    public class LanguageSerializationEntity
    {
        [SerializeField]
        private string _languageName;
        [SerializeField]
        private string _key;
        [SerializeField]
        private string _value;

        public string Value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
            }
        }

        public string LanguageName
        {
            get
            {
                return _languageName;
            }

            set
            {
                _languageName = value;
            }
        }

        public string Key
        {
            get
            {
                return _key;
            }

            set
            {
                _key = value;
            }
        }
    }

    public partial class I18NComponent : ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<string> _languageNames = new List<string>();
        [SerializeField]
        private List<string> _keys = new List<string>();
        [SerializeField]
        private List<string> _values = new List<string>();
//        [SerializeField]
//        private List<LanguageSerializationEntity> _entity = new List<LanguageSerializationEntity>();
        
        public void OnBeforeSerialize()
        {
            _languageNames.Clear();
//            _entity.Clear();
            _keys.Clear();
            _values.Clear();
            _languageNames.AddRange(ConvenienceLanguageTable.Keys);
            bool initKeys = false;
            foreach (var pair in ConvenienceLanguageTable)
            {
                if (!initKeys)
                {
                    _keys.AddRange(pair.Value.Keys);
                    initKeys = true;
                }
                foreach (var valuePair in pair.Value)
                {
                    _values.Add(valuePair.Value);
                }
            }
        }

        public void OnAfterDeserialize()
        {
            ConvenienceLanguageTable = new Dictionary<string, Dictionary<string, string>>();
            int i = 0;
            foreach (var languageName in _languageNames)
            {
                ConvenienceLanguageTable.Add(languageName, new Dictionary<string, string>());
                foreach (var key in _keys)
                {
                    if (_values.Count <= i)
                    {
                        _values.Add(String.Empty);
                    }
                    ConvenienceLanguageTable[languageName].Add(key, _values[i]);
                    i++;
                }
            }
        }

        int _getValueIndex(int keyIndex)
        {
            return keyIndex * _keys.Count / _keys.Count;
        }
    }
}