using OxDb.SharedGame.Spawns.WorldData;
using System.Collections.Generic;
using System.Linq;
//using TColl = System.Collections.Generic.HashSet<Entities.MapObjects.MapObject>;

namespace OxDb.SharedGame.MapObjects.Entities
{
    public class MapObjectGridData
    {
        public int GX { get; set; }
        public int GZ { get; set; }
        /// <summary>
        /// List of objects in a grid. This is handled in an immutable fashion so
        /// that it gets locked on Add and Remove, but any number of threads can read from it to get nearby objects.
        /// 
        /// Do we need a volatile field instead?
        /// </summary>
        protected List<MapObject> _objs { get; set; } = new List<MapObject>();
        public List<MapSpawn> Spawns { get; set; } = new List<MapSpawn>();
        public bool SpawnedObjects { get; set; }

        public object SpawnLock = new object();
        public object ObjsLock = new object();

        public volatile int PlayerCount = 0;

        public void AddObj(MapObject obj)
        {
            lock (ObjsLock)
            {
                List<MapObject> set = new List<MapObject>(_objs.Where(x => !x.IsDeleted()));
                set.Add(obj);
                _objs = set;
            }
        }

        public void RemoveObj(MapObject obj)
        {
            lock (ObjsLock)
            {
                List<MapObject> set = new List<MapObject>(_objs);
                set.Remove(obj);
                _objs = set;
            }
        }

        public IReadOnlyList<MapObject> GetObjects()
        {
            return _objs;
        }
    }
}


