using Assets.Scripts.entity;
using Assets.Scripts.entity.modules;
using Assets.Scripts.manager;
using Assets.Scripts.map;
using Assets.Scripts.message.custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyMessenger;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class MapRenderer : MonoBehaviour
    {
        private IMessageBus _bus;
        private IEntityManager _entityManger;
        private const string TILES = "Map/Tilesets/";

        public GameObject TilePrefab;
        public GameObject EnemyPrefab;
        public GameObject CharacterPrefab;
        public GameObject GoalPrefab;

        private Dictionary<string, Sprite> _tiles;
        private const float _scale = 0.02f;

        private TinyMessageSubscriptionToken _token;

        public void Awake()
        {
            _tiles = new Dictionary<string, Sprite>();
            _bus = Initialiser.Instance.GetService<IMessageBus>();
            _entityManger = Initialiser.Instance.GetService<IEntityManager>();
            _token = _bus.Subscribe<MapLoadedMessage>(OnMapLoaded);
        }

        public void OnEnable()
        {
            _bus.Publish<LoadMapMessage>(new LoadMapMessage(this));
        }

        public void OnDisable()
        {
            _bus.Unsubscribe<LoadMapMessage>(_token);
        }

        private IEnumerator<object> WaitForLoad()
        {
            yield return new WaitForSeconds(1);
            _bus.Publish<LoadMapMessage>(new LoadMapMessage(this));
        }

        private void OnMapLoaded(MapLoadedMessage msg)
        {
            List<GameEntity> enemies = new List<GameEntity>();

            Sprite[] all = Resources.LoadAll<Sprite>(TILES + msg.Map.tilesets[0].name);
            foreach (Sprite sp in all)
            {
                _tiles[sp.name] = sp;
            }

            Dictionary<int, Template> musicTemplates = new Dictionary<int,Template>();
            foreach (TileSet set in msg.Map.tilesets)
            {
                if(set.tileproperties != null)
                {
                    foreach (TileProperty tp in set.tileproperties)
                    {
                        musicTemplates[tp.id] = new Template() { GameType = new GameType(EntityTypes.tile.ToString()), MusicType = new GameType(tp.tileType) };
                    }
                    break;
                }
            }

            Dictionary<string, Template> _enemyTemplates = new Dictionary<string, Template>();
            _enemyTemplates[MusicTypes.classic.ToString()] = new Template(){ GameType = new GameType(MusicTypes.classic.ToString()), MusicType = new GameType(MusicTypes.classic.ToString()) };
            _enemyTemplates[MusicTypes.metal.ToString()] = new Template(){ GameType = new GameType(MusicTypes.metal.ToString()), MusicType = new GameType(MusicTypes.metal.ToString()) };
            _enemyTemplates[MusicTypes.techno.ToString()] = new Template(){ GameType = new GameType(MusicTypes.techno.ToString()), MusicType = new GameType(MusicTypes.techno.ToString()) };

            foreach (TileLayer layer in msg.Map.layers)
            {
                if (layer.name == "Objektebene 1")
                {
                    foreach (TiledObject to in layer.objects)
                    {
                        // add enemies based on the json file/object names
                        if (to.name.Contains("enemy"))
                        {
                            GameEntity enemy = new GameEntity(new GameType(EntityTypes.enemy.ToString()));
                            Vector3 spawnPosition = new Vector3(to.x, 860, -to.y);
                            enemy.AddModule<EnemyModule>(GetEnemyModule(enemy, _enemyTemplates[to.properties.enemyType], spawnPosition));
                            enemies.Add(enemy);

                            GameObject go = Instantiate(EnemyPrefab);
                            go.transform.position = spawnPosition *= _scale;
                            go.transform.SetParent(transform, false);
                            go.GetComponentInChildren<EnemyComponent>().GameEntity = enemy;
                        }
                        else if (to.name.Contains("START"))
                        {
                            Vector3 spawnPosition = new Vector3(to.x, 1000, -to.y);
                            CharacterPrefab.transform.position = spawnPosition *= _scale;
                            CharacterPrefab.SetActive(true);
                        }
                        else if (to.name.Contains("END"))
                        {
                            GameObject go = Instantiate(GoalPrefab);
                            Vector3 pos = new Vector3(to.x - 6 / _scale, 0, -to.y - 27 / _scale);
                            go.transform.position = pos *= _scale;
                            go.transform.GetComponent<BoxCollider>().size = new Vector3(to.width * _scale, 20, to.height * _scale);
                            go.transform.SetParent(transform, false);
                        }
                    }
                }
                else if(layer.name == "Background")
                {
                    int row = 0;
                    int column = 0;
                    for (int i = 0; i < layer.data.Length; i++)
                    {
                        column = i % msg.Map.width;
                        if (i % (msg.Map.width) == 0 && i > 0) row++;
                        int id = layer.data[i] - 1;

                        if (id >= 0)
                        {
                            GameObject go = Instantiate(TilePrefab);
                            Vector3 pos = new Vector3(column * msg.Map.tilewidth * _scale, 0.1f, -row * msg.Map.tileheight * _scale);
                            go.transform.position = pos;
                            go.transform.SetParent(transform, false);

                            string sprite = msg.Map.tilesets[0].name + "_" + id;
                            go.GetComponentInChildren<SpriteRenderer>().sprite = _tiles.ContainsKey(sprite) ? _tiles[sprite] : null;

                            GameEntity tile = new GameEntity(new GameType(EntityTypes.tile.ToString()));
                            // add tile module based on how tile is designed
                            tile.AddModule<TileModule>(GetTileModule(tile, musicTemplates.ContainsKey(id) ? musicTemplates[id] : new Template()));
                            go.GetComponentInChildren<TileComponent>().Tile = tile;
                        }

                    }
                }

            }
            _bus.Publish<LoadEnemiesMessage>(new LoadEnemiesMessage(this, enemies));
            _bus.Publish<LoadPlayerMessage>(new LoadPlayerMessage(this, _entityManger.GetEnitiesOfType(new GameType(EntityTypes.player.ToString())).First()));
        }

        private EnemyModule GetEnemyModule(GameEntity enemy, Template template, Vector3 spawnPosition)
        {
            Data data = new Data() { CurrentPosition = spawnPosition, CurrentMusicType = template.MusicType };
            EnemyModule result = new EnemyModule(enemy, _bus, data, template);
            return result;
        }

        private TileModule GetTileModule(GameEntity tile, Template template)
        {
            TileModule result = new TileModule(tile, _bus, new Data() { CurrentMusicType = template.MusicType }, template);
            return result;
        }

    }
}
