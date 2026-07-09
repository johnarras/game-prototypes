using AssetProcessing;
using UnityEditor;

namespace Assets.Editor.SpriteUtils
{
    public static class CreateSpritesUtils
    {

        [MenuItem("Tools/Crawler/Setup Monster Sprites")]
        public static void SetupCrawlerMonsterImages()
        {
            SpritePrefabBuilder.ProcessSpritesToPrefabs("FullAssets/Crawler/Images/Monsters", "BundledAssets/SpriteLists");
        }

        [MenuItem("Tools/Crawler/Copy Minimap Terrain To 3D")]
        public static void Setup3DTerrainFromMinimapTerrain()
        {
            TextureToTerrainUtils.CopyTexturesToTerrainTextures("FullAssets/Crawler/Atlas/CrawlerMinimapAtlas", "FullAssets/TerrainTex/Textures");
        }

    }
}
