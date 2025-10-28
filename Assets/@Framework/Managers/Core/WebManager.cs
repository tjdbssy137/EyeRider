using Global.Shared.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Types;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEditor.ObjectChangeEventStream;

public class WebManager
{

    public void Init()
    {
        new RoutePaths();
    }

    public async Awaitable<ApiResultModel<Response>> GetRequest<Request, Response>(Request dto,
        Action<string> completeAction = null, CancellationToken ct = default)
    {
        await Awaitable.MainThreadAsync();
        if (!RoutePaths.Routes.TryGetValue(typeof(Request), out var url))
            return ApiResultModel<Response>.RouteNotFound(typeof(Request));

        url = AppendQuery(url, dto);
        //var text = await _transport.GetAsync(url, ct);
        //return ParseResponse<ResDto>(text, complete);

        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<color=green><b>Web Log Start[GET] : </b></color>");
            builder.AppendLine($" url : {url}");
            Debug.Log(builder.ToString());
        }
        using (var req = UnityWebRequest.Get(url))
        {
            await req.SendWebRequest();


#if UNITY_2020_1_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                return new ApiResultModel<Response>
                {
                    SuccessStatus = ApiResultModelStatus.ProtocolError,
                    Message = req.error,
                };
            }

            var responseContentString = req.downloadHandler.text;
            var responseDto = JsonConvert.DeserializeObject<ApiResultModel<Response>>(responseContentString);
            if (responseDto == null)
            {
                return new ApiResultModel<Response>
                {
                    SuccessStatus = ApiResultModelStatus.DtoVersionError,
                    Message = responseContentString,
                };
            }
            completeAction?.Invoke(responseContentString);


            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("<color=green><b>Web Log End[GET] : </b></color>");
                builder.AppendLine($" {responseContentString}");
                Debug.Log(builder.ToString());
            }
            return responseDto;
        }
    }

    public async Awaitable<ApiResultModel<Response>> PostRequest<Request, Response>(Request dto,
        Action<string> completeAction = null, CancellationToken ct = default)
    {
        await Awaitable.MainThreadAsync();
        if (!RoutePaths.Routes.TryGetValue(typeof(Request), out var url))
            return ApiResultModel<Response>.RouteNotFound(typeof(Request));


        string json = JsonConvert.SerializeObject(dto);
        var bodyRaw = Encoding.UTF8.GetBytes(json);
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<color=green><b>Web Log Start[POST] : </b></color>");
            builder.AppendLine($" url : {url}");
            builder.AppendLine($" body : {json}");
            Debug.Log(builder.ToString());
        }
        using (var req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            await req.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                return new ApiResultModel<Response>
                {
                    SuccessStatus = ApiResultModelStatus.ProtocolError,
                    Message = req.error,
                };
            }

            var responseContentString = req.downloadHandler.text;
            var responseDto = JsonConvert.DeserializeObject<ApiResultModel<Response>>(responseContentString);
            if (responseDto == null)
            {
                return new ApiResultModel<Response>
                {
                    SuccessStatus = ApiResultModelStatus.DtoVersionError,
                    Message = responseContentString,
                };
            }
            completeAction?.Invoke(responseContentString);


            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("<color=green><b>Web Log End[POST] : </b></color>");
                builder.AppendLine($" {responseContentString}");
                Debug.Log(builder.ToString());
            }
            return responseDto;
        }
    }

    public static Dictionary<string, object> ToDictionary(object obj)
    {
        var json = JsonConvert.SerializeObject(obj);
        var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        return dict;

    }
    public static string AppendQuery(string url, object dto)
    {
        var dict = ToDictionary(dto);

        if (dict == null || dict.Count == 0)
            return url;

        var sb = new StringBuilder(url);
        bool hasQuery = url.Contains("?");

        foreach (var kv in dict)
        {
            sb.Append(hasQuery ? '&' : '?');
            sb.Append(Uri.EscapeDataString(kv.Key));
            sb.Append('=');
            sb.Append(Uri.EscapeDataString(kv.Value?.ToString() ?? ""));
            hasQuery = true;
        }
        return sb.ToString();
    }
}


