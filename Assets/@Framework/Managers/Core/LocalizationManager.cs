
using UnityEngine;

public class LocalizationManager
{
    public enum ELanguage
    {
        English,
        Korean
    }

    public ELanguage Language { get; private set; } = ELanguage.English;

    public void Init()
    {
        this.SetLanguage(ELanguage.Korean);
    }

    public void SetLanguage(ELanguage language)
    {
        Language = language;
    }

    public string GetText(int key)
    {
        if(Managers.Data.LocalizationDic.ContainsKey(key) == false)
        {
            Debug.LogError($"LocalizationManager::GetText key not found key={key}");
            return key.ToString();
        }

        switch (Language)
        {
            case ELanguage.English:
                return Managers.Data.LocalizationDic[key].EN;
            case ELanguage.Korean:
                return Managers.Data.LocalizationDic[key].KO;
        }

        return "";
    }
}