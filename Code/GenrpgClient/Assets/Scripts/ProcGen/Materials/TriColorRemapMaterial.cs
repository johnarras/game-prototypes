using Assets.Scripts.Core;
using Genrpg.Shared.Accounts.PlayerData;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.ProcGen.Materials
{
    public class TriColorRemapMaterial : BaseBehaviour
    {
        public Color RedRemap;
        public Color GreenRemap;
        public Color BlueRemap;

        public GRawImage RawImage;

        public void Start()
        {
           RawImage.material = new Material(RawImage.material);
        }

        public void Update()
        {
            RawImage.material.SetColor("_RedRemap", RedRemap);
            RawImage.material.SetColor("_BlueRemap", BlueRemap);
            RawImage.material.SetColor("_GreenRemap", GreenRemap);

        }
    }
}
