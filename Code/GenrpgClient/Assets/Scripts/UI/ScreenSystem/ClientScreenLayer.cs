using Assets.Scripts.UI.Entities;
using OxDb.SharedGame.UI.Settings;
using System.Collections.Generic;
using UnityEngine;

public class ClientScreenLayer
{
    public ScreenLayer Layer { get; set; }
    public ActiveScreen CurrentScreen { get; set; }
    public ActiveScreen CurrentLoading { get; set; }
    public List<ActiveScreen> ScreenQueue { get; set; } = new List<ActiveScreen>();
    public GameObject LayerParent { get; set; }
    public bool JustClosedScreen { get; set; }

}


