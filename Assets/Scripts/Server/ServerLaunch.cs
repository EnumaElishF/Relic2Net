using MongoDB.Bson.Serialization.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using MongoDB.Driver;
using System.Collections.Generic;
// [BsonId]: 确定某个成员(变量、属性)是主键,无论什么成员(变量、属性)名在MongoDB里面都是 _id
// [BsonElement]: 要求一定序列化，即使private也要序列化
// [BsonIgnore]: 忽视成员(变量、属性)，即使public也不要
// [BsonDateTimeOptions(Kind = DateTimeKind.Local)]: 设置时间时区
public class TestUseData
{
    [BsonId]
    public int useID;
    public string name;
    public int lv;
    //[BsonElement]
    //private int age;
    //[BsonIgnore]
    //private int age2;
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime dateTime;

}
public class ServerLaunch : MonoBehaviour
{
    void Start()
    {
/*        //连接 MongoDB
        string connstr = "mongodb://localhost:27017/";
        MongoClient mongoClient = new MongoClient(connstr);
        //查找或建立DataBase,如果没有会自动创建
        IMongoDatabase mongoDatabase = mongoClient.GetDatabase("Relic2Net_MMONetCode");
        //查找或建立集合,如果没有会自动创建
        IMongoCollection<TestUseData> userInfoCollection = mongoDatabase.GetCollection<TestUseData>("UseInfo");

        //插入数据
        TestUseData testUseData = new TestUseData
        {
            useID = 1,
            name = "WSD",
            lv=1,
            dateTime = DateTime.Now
        };
        userInfoCollection.InsertOne(testUseData);
        // 查询
        // 查询一个
        TestUseData useData = userInfoCollection.Find(Builders<TestUseData>.Filter.Eq("useID", 1)).FirstOrDefault();
        Debug.Log(useData.name);
        //查询所有
        List<TestUseData> useDataList = userInfoCollection.Find(Builders<TestUseData>.Filter.Empty).ToList();
        Debug.Log(useDataList.Count);
        //修改 ： 修改useID 为1的 这条数据的等级lv为2
        userInfoCollection.UpdateOne(Builders<TestUseData>.Filter.Eq("useID", 1), Builders<TestUseData>.Update.Set("lv", 2));
        //替换
        testUseData.lv = 3;
        userInfoCollection.ReplaceOne(Builders<TestUseData>.Filter.Eq("useID", 1), testUseData);
        //删除
        //userInfoCollection.DeleteOne(Builders<TestUseData>.Filter.Eq("useID", 1));

        return;*/

        Application.targetFrameRate = 30; //帧数设置
        InitServers();
        SceneManager.LoadScene("GameScene");
    }

    private void InitServers()
    {
        //NetManager继承的基类NetworkManager做了不会销毁的设置代码
        //被ServerResSystem取代：ResSystem.InstantiateGameObject<NetManager>("NetworkManager").Init(false);//直接用的同步，因为文件很小，如果大了还是要用异步加载
        //  因为可能有其他的逻辑要走，所以我们没有用原生的函数NetworkManager而是自制->NetManager
        //--->我们自建一个服务网络启动
        //NetManager.Instance.InitServer();

        ServerResSystem.InstantiateNetworkManager().Init(false);
        Debug.Log("InitServers Succeed");
    }
    
}
