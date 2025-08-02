
8/1/2025

This repo contains prototypes of projects I find interesting to work on.

The art is a bit I made myself, a bit I found online that can be redistributed,
and some AI art.

QUICKSTART: If you want to play something quickly, open the GenrpgClient project
in the Unity Editor, load the GameMain scene, click on the InitClient object, then
the ClientConfig reference and set the GameMode to Crawler. Then press play 
in the editor and you should just be able to play the retro dungeon crawler.

Toplevel folders:

	AppConfig -- contains the App.Config file used for online services.

	Code -- contains subfolders with different projects/solutions described more below.

	GameData -- game settings you will want to add to NoSQL using the editor during setup.

	ImportData -- CSVs for using the importers in the editor. These are fixed
		file locations since I am lazy, but also it allows you to use git to version
		your CSVs.
		
	Old -- contains genmud, an old MUD in C that I made 20-25 years go. It's also available
		on SourceForge.

There are 4 gmae projects of various kinds using a lot of shared code.
The parts of the projects are all in one repo, and that makes it easier
for one person or a really small team to work faster, but you probably
will want to split things up if you grow to a large team.

The games:

1. Crawler - procedurally generated dungeon crawler with Roguelite elements.
	You can open the Unity client and in the GameMain scene click on 
	InitClient and then ClientConfig and then change the GameMode to Crawler
	and immedately play the game (assuming no bugs) without any other setup.

2. BoardGame - a boardgame where you roll around a loop and gather resources
				and spend on upgrades to get more resources. Client/server
				meant for mobile.

3. MMO - a procedurally generated MMO where all monsters are simple, but active
			all the time. Realtime multiplayer client/server.

4. Trader - a game about being a caravan trader traveling the spice road.
		Meant to be a mobile game.


All of these can be played using the same client, and the game you enter can
be picked by clicking on the InitClient object, then the ClientConfig and
changing the GameMode.

For everything else besides the Crawler game, there are some more setup steps.

1. I am using Azure for my backend and all data editing goes through it. I am currently using:
	a. Blob Storage
	b. Cosmos with the Mongo API (currently the serverless tier)
	c. Service Bus (with PubSub...currently $10 tier for that)

2. Look in the AppConfig/App.Config file. At a minimum you will need to set up:

	a. BlobDefaultConnection to be a Blob Storage connection string
	b. NoSQLDefaultConnection to some flavor MongoDb. 
		Note that the & symbol in the App.Config must be escaped using &amp;
	c. The Service Bus (Queue and PubSub) connection.
	d. The content root that points to your blob storage or CDN root. 
	e. The EtherscanKey was from a smaller prototype involving using Ethereum transaction 
		hashes as random seeds for NFTs where the stats are based on the hash. 
		Totally optional. Interesting idea, never went anywhere.
		
3. You will now need to build the FileUploader. There is no CI/CD for uploading assets,
	and I don't want any connection strings/keys in the client. So navigate into FileUploader
	and open the solution and build it. It will output to the folder that the client looks
	for when building bundles or proecdurally generating a map for the MMO.
	
4. The implementations for the Azure code are found in the AzureOnlineResourceProvider 
	that implements IOnlineResourceProvider so if you want to use another cloud service 
	or self-host, you can implement those interfaces.
	
