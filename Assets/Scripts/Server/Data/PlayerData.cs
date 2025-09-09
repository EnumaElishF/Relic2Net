using MongoDB.Bson.Serialization.Attributes;
using UnityEngine;

public class PlayerData
{
    [BsonId]
    public string name;
    public string password;
    public CharacterData characterData;

}
public class CharacterData
{
    public Vector3 position;
    public float rotation_Y;
}