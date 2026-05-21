namespace OxDb.PlatformServer.Accounts.PlayerData
{
    public class ConnectionCount : BaseAccountData
    {
        public override string Id { get; set; }
        /// <summary>
        /// Account Id for this user
        /// </summary>
        public string AccountId { get; set; }
        /// <summary>
        /// Which product this graph is for
        /// </summary>
        public long ProductId { get; set; }
        /// <summary>
        /// Which graph index this is for
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Direct (Depth=1) connection count
        /// </summary>
        public long DirectCount { get; set; }
        /// <summary>
        /// Viral (depth > 1) connection count
        /// </summary>
        public long ViralCount { get; set; }

    }
}


