using JKFrame;
using MongoDB.Bson.Serialization.Attributes;
using UnityEngine;

public class PlayerData
{
    [BsonId]
    public string name;
    public string password;
    public CharacterData characterData = new CharacterData();
    public BagData bagData = new BagData();

    public void Init(string name, string password,Vector3 position)
    {
        this.name = name;
        this.password = password;
        this.characterData.position = position;
    }


}
//这里都是会存到MongoDB数据库的内容数据
public class CharacterData
{
    public Vector3 position;
    public float rotation_Y;
    public string usedWeaponName;
}