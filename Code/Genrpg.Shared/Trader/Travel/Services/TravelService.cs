using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Utils;
using System;

namespace Genrpg.Shared.Trader.Travel.Services
{
    public class CityPath
    {
        public City City { get; set; }
        public double TotalDistance = 100000000;
        public CityPath PrevCityPath { get; set; } = null;
    }

    public interface ITravelService : IInjectable
    {
        public void SetWaterMask(byte[] waterMask);

        bool IsWater(double x, double y);
        int GetWaterMaskIndex(double x, double y);

    }


    public class TravelService : ITravelService
    {
        private IGameData _gameData = null;

        private byte[] _waterMask = null;


        private int _width = 8192;
        private int _height = 8192;

        // Assume the mask will be 2x by x 
        public void SetWaterMask(byte[] waterMask)
        {
            _waterMask = waterMask;
            int length = waterMask.Length;
            int totalSize = length * 8;
            totalSize /= 2;
            int size = (int)(Math.Sqrt(totalSize));
            _width = size * 2;
            _height = size;
        }

        public int GetWaterMaskIndex(double x, double y)
        {
            if (_waterMask == null)
            {
                return 0;
            }
            return (int)x + (int)y * _width;
        }

        public bool IsWater(double x, double y)
        {
            if (_waterMask == null)
            {
                return false;
            }
            int index = GetWaterMaskIndex(x, y);
            int byteIndex = index / 8;
            int bitOffset = index % 8;

            if (byteIndex >= _waterMask.Length)
            {
                return false;
            }

            return FlagUtils.IsSet(_waterMask[byteIndex], (1 << bitOffset));
        }
    }
}


