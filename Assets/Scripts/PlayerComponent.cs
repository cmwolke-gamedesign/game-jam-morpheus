using UnityEngine;
using System.Collections;
using Assets.Scripts.entity;
using Assets.Scripts.manager;
using System.Collections.Generic;
using UnityStandardAssets.Characters.ThirdPerson;
using TinyMessenger;
using Assets.Scripts.message.custom;
using UnityEngine.Audio;
using UnityStandardAssets.Utility;
using Assets.Scripts.entity.modules;
using Assets.Scripts;
using UnityEngine.UI;

public class PlayerComponent : MonoBehaviour 
{
    public GameObject EffectContainer;
    public Animator SpriteAnimator;

    public Image[] StateImages; 

    public Light Spotlight;
    public AudioSource AudioSource;
    public float MorphSoundDelayed;
    public SpriteRenderer HeadSprite;
    public MeterFillScript UiFillBar;
    public float CoolDownTimer;
    public AudioSource tutorial_voice;

    [HideInInspector]
    public ParticleSystem activeAttack;
    public bool isWalking;
    private bool falling = false;

    private IEntityManager _entityManager;
    private Dictionary<AudioClip, float> _activeAudioSources;
    private IMessageBus _bus;
    private bool musicIsDropped = false;
    private bool PlayerIsInMusicBubble;
    private bool forceSwitch = false;
    private bool GameStarted = false;
    private Time _startTime;

    private MusicTypes _activeMusikType;
    private AudioMixer _mixer;
    private IEnumerator _startCoroutine;

    private GameEntity _gameEntity;
    public GameEntity GameEntity
    {
        get { return _gameEntity; }
        set { _gameEntity = value; Refresh(); }
    }

    public void Start ()
    {
        _bus = Initialiser.Instance.GetService<IMessageBus>();
        _bus.Subscribe<PlayerChangedMusikTypeMessage>(OnSwitchType);
        _activeAudioSources = new Dictionary<AudioClip, float>();
        _mixer = Resources.Load<AudioMixer>("Audio/Master");

        _gameEntity = new GameEntity(new GameType(EntityTypes.player.ToString()));
        _gameEntity.AddModule<PlayerModule>(new PlayerModule(_gameEntity, _bus, new Data() { CurrentMusicType = new GameType(MusicTypes.neutral.ToString()) }, new Template()));

        CoolDownTimer = 13;
        _startCoroutine = FirstMusicChangeAfterIntro(CoolDownTimer);
        StartCoroutine(_startCoroutine);
        foreach (Image i in StateImages)
        {
            i.color = Color.grey;
        }
    }

