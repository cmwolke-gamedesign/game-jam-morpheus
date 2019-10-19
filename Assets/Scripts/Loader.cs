using UnityEngine;
using System.Collections;
using Assets.Scripts.manager;
using TinyMessenger;
using Assets.Scripts.message.custom;
using Assets.Scripts.entity;
using System.Collections.Generic;
using Assets.Scripts.entity.modules;
using System.IO;
using Assets.Scripts.map;

public class Loader : MonoBehaviour 
{
    private IMessageBus _bus;
    private const string CHARACTERS_PATH = "/Resources/Characters";
    private const string PLAYER = "/player.json";
    private const string ENEMIES = "/enemies.json";
    private const string MAP = "Map/Test/level1_final_unity";

    private TinyMessageSubscriptionToken _token;

	// Use this for initialization
	void Start() 
    {
        //_bus = Initialiser.Instance.GetService<IMessageBus>();
        //_bus.Subscribe<LoadMapMessage>(OnLoadMap);
        //TestSavePlayer();
        //TestSaveEnemies();
        //Load();
	}

    public void Awake()
    {
        _bus = Initialiser.Instance.GetService<IMessageBus>();
        _token = _bus.Subscribe<LoadMapMessage>(OnLoadMap);
    }

    private void OnLoadMap(LoadMapMessage obj)
    {
        _bus.Publish(new MapLoadedMessage(this, GetTiles()));
    }

    public void OnDisable()
    {
        _bus.Unsubscribe<LoadMapMessage>(_token);
    }


    private void TestSaveEnemies()
    {
        Enemies enemies = new Enemies();

        Connection enemy1 = new Connection();
        enemy1.Data = new Data()
        {
            CurrentMusicType = new GameType(MusicTypes.metal.ToString()),
            CurrentPosition = new Vector2(5, 5)
        };
        enemy1.Template = new Template()
        {
            MusicType = new GameType(MusicTypes.metal.ToString()),
            GameType = new GameType(EntityTypes.enemy.ToString())
        };

        Connection enemy2 = new Connection();
        enemy2.Data = new Data()
        {
            CurrentMusicType = new GameType(MusicTypes.metal.ToString()),
            CurrentPosition = new Vector2(5, 5)
        };
        enemy2.Template = new Template()
        {
            MusicType = new GameType(MusicTypes.metal.ToString()),
            GameType = new GameType(EntityTypes.enemy.ToString())
        };

        enemies.Connections[0] = enemy1;
        enemies.Connections[1] = enemy2;

        string jsonEnemies = JsonUtility.ToJson(enemies, true);
        File.WriteAllText(Application.dataPath + CHARACTERS_PATH + ENEMIES, jsonEnemies);
    }

    private void TestSavePlayer()
    {
        Connection player = new Connection();
        player.Data = new Data() { CurrentMusicType = new GameType(MusicTypes.classic.ToString()), 
                                CurrentPosition = new Vector2(0, 0) };
        player.Template = new Template() { MusicType = new GameType(MusicTypes.classic.ToString()),
                                        GameType = new GameType(EntityTypes.player.ToString()) };

        string jsonPlayer = JsonUtility.ToJson(player, true);
        File.WriteAllText(Application.dataPath + CHARACTERS_PATH + PLAYER, jsonPlayer);
    }

    private void Load()
    {
        _bus.Publish(new LoadEnemiesMessage(this, GetEnemies()));
        _bus.Publish(new LoadPlayerMessage(this, GetPlayer()));
    }

    private TileMap GetTiles()
    {
        TextAsset m = Resources.Load<TextAsset>(MAP);
        TileMap map = JsonUtility.FromJson<TileMap>(m.text);
        return map;
    }

    private GameEntity GetPlayer()
    {
        GameEntity ent = new GameEntity(new GameType("player"));
        Connection con = JsonUtility.FromJson<Connection>(File.ReadAllText(Application.dataPath + CHARACTERS_PATH + PLAYER));
        ent.AddModule<PlayerModule>(new PlayerModule(ent, _bus, con.Data, con.Template));
        return ent;
    }

    private IEnumerable<GameEntity> GetEnemies()
    {
        List<GameEntity> result = new List<GameEntity>();

        Enemies enemies = JsonUtility.FromJson<Enemies>(File.ReadAllText(Application.dataPath + CHARACTERS_PATH + ENEMIES));
        foreach (Connection con in enemies.Connections)
        {
            GameEntity enemy = new GameEntity(con.Template.GameType);
            enemy.AddModule<EnemyModule>(new EnemyModule(enemy, _bus, con.Data, con.Template));
            result.Add(enemy);
        }

        return result;
    }
	
}
