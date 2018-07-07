using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Icarus.UnityGameFramework.Runtime;
using Icarus.UnityGameFramework.Runtime.I18N;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Icarus.UnityGameFramework.Editor
{
    [CustomEditor(typeof(I18NComponent))]
    public class I18NInspector : GameFrameworkInspector
    {
        private I18NComponent _i18N;
        private SerializedProperty _defaultLanguageSer;
        private SerializedProperty _currenLanguageSer;
        private SerializedProperty _languageNamesSer;
        private Dictionary<string, bool> _showState = new Dictionary<string, bool>();
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            EditorGUILayout.PropertyField(_defaultLanguageSer, new GUIContent("默认语言:"));
            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.PropertyField(_currenLanguageSer, new GUIContent("当前语言:"));
            }

            EditorGUILayout.BeginHorizontal();
            {

            }
            EditorGUILayout.EndHorizontal();

            _createLanguage();

            _addItem();

            _fileLoadAndExport();

            _drowLanguageList();

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }
        private void _addItem()
        {
            if (_languageNamesSer.arraySize > 0)
            {
                //添加数据
                EditorGUILayout.BeginHorizontal();
                {
                    _key = EditorGUILayout.TextField("添加条目：", _key);
                    if (GUILayout.Button("+"))
                    {
                        for (int i = 0; i < _keysSer.arraySize; i++)
                        {
                            var item = _keysSer.GetArrayElementAtIndex(i);
                            if (item.stringValue == _key)
                            {
                                return;
                            }
                        }


                        var result = _add(_keysSer, _key, $"添加Key:{_key},失败！", false);

                        if (!result)
                        {
                            return;
                        }

                        _insertValue();

                        _key = String.Empty;

                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void _insertValue(bool update = true)
        {
            for (int i = 0; i < _languageNamesSer.arraySize; i++)
            {
                var index = _getValueIndex(_keysSer.arraySize - 1, i);
                _valuesSer.InsertArrayElementAtIndex(index);
                _valuesSer.GetArrayElementAtIndex(index).stringValue = String.Empty;
            }

            if (!update) return;

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

        }

        private void _fileLoadAndExport()
        {
            _ex = EditorPrefs.GetString(_exKey);
            if (string.IsNullOrWhiteSpace(_ex))
            {
                _ex = ConstTable.SuffixName;
            }
            EditorPrefs.SetString(_exKey, EditorGUILayout.TextField("文件后缀：", _ex));

            _load();

            if (_languageNamesSer.arraySize > 0)
            {
                _export();
            }

        }

        private string _exKey = "I18NFileEx";
        private string _ex;
        private void _load()
        {
            if (GUILayout.Button("读取", GUILayout.Height(25)))
            {
                var path = _getPath("读取目录");
                if (string.IsNullOrWhiteSpace(path))
                {
                    return;
                }
                var filePaths = Directory.GetFiles(path, $"*.{_ex}", SearchOption.AllDirectories);
                _clearSer();
                foreach (var filePath in filePaths)
                {
                    var fileContent = File.ReadAllLines(filePath);
                    var languageName = fileContent[0];

                    var result = _add(_languageNamesSer, languageName, "读取失败!", false);

                    if (!result)
                    {
                        return;
                    }

                    var languageIndex = _languageNamesSer.arraySize - 1;

                    var keys = fileContent.Skip(1).Select(x => x.Split('\t')[0]).ToList();
                    var values = fileContent.Skip(1).Select(x => x.Split('\t')[1]).ToList();
                    var nextIndex = 0;

                    foreach (var key in keys)
                    {
                        bool isHit = false;
                        int keyIndex = -1;
                        for (int i = 0; i < _keysSer.arraySize; i++)
                        {
                            var item = _keysSer.GetArrayElementAtIndex(i);
                            if (item.stringValue == key)
                            {
                                isHit = true;
                                keyIndex = i;
                                break;
                            }
                        }

                        if (!isHit)
                        {
                            result = _add(_keysSer, key, "读取失败!", false);
                            if (!result)
                            {
                                return;
                            }
                            keyIndex = _keysSer.arraySize - 1;
                            _insertValue(false);
                            var valueIndex = _getValueIndex(keyIndex, languageIndex);

                            _valuesSer.GetArrayElementAtIndex(valueIndex).stringValue = values[nextIndex];
                        }
                        else
                        {
                            result = _add(_valuesSer, values[nextIndex], "读取失败!", false);
                            if (!result)
                            {
                                return;
                            }
                        }

                        nextIndex++;
                    }
                }
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }
        }

        private void _clearSer()
        {
            _languageNamesSer.ClearArray();
            _keysSer.ClearArray();
            _valuesSer.ClearArray();
        }

        private string _pathKey = "saveAndLoadPath";

        private void _export()
        {
            if (GUILayout.Button("导出", GUILayout.Height(25)))
            {
                var path = _getPath("导出目录");
                foreach (var pair in _i18N.ConvenienceLanguageTable)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(pair.Key);
                    foreach (var table in pair.Value)
                    {
                        var line = $"{table.Key}\t{table.Value}\n";
                        sb.Append(line);
                    }

                    sb.Remove(sb.Length - 1, 1);
                    File.WriteAllText(Path.Combine(path, $"{pair.Key}.{_ex}"), sb.ToString());
                }

            }

        }

        string _getPath(string title)
        {
            var path = EditorUtility.OpenFolderPanel(title, EditorPrefs.GetString(_pathKey), "");
            if (!string.IsNullOrWhiteSpace(path))
            {
                EditorPrefs.SetString(_pathKey, path);
            }
            return path;
        }

        private void _drowLanguageList()
        {
            for (var i = 0; i < _languageNamesSer.arraySize; i++)
            {
                var languageName = _languageNamesSer.GetArrayElementAtIndex(i).stringValue;
                if (!_showState.ContainsKey(languageName))
                {
                    _showState.Add(languageName, false);
                }

                EditorGUILayout.BeginHorizontal();
                {
                    _showState[languageName] = EditorGUILayout.Foldout(_showState[languageName], languageName, true);
                    if (GUILayout.Button("删除"))
                    {
                        //                        _languageNames.RemoveAt(i);
                        _showState.Remove(languageName);

                        _languageNamesSer.DeleteArrayElementAtIndex(i);

                        serializedObject.ApplyModifiedProperties();
                        serializedObject.Update();
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (!_showState.ContainsKey(languageName) || !_showState[languageName]) continue;
                _drowLanguageTable(i);
            }
        }

        private string _key;
        private void _drowLanguageTable(int languageIndex)
        {
            EditorGUI.indentLevel += 1;
            {
                for (int i = 0; i < _keysSer.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        var key = _keysSer.GetArrayElementAtIndex(i);
                        var index = _getValueIndex(i, languageIndex);
                        var value = _valuesSer.GetArrayElementAtIndex(index);
                        var keyRect =
                            GUILayoutUtility.GetRect(new GUIContent(key.stringValue), EditorStyles.objectField);
                        var valueRect =
                            GUILayoutUtility.GetRect(new GUIContent(value.stringValue), EditorStyles.objectField);
                        EditorGUIUtility.labelWidth = 50;
                        EditorGUI.PropertyField(keyRect, key, new GUIContent("Key:"), false);
                        EditorGUIUtility.labelWidth = 60;
                        EditorGUI.PropertyField(valueRect, value, new GUIContent("Value:"), false);
                        if (GUILayout.Button("删除"))
                        {
                            _remove(i);
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                }
            }
            EditorGUI.indentLevel -= 1;
            EditorGUIUtility.labelWidth = 0;
        }

        private void _remove(int keyIndex)
        {
            for (int i = 0; i < _languageNamesSer.arraySize; i++)
            {
                _valuesSer.DeleteArrayElementAtIndex(_getValueIndex(keyIndex, i) - i);
            }
            _keysSer.DeleteArrayElementAtIndex(keyIndex);

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        private int _getValueIndex(int keyIndex, int languageIndex)
        {
            var index = keyIndex + languageIndex * _keysSer.arraySize + 1 - 1;
            /* 0 + 0 + 1 - 1 = 0
             * 1 + 0 + 1 - 1 = 1
             * 2 + 0 + 1 - 1 = 2
             * 0 + 1 * 3 + 1 - 1 = 3
             * 1 + 1 * 3 + 1 - 1 = 4
             * 2 + 1 * 3 + 1 - 1 = 5
             */
            return index;
        }

        private string _languageName;
        //        private readonly List<string> _languageNames = new List<string>();
        private SerializedProperty _keysSer;
        private SerializedProperty _valuesSer;

        private void _createLanguage()
        {
            EditorGUILayout.BeginHorizontal();
            {
                _languageName = EditorGUILayout.TextField("语言名称:", _languageName);
                if (GUILayout.Button("创建") || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    if (string.IsNullOrWhiteSpace(_languageName) || _languageNamesExists(_languageName))
                    {
                        return;
                    }

                    if (!_add(_languageNamesSer, _languageName, $"创建{_languageName}失败！"))
                    {
                        return;
                    }

                    //                    _languageNames.Add(_languageName);
                    _languageName = string.Empty;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private bool _languageNamesExists(string languageName)
        {
            for (int i = 0; i < _languageNamesSer.arraySize; i++)
            {
                var item = _languageNamesSer.GetArrayElementAtIndex(i);
                if (item.stringValue == languageName)
                {
                    return true;
                }
            }

            return false;
        }

        bool _add(SerializedProperty serialized, string value, string errorMeeage, bool update = true)
        {
            if (serialized == null)
            {
                Debug.LogError(errorMeeage);
                return false;
            }
            serialized.arraySize += 1;
            var pro = serialized.GetArrayElementAtIndex(serialized.arraySize - 1);
            pro.stringValue = value;
            if (update)
            {
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }

            return true;
        }


        private void OnEnable()
        {
            _i18N = (I18NComponent)target;
            //            _languageNames.AddRange(_i18N.GetLanguges());
            _defaultLanguageSer = serializedObject.FindProperty("_defaultLanguage");
            _languageNamesSer = serializedObject.FindProperty("_languageNames");
            _keysSer = serializedObject.FindProperty("_keys");
            _valuesSer = serializedObject.FindProperty("_values");
        }
    }
}