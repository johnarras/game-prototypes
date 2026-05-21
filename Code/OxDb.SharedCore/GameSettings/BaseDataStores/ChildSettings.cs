namespace OxDb.SharedCore.GameSettings.BaseDataStores
{

    public interface IChildSettings
    {

    }

    public abstract class ChildSettings : BaseGameSettings, IChildSettings
    {
        [MessagePack.IgnoreMember]
        public abstract string ParentId { get; set; }

        [MessagePack.IgnoreMember]
        public abstract override string Name { get; set; }
    }
}


