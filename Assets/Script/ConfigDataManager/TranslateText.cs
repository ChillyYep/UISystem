using ConfigData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 翻译文本
/// </summary>
[RequireComponent(typeof(Text))]
public class TranslateText : MonoBehaviour
{
    /// <summary>
    /// 在UI组中翻译项ID
    /// </summary>
    public int m_uiTranslateID;
    /// <summary>
    /// 在所有翻译项中的ID
    /// </summary>
    public int m_translateID;

    private Text m_text;

    public Text SourceText
    {
        get
        {
            if (m_text == null)
            {
                m_text = GetComponent<Text>();
            }
            return m_text;
        }
    }

    public void Awake()
    {
        if (ConfigDataManager.Instance.ConfigDataLoader.ConfigDataUITranslateTextItemTable.TryGetValue(m_uiTranslateID, out var translateItem))
        {
            SourceText.text = translateItem.sourcetext;
        }
    }
}
