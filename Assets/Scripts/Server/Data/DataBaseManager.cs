using JKFrame;
using MongoDB.Driver;
using UnityEngine;

public class DataBaseManager: SingletonMono<DataBaseManager>
{
    [SerializeField] private string connstr = "mongodb://localhost:27017";
    private MongoClient mongoClient;
    private IMongoDatabase mmoDatabase;
    private IMongoCollection<PlayerData> playerDataCollection;
    public void Init()
    {
        //连接 MongoDB
        mongoClient = new MongoClient(connstr);
        //查找或建立DataBase,如果没有会自动创建
        mmoDatabase = mongoClient.GetDatabase("Relic2Net_MMONetCode");
        //查找或建立集合,如果没有会自动创建
        playerDataCollection = mmoDatabase.GetCollection<PlayerData>("PlayerData");

    }

    public PlayerData GetPlayerData(string playerName)
    {
        //查找
        PlayerData playerData = playerDataCollection.Find(Builders<PlayerData>.Filter.Eq(nameof(PlayerData.name),playerName)).FirstOrDefault();
        return playerData;
    }
    public void CreatePlayerData(PlayerData playerData)
    {
        //创建
        playerDataCollection.InsertOne(playerData);
    }
    public void SavePlayerData(PlayerData newPlayerData)
    {
        //替换修改
        playerDataCollection.ReplaceOne(Builders<PlayerData>.Filter.Eq(nameof(PlayerData.name), newPlayerData.name), newPlayerData);
    }
}