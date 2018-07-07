using System;
using System.Collections.Generic;

namespace Icarus.GameFramework.I18N
{
    public class I18NManager : II18N
    {
        private static readonly Dictionary<string, LanguageEntity> LanguageTables = new Dictionary<string, LanguageEntity>();
        private EventHandler<LanguageChangeEventArgs> _languageChange;
        public event EventHandler<LanguageChangeEventArgs> LanguageChange
        {
            add
            {
                _languageChange += value;
            }

            remove
            {
                _languageChange -= value;
            }
        }

        public void SetCurrentLanguage(string language)
        {
            CurrentLanguage = language;
            var args = new LanguageChangeEventArgs {Lanaguage = language};
            _languageChange(this, args);
        }

        ///<inheritdoc cref="II18N"/>
        public void AddLanguageTable(string language, Dictionary<string, string> table)
        {
            if (LanguageTables.ContainsKey(language))
            {
                LanguageTables[language].AddAndUpdateLanguageTable(table);
            }
            else
            {
                LanguageTables.Add(language, new LanguageEntity(table));
            }

        }
        ///<inheritdoc cref="II18N"/>
        /// <summary>
        /// 如果对应的key返回  string.Empty
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(string key)
        {
            if (!LanguageTables.ContainsKey(CurrentLanguage))
            {
                return string.Empty;
            }

            return LanguageTables[CurrentLanguage].GetValue(key);
        }

        public IEnumerable<string> GetLanguges()
        {
            return LanguageTables.Keys;
        }

        public string CurrentLanguage { get; private set; }
    }
}