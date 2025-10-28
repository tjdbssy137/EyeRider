using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Localization.Editor;
using UnityEngine;
using static Define;


namespace Data
{
    public interface IData
    {
        int Id { get; }
    }

    public class NoticeInfo
    {
        public enum EType
        {
            Unknown,
            Debug,
            Info,
            Warning,
            Error,
            Critical
        }

        public string Title;
        public string Notice;
        public EType Type;
        
        public NoticeInfo(string title, string notice, EType type = EType.Unknown)
        {
            this.Title = title;
            this.Notice = notice;
            this.Type = type;
        }
    }

    [Serializable]
    public class LocalizationData : IData
    {
        [JsonProperty("Id")]
        public int Id { get; set; }
        public string Description;
        public string KO;
        public string EN;
    }

    [Serializable]
    public class LocalizationDataLoader : ILoader<int, LocalizationData>
    {
        public List<LocalizationData> datas = new List<LocalizationData>();

        public Dictionary<int, LocalizationData> MakeDict()
        {
            Dictionary<int, LocalizationData> dict = new Dictionary<int, LocalizationData>();
            foreach (LocalizationData data in datas)
                dict.Add(data.Id, data);

            return dict;
        }
    }
}