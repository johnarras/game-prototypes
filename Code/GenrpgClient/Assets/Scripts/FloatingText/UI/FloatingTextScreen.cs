using System;
using System.Collections.Generic;
using UnityEngine;

using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Client.Assets.Constants;

public class FloatingTextQueuedItem
{
    public string Message;
    public string ArtName;
}

public class FloatingTextScreen : BaseScreen
{
    private IInputService _inputService;
    
    public GameObject _textAnchor;

    private string MessageArt = "FloatingMessageText";
    private string ErrorArt = "FloatingErrorText";

    
    public float _TimeBetweenMessages = 3.0f;

    private List<FloatingTextItem> _currentItems = new List<FloatingTextItem>();
    private List<FloatingTextQueuedItem> _messageQueue = new List<FloatingTextQueuedItem>();

    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        AddListener<ShowFloatingText>(OnReceiveMessage);
        await Task.CompletedTask;

    }

    private void OnReceiveMessage(ShowFloatingText message)
    {
        ShowMessage(message.Text, message.Art);
        return;
    }

    DateTime _lastShowTime = DateTime.UtcNow.AddDays(-1);
    float _timeDelta = 0;
    List<FloatingTextItem> _removeList = new List<FloatingTextItem>();

    protected override void ScreenUpdate()
    {

        if (_messageQueue.Count > 0 && (DateTime.UtcNow-_lastShowTime).TotalSeconds >= _TimeBetweenMessages)
        {
            FloatingTextQueuedItem firstItem = _messageQueue[0];
            _messageQueue.RemoveAt(0);

            _assetService.LoadAssetInto(_textAnchor, AssetCategoryNames.UI, firstItem.ArtName, OnLoadText, firstItem.Message, _token, Subdirectory);

        }


        _timeDelta = _inputService.GetDeltaTime();

        _removeList.Clear();

        foreach (FloatingTextItem ft in _currentItems)
        {
            ft.transform.localPosition += new Vector3(0, _timeDelta * ft.PixelsPerSecond, 0);
            ft.ElapsedSeconds += _timeDelta;

            if (ft.ElapsedSeconds > ft.DurationSeconds)
            {
                _removeList.Add(ft);
            }
        }
        foreach (FloatingTextItem item in _removeList)
        {
            if (_currentItems.Contains(item))
            {
                _currentItems.Remove(item);
            }
            _clientEntityService.Destroy(item.gameObject);
        }
        
    }

    private void ShowMessage(string msg, EFloatingTextArt art)
    {
        string prefabName = MessageArt;
        if (art == EFloatingTextArt.Error)
        {
            prefabName = ErrorArt;
        }


        if (_textAnchor == null || string.IsNullOrEmpty(msg) || string.IsNullOrEmpty(prefabName))
        {
            return;
        }

        FloatingTextQueuedItem queuedItem = new FloatingTextQueuedItem()
        {
            Message = msg,
            ArtName = prefabName,
        };

        _messageQueue.Add(queuedItem);
    }

    private void OnLoadText(object obj, object data, CancellationToken token)
    {
        GameObject go = obj as GameObject;
        if (go == null)
        {
            return;
        }
        string txt = data as String;
        if (string.IsNullOrEmpty(txt))
        {
            _clientEntityService.Destroy(go);
            return;
        }
        FloatingTextItem ft = go.GetComponent<FloatingTextItem>();
        if (ft == null || ft.TextString == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }
        _uiService.SetText(ft.TextString, txt);
        if (_currentItems == null)
        {
            _currentItems = new List<FloatingTextItem>();
        }

        _currentItems.Add(ft);
        ft.ElapsedSeconds = 0;
    }
}

