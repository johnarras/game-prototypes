using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Trader.Cities.Settings;
using System;

namespace OxDb.SharedGame.Trader.Travel.Services
{
    public class CityPath
    {
        public City City { get; set; }
        public double TotalDistance = 100000000;
        public CityPath PrevCityPath { get; set; } = null;
    }

    public interface ITravelService : IInjectable
    {
        public void SetTerrainMap(byte[] terrainMap);

        bool IsWater(double x, double y);
        int GetTerrainIndexIndex(double x, double y);
        int GetTerrainIndex(double x, double y);

    }


    public class TravelService : ITravelService
    {
        private byte[] _terrainIndexes = null;


        private int _width = 8192;
        private int _height = 8192;

        // Assume the mask will be 2x by x 
        public void SetTerrainMap(byte[] terrainIndexes)
        {
            _terrainIndexes = terrainIndexes;
            int length = terrainIndexes.Length;
            int totalSize = length;
            totalSize /= 2;
            int size = (int)(Math.Sqrt(totalSize));
            _width = size * 2;
            _height = size;
        }

        public int GetTerrainIndexIndex(double x, double y)
        {
            if (_terrainIndexes == null)
            {
                return 0;
            }

            if (x < 0 || y < 0 || x >= _width || y >= _height)
            {
                return 0;
            }

            return (int)x + (int)y * _width;
        }

        public int GetTerrainIndex(double x, double y)
        {
            int maskIndex = GetTerrainIndexIndex(x, y);

            return _terrainIndexes[maskIndex];
        }

        public bool IsWater(double x, double y)
        {
            if (_terrainIndexes == null)
            {
                return false;
            }
            int index = GetTerrainIndexIndex(x, y);
            return _terrainIndexes[index] == 0;
        }
    }
}