5. There are 4 Visual Studio solutions I tend to have open when working.

	a. 	Genrpg.GameServer -- MMO map server plus some microservice servers for the MMO.
		Run this after generating a map. 
		**** I have a lot of shared code across different parts of the project, and in
			particular the Genrpg.Shared dll gets copied into the client when Genrpg.GameServer
			is rebuilt.
		
	b. 	Genrpg.WebServer -- Used for auth for the MMO and for the web server component for BoardGame and Trader.
	
	c. 	Genrpg.Editor -- Procedurally generated editor that has CSV importer funcitonality. You will need to copy
		data to the database the first time you use this. 
		Button Column Meaning:
			1. dev Data -- edit the current data in dev environment
			2. dev Importer -- opens a window listing importers int he ImportData folder (Currently there is no
				file select for impoted files, you just have to put them in that one spot so they get into git.
			3. dev CopyToGit -- copy the game data database to the GameData folder in git.
			4. dev CopyToClient -- copy the current default data to the client's Resources/BakedGameData folder
				to ship the player with default data.
			5. dev CopyToServers -- this triggers a PubSub messing telling servers (web/mmo) to reload their game data.
				Game data/settings does not have a TTL, it has to explicitly be flushed.
			6. dev MessageSetup -- this is to synchronize the MessagePack attributes between client and server (This is
				a big reason for the shared Genrpg.Shared .ddl) this also fixes up the Key attributes so they are
				sequential.
			7.	dev Users -- lets you edit user data.
			8.	dev CopyToTest -- example of copying data to another environment .Prob best to do this from git.
			9.	dev CopyToDB -- **** DO THIS INITIALLY TO GET GAME SETTINGS FROM GIT INTO THE DB ****
			10. dev TestAccountSettings -- don't really use
			11. Test Shared Random -- was seeing how Random.Shared performed vs differnet randoms running in parallel.
			
	d. Genrpg.Client -- the game client in some recent version of Unity. I set this up with some topelevel folders:
		1. BundledAssets -- the asset bundle system I use uses the path to an asset to determine its asset bundle name.
			The system only goes one level deep, and the asset loading system uses an AssetCategoryName which is the root
			folder name to find which bundle is used for which item. I prefer this because a giant explicit
			list of which asset is in which bundle is not needed. The AssetService figures it out using the category + asset name
			(+ subdirectory sometimes)
		2. FullAssets - The big chunks of data going into assets generally go here.
		3. Plugins -- external .dlls
		4. Resources -- core assets that ship with the game. I try to keep this small.
		5. Scenes -- the one scene in the game.
		6. Scripts -- all scripts go here. They are generally found inside of folders describing their features.
		7. Settings -- used by Unity
		8. StreamingAssets -- local bundles get copied here to be embedded in players.
		9. TextMesh Pro -- assets to make TextMeshPro work.
	
	
	---	The Genrpg.ServerShared .dll is used within all online/server assemblies. (GameServer/Editor/WebServer)
	
	--- The Genrpg.Shared .dll is used in all server solutions and in the client. It is copied to the client when the
			Genrpg.GameServer solution is rebuilt.
		
		
			
I prefer convention over configuration so a lot of reflection is used to set things up at program startup and to 
index things automatically.

	a. There is a basic dependency injection system that adds things implementing IInjectable to a hidden service
	locator during the startup process by walking the assembly dependency graph from the executing assembly through
	all assemblies prefixed with Genrpg.
	
	b. The Strategy pattern is used a lot. The root interface for this is: ISetupDictionaryItem<K,V> where K is the keyspace
		and V is the value space (usually some other interface).
		-- The SetupDictionaryContainer<K,V> class allows for an ISetupDictionary<K,V> collection to be automatically set up
			and have dependencies injected into it during initialization.
	
	c. To create an in-memory database of game settings, the IEntityHelper interface is implemented whenever 
		it makes sense to have a list of objects of a certain type. (By default extend BaseEntityHelper)
		These helpers are used in the server editor to dynamically generate dropdowns (GetDropdownList), and in the client in the 
		EntityIdDropdown and EntityTypeWithIdUI classes. Since I do things this way, I can avoid using 
		enums everywhere, even though enums are easier to use to make dropdowns.
		
	d. Adding game settings or player data requires implementing a handful of small classes that get woven into
		existing core systmes at startup using reflection. Look at CurrencyData and CurrencySettings for examples
		of this pattern. I generally copy/paste simple files like the CurrencyData or CurrencySettings classes
		and change Currency to whatever feature I want to make and things tend to "just work" since the code
		is split into totally new small generic classes.
	 				
Starting to Play.

As per the Quickstart above:

Open the Unity client and click on InitClient and then ClientConfig and change the GameMode to Crawler. You should
be able to start the game and play. Every other game mode will require the WebServer at least.

Other things to do:

Generating an MMO map requires you to log in to the WebServer, then click the Gen button. Use the settings
in InitClient to generate the map. Keep the BlockCount small (like 4) since the generation process
is very slow and can take hours if you make a map with 70-80 blocks. If you want to create such a large map,
go into release mode in the editor and close the Scene window and the Game window once you click Gen.
Drawing the map during the generation process slows things down a lot. If you set up the
Azure account and FileUploader correctly, it should upload the map chunks to blob storage and
if you have the WebServer open, it should upload the map and spawns to the World NoSQL database.

Under Tools there are BuildClients and BuildBundles buttons that bring up windows that
let you do those things.

You will want to open the editor and click dev CopyDataToDb to copy data from git to the Db.

When editing I like to save my data and then dev CopyToGit and dev CopyToClient at the same time
and even though it takes a few seconds, I know that the data is backed up once I commit and push git.


Adding new features:

I always add data to the Genrpg.Shared project with a toplevel folder with the feature name and subfolders:

	Constants - for constants
	Entities - for temp objects used inside the program that are not saved/transmitted anywhere
	Messages - for MMO messaging (if needed)
	PlayerData - for player data
	Services - for code (I tend to split code into data objects and code objects separately
	Settings - for game data/settings/configs
	
There are certain patterns I use that allow me to keep adding stuff. Let's use Currencies feature 
	for this example:
		
	a. Constants - CurrencyTypes lists explicit Ids. I tend to not use Enums because they can deserialize badly and don't
		allow you to add a new item of a certain type without a new release or without a lot of versioning. A lot of the
		code uses the EntityTypeId/EntityId pairs that let a spawn table or loot table or other object contain a list
		of anything. 

	b. PlayerData - CurrencyData:
	
				
			//(Parent class containing a list of child CurrencyStatusObjects)
		    [MessagePackObject]
		    public class CurrencyData : OwnerQuantityObjectList<CurrencyStatus>
		    {
		        [Key(0)] public override string Id { get; set; }
		
		        public long GetQuantity(long currencyTypeId)
		        {
		            return Get(currencyTypeId).Quantity;
		        }
		    }
		
		    // Class used for one currency for the player.
		    [MessagePackObject]
		    public class CurrencyStatus : OwnerQuantityChild
		    {
		        [Key(0)] public override string Id { get; set; }
		        [Key(1)] public override string OwnerId { get; set; }
		        [Key(2)] public override long IdKey { get; set; }
		        [Key(3)] public override long Quantity { get; set; }
		
		    }
		
		    // Class used to transfer data between client and server.
		    public class CurrencyDto : OwnerDtoList<CurrencyData, CurrencyStatus> { }
		
		    // Class used to load/save data from a program to a datastore (could be file storage if local)
		    public class CurrencyDataLoader : OwnerIdDataLoader<CurrencyData, CurrencyStatus> { }
		
		
			// Used to convert between the dto and the internal state used in the client/server
		    public class CurrencyDataMapper : OwnerDataMapper<CurrencyData, CurrencyStatus, CurrencyDto> { }
		
		    
	c. 
		