    private void Update()
    {
        if (!SpriteAnimator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
            SpriteAnimator.SetBool("isWalking", isWalking);

        if (transform.position.y < -10)
            _bus.Publish(new GameOverMessage(this));

        if (PlayerIsInMusicBubble)
        {
            if (_gameEntity.GetModule<PlayerModule>().BaseData.MusicHealthMeter <=100)
                _gameEntity.GetModule<PlayerModule>().BaseData.MusicHealthMeter += 0.1f;
            UiFillBar.increaseByAmount(0.1f);
        }
        else
        {
            if (_gameEntity.GetModule<PlayerModule>().BaseData.MusicHealthMeter >= 0)
                _gameEntity.GetModule<PlayerModule>().BaseData.MusicHealthMeter -= 0.1f;
            UiFillBar.reduceByAmount(0.1f);
        }
        if (CoolDownTimer >= 0)
            CoolDownTimer -= Time.deltaTime;
        else
        {
            foreach (Image i in StateImages)
            {
                i.color = Color.white;
            }
        }
        if (transform.position.y <= -15 && falling == false) { 
            transform.GetComponentInParent<Animation>().Play("falling");
            falling = true;    
        }
    }
    private void Refresh()
    {
    }
    private void SwitchType(MusicTypes musicType)
    {
        Debug.Log(CoolDownTimer);
        if (musicType != _activeMusikType && CoolDownTimer < 0 || forceSwitch)
        {
            Refresh();

            GameEntity.GetModule<PlayerModule>().BaseData.CurrentMusicType.Value = musicType.ToString();

            if (AudioSource.clip != null && !_activeAudioSources.ContainsKey(AudioSource.clip))
                _activeAudioSources.Add(AudioSource.clip, AudioSource.time);
            else if (AudioSource.clip != null)
                _activeAudioSources[AudioSource.clip] = AudioSource.time;

            AudioSource.Stop();
            switchAnimation(_activeMusikType, musicType);

            if (musicType == MusicTypes.metal)
            {
                Spotlight.gameObject.SetActive(true);
                Spotlight.color = Color.blue;
                foreach (Light sp in Spotlight.GetComponentsInChildren<Light>())
                {
                    sp.color = new Color(0.0f, 0.082f, 1.0f, 1.0f);
                }
                AudioSource.clip = Resources.Load<AudioClip>("Audio/music/metal");
                AudioSource.outputAudioMixerGroup = _mixer.FindMatchingGroups("music_metal")[0];
                StateImages[0].color = Color.white;
                StateImages[1].color = StateImages[2].color = Color.grey;
                GetComponent<TopDownUserControl>().MovementSpeed = 30;
            }
            if (musicType == MusicTypes.classic)
            {
                Spotlight.gameObject.SetActive(true);
                Spotlight.color = Color.red;
                foreach (Light sp in Spotlight.GetComponentsInChildren<Light>())
                {
                    sp.color = new Color(0.992f, 0.102f, 0.102f, 1.0f);
                }
                AudioSource.clip = Resources.Load<AudioClip>("Audio/music/classic");
                AudioSource.outputAudioMixerGroup = _mixer.FindMatchingGroups("music_classic")[0];
                StateImages[1].color = Color.white;
                StateImages[0].color = StateImages[2].color = Color.grey;
                GetComponent<TopDownUserControl>().MovementSpeed = 20;
            }
            if (musicType == MusicTypes.techno)
            {
                Spotlight.gameObject.SetActive(true);
                Spotlight.color = Color.green;
                foreach (Light sp in Spotlight.GetComponentsInChildren<Light>())
                {
                    sp.color = new Color(0.176f, 0.659f, 0.176f, 1.0f);
                }
                AudioSource.clip = Resources.Load<AudioClip>("Audio/music/electro");
                AudioSource.outputAudioMixerGroup = _mixer.FindMatchingGroups("music_techno")[0];
                StateImages[2].color = Color.white;
                StateImages[1].color = StateImages[0].color = Color.grey;
                GetComponent<TopDownUserControl>().MovementSpeed = 40;
            }
            if(musicType == MusicTypes.neutral)
            {
                Spotlight.gameObject.SetActive(false);
            }
            _activeMusikType = musicType;
            InstantiateParticleEffect(musicType);

            GetAudioSourceTime();

            _gameEntity.GetModule<PlayerModule>().BaseData.MusicHealthMeter = 0;
            UiFillBar.setFillAmount(0);
            CoolDownTimer = 3;

        }
    }

    public void OnSwitchType(PlayerChangedMusikTypeMessage msg)
    {
        SwitchType(msg.Type);
    }
    private void GetAudioSourceTime()
    {
        float audioStartTime;
        if (_activeAudioSources.TryGetValue(AudioSource.clip, out audioStartTime))
        {
            AudioSource.Play();
        }
        else if (!AudioSource.isPlaying)
        {
            AudioSource.Play();
        }
    }

    private void switchAnimation(MusicTypes lastType, MusicTypes activeType)
    {
        Debug.Log(lastType.ToString() + "_to_" + activeType.ToString());
        SpriteAnimator.SetTrigger(lastType.ToString() + "_to_" + activeType.ToString());
        PlayMusicSwitchSound();
        Sprite sprite = Resources.Load<Sprite>("Animation/"+ activeType);
        Debug.Log(SpriteAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"));
        if (sprite != null && SpriteAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") || SpriteAnimator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
            StartCoroutine(LateSetSpriteAfterAnimation(sprite));
    }

    private void PlayMusicSwitchSound()
    {
        AudioSource warpSource = Instantiate<AudioSource>(Resources.Load<AudioSource>("Audio/AudioSource"));
        warpSource.clip = Resources.Load<AudioClip>("Audio/sfx/warp");
        warpSource.outputAudioMixerGroup = _mixer.FindMatchingGroups("sfx")[0];
        warpSource.transform.SetParent(gameObject.transform);
        if (!warpSource.isPlaying)
            warpSource.Play();
        StartCoroutine(DestroyObjectAfterTime(warpSource.gameObject));
    }

    private void InstantiateParticleEffect(MusicTypes type)
    {
        if(activeAttack != null)
            Destroy(activeAttack.gameObject);
        ParticleSystem resource = Resources.Load<ParticleSystem>("Effects/Attacks/" + type.ToString());
        activeAttack = Instantiate<ParticleSystem>(resource);
        activeAttack.transform.SetParent(EffectContainer.transform);
        activeAttack.transform.localEulerAngles = new Vector3(270, 0, 0);
        activeAttack.transform.localPosition = Vector3.zero;

    }
    public void FollowMouse()
    {
        transform.rotation = GetMousePosition();
        transform.localEulerAngles = new Vector3(90, transform.localEulerAngles.y, 0);
    }

    public void MoveForeward(float move)
    {
        if (GameStarted)
        {
            float distance = Vector3.Distance(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (distance > 5.5f)
            {
                transform.Translate(transform.forward * move * Time.fixedDeltaTime);
            }
        }
    }
    public void MoveSidewards(float move)
    {
        if(GameStarted)
            transform.Translate(new Vector3(1, 0, 0) * move * Time.fixedDeltaTime);
    }

    private Quaternion GetMousePosition()
    {
        Vector3 mousePosition = Input.mousePosition;

        Vector3 targetPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector3 relativePos = targetPosition - transform.position;
        relativePos.y = 90;
        Quaternion rotation = Quaternion.LookRotation(relativePos);
        return rotation;
    }
    public void ToggleDropMusic()
    {
        if (!musicIsDropped)
        {
            Spotlight.GetComponent<FollowTarget>().target = null;
            musicIsDropped = true;
        }
        else if(PlayerIsInMusicBubble)
        {
            Spotlight.GetComponent<FollowTarget>().target = transform;
            Spotlight.transform.position = transform.position;
            musicIsDropped = false;
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        EnemyComponent com = collision.gameObject.GetComponent<EnemyComponent>();
        if (com == null) return;
        if (com.GameEntity.GetModule<EnemyModule>().BaseData.CurrentMusicType.Value != GameEntity.GetModule<PlayerModule>().BaseData.CurrentMusicType.Value)
        {
            float health = _gameEntity.GetModule<PlayerModule>().BaseData.MusicHealthMeter - 10.0f;
            UiFillBar.reduceByAmount(10.0f);
            if (health <= 0)
                _bus.Publish(new GameOverMessage(this));
            _gameEntity.GetModule<PlayerModule>().BaseData.MusicHealthMeter = health;

            UiFillBar.score -= 10.0f;
        }
    }

    IEnumerator DestroyObjectAfterTime(GameObject obj)
    {
        yield return new WaitForSeconds(1);
        Destroy(obj);
    }
    IEnumerator LateSetSpriteAfterAnimation(Sprite sprite)
    {
        HeadSprite.gameObject.SetActive(false); 
        yield return new WaitForSeconds(SpriteAnimator.GetCurrentAnimatorClipInfo(0).Length + 0.75f);
        HeadSprite.gameObject.SetActive(true); 
        HeadSprite.sprite = sprite;
    }
    IEnumerator FirstMusicChangeAfterIntro(float duration)
    {
        yield return new WaitForSeconds(duration);
        forceSwitch = true;
        SwitchType(MusicTypes.classic);
        forceSwitch = false;
        UiFillBar.StartScoreDown = true;
        GameStarted = true;
    }

    void OnTriggerStay(Collider other)
    {
        if(!PlayerIsInMusicBubble)
            PlayerIsInMusicBubble = true;
    }
    void OnTriggerExit(Collider other)
    {
        if(PlayerIsInMusicBubble)
            PlayerIsInMusicBubble = false;
    }
    public void SkipIntro()
    {
        tutorial_voice.Stop();
        CoolDownTimer = 0.1f;
        StopCoroutine(_startCoroutine);
        StartCoroutine(FirstMusicChangeAfterIntro(CoolDownTimer));
    }

}
