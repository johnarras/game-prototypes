namespace OxDb.PlatformServer.Accounts.PlayerData
{
    public class AccountConnection : BaseAccountData
    {
        public override string Id { get; set; }
        /// <summary>
        /// Your account id.
        /// </summary>
        public string AccountId { get; set; }
        /// <summary>
        /// The account Id of this connection (not necessarily your referrer)
        /// </summary>
        public string ReferrerId { get; set; }
        /// <summary>
        /// How far from you this connection is in the graph.
        /// </summary>
        public int Depth { get; set; }
        /// <summary>
        /// Which product graph this is a part of (1 = main account graph)
        /// </summary>
        public long ProductId { get; set; }
        /// <summary>
        /// Which index this connection is 1 (for simple tree, vs others where we 
        /// attempt overlap.
        /// </summary>
        public int Index { get; set; }
    }
}


