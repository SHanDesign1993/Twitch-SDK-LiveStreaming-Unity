using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
// If type or namespace TwitchLib could not be found. Make sure you add the latest TwitchLib.Unity.dll to your project folder
// Download it here: https://github.com/TwitchLib/TwitchLib.Unity/releases
// Or download the repository at https://github.com/TwitchLib/TwitchLib.Unity, build it, and copy the TwitchLib.Unity.dll from the output directory
using TwitchLib.Unity;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Demo : MonoBehaviour
{
    private Api _api;
    [SerializeField] Button openURLBtn;
    [SerializeField] TMP_InputField IdField;
    [SerializeField] TextMeshProUGUI streamstatusLabel;

    private void Start()
    {
        // To keep the Unity application active in the background, you can enable "Run In Background" in the player settings:
        // Unity Editor --> Edit --> Project Settings --> Player --> Resolution and Presentation --> Resolution --> Run In Background
        // This option seems to be enabled by default in more recent versions of Unity. An aditional, less recommended option is to set it in code:
        // Application.runInBackground = true;

        // Create new instance of Api
        _api = new Api();

        // The api needs a ClientID or an OAuth token to start making calls to the api.

        // Set the client id
        _api.Settings.ClientId = Secrets.CLIENT_ID;

        // Set the oauth token.
        // Most requests don't require an OAuth token, in which case setting a client id would be sufficient.
        // Some requests require an OAuth token with certain scopes. Make sure your OAuth token has these scopes or the request will fail.
         _api.Settings.AccessToken = Secrets.ACCESS_TOKEN;

        IdField.onValueChanged.AddListener(OnInputChanged);
    }

    public void OpenURL()
    {
        Application.OpenURL(streamstatusLabel.text);
    }

    public void OpenStream()
    {
        StartCoroutine(StreamToTwitchCoroutine());
    }

    IEnumerator StreamToTwitchCoroutine()
    {
        //---Get Ingest Server
        Task<TwitchLib.Api.Models.v5.Ingests.Ingests> taskGetServer = _api.Ingests.v5.GetIngestServerListAsync();
        yield return new WaitUntil(() => taskGetServer.Status == TaskStatus.RanToCompletion);
        var servertemplate = taskGetServer.Result.IngestServers[0].UrlTemplate;
        Debug.Log($"template server is {servertemplate}");

        //---Get Streamkey
        Task<TwitchLib.Api.Models.v5.Channels.ChannelAuthed> taskGetStreamkey = _api.Channels.v5.GetChannelAsync();
        yield return new WaitUntil(() => taskGetStreamkey.Status == TaskStatus.RanToCompletion);
        var streamkey = taskGetStreamkey.Result.StreamKey;
        Debug.Log($"stream key is {streamkey}");

        //---Get push_addr
        var streamstr = "{stream_key}";
        var push_addr = servertemplate.Substring(0, servertemplate.Length - streamstr.Length) + streamkey;
        Debug.Log($"push_addr is {push_addr}");

        openURLBtn.interactable = true;

        #region  push Stream to rtmp address
        Debug.Log("======push Stream to rtmp address Started=======");

        //TODO : 

        #endregion
    }

    private void OnInputChanged(string text)
    {
        streamstatusLabel.text = "https://www.twitch.tv/" + text;
    }

    #region Unused Method
    private IEnumerator GetChannelVideosByUsername(string usernameToGetChannelVideosFrom)
    {
        // Lets get Lucky's id first
        TwitchLib.Api.Models.Helix.Users.GetUsers.GetUsersResponse getUsersResponse = null;
        yield return _api.InvokeAsync(_api.Users.helix.GetUsersAsync(logins: new List<string> { usernameToGetChannelVideosFrom }),
                                      (response) => getUsersResponse = response);
        // We won't reach this point until the api request is completed, and the getUsersResponse is set.

        // We'll assume the request went well and that we made no typo's, meaning we should have 1 user at index 0, which is LuckyNoS7evin
        string luckyId = getUsersResponse.Users[0].Id;

        // Now that we have lucky's id, lets get his videos!
        TwitchLib.Api.Models.v5.Channels.ChannelVideos channelVideos = null;
        yield return _api.InvokeAsync(_api.Channels.v5.GetChannelVideosAsync(luckyId),
                                      (response) => channelVideos = response);
        // Again, we won't reach this point until the request is completed!

        // Handle user's ChannelVideos
        // Using this way of calling the api, we still have access to usernameToGetChannelVideosFrom!

        var listOfVideoTitles = GetListOfVideoTitles(channelVideos);
        var printableListOfVideoTitles = string.Join("  |  ", listOfVideoTitles);

        Debug.Log($"Videos from user {usernameToGetChannelVideosFrom}: {printableListOfVideoTitles}");
    }

    private void GetChannelVideosCallback(TwitchLib.Api.Models.v5.Channels.ChannelVideos e)
    {
        var listOfVideoTitles = GetListOfVideoTitles(e);
        var printableListOfVideoTitles = string.Join("  |  ", listOfVideoTitles);

        Debug.Log($"Videos from 14900522: {printableListOfVideoTitles}");
    }

    private List<string> GetListOfVideoTitles(TwitchLib.Api.Models.v5.Channels.ChannelVideos channelVideos)
    {
        List<string> videoTitles = new List<string>();

        foreach (var video in channelVideos.Videos)
            videoTitles.Add(video.Title);

        return videoTitles;
    }
    #endregion
}