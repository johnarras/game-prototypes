namespace OxDb.SharedGame.DataStores.Categories.PlayerData.Constants
{
    /// <summary>
    /// This is used to track if a player document ever existed and then gets sent to the client with the jwt-style
    /// session token as a long int, and when data is loaded during the session, if it's missing and the bit is
    /// set here, it throws an exception meaning somehow we had a document and now it's gone.
    /// </summary>
    public class EPersonalDataOffsetBits
    {
        public const int None = 0;
        public const int Core = 1;
        public const int GameAccount = 2;
        public const int Attributes = 3;
        public const int Holdings = 4;
        public const int Caravan = 5;
        public const int Ftue = 6;
        public const int Shipments = 7;
        public const int PurchaseHistory = 8;
        public const int CurrentPurchases = 9;
        public const int StoreOffers = 10;
        public const int Resets = 11;
    }
}
