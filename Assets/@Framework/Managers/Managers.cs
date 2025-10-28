using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Managers s_instance;
    private static Managers Instance { get { Init(); return s_instance; } }
    #region Contents
    private GameManager _game = new GameManager();
    private ObjectManager _object = new ObjectManager();

    public static GameManager Game { get { return Instance?._game; } }
    public static ObjectManager Object { get { return Instance?._object; } }
    #endregion

    #region Core
    private DataManager _data = new DataManager();
    private PoolManager _pool = new PoolManager();
    private ResourceManager _resource = new ResourceManager();
    private SceneManagerEx _scene = new SceneManagerEx();
    private SoundManager _sound = new SoundManager();
    private UIManager _ui = new UIManager();
    private EventManager _event = new EventManager();
    private InputManagerEx _input = new InputManagerEx();
    private RandomManager _random = new RandomManager();
    private LocalizationManager _localization = new LocalizationManager();
    private TimeManager _time = new TimeManager();
    private WebManager _web = new WebManager();

    public static DataManager Data { get { return Instance?._data; } }
    public static PoolManager Pool { get { return Instance?._pool; } }
    public static ResourceManager Resource { get { return Instance?._resource; } }
    public static SceneManagerEx Scene { get { return Instance?._scene; } }
    public static SoundManager Sound { get { return Instance?._sound; } }
    public static UIManager UI { get { return Instance?._ui; } }
    public static EventManager Event { get { return Instance?._event; } }
    public static InputManagerEx Input { get { return Instance?._input; } }
    public static RandomManager Random { get { return Instance?._random; } }
    public static LocalizationManager Localization { get { return Instance?._localization; } }
    public static WebManager Web { get { return Instance?._web; } }
    public static TimeManager Time { get { return Instance?._time; } }
    #endregion


    public static void Init()
    {
        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);

            // 초기화
            s_instance = go.GetComponent<Managers>();

            // Init
            s_instance._event.Init();
            s_instance._localization.Init();
            s_instance._web.Init();
        }
    }

    public static void Clear()
    {
        Event.Clear();
        Scene.Clear();
        UI.Clear();
        Object.Clear();
        Pool.Clear();
    }

    void Update()
    {
        Managers.Input.OnUpdate();
    }
}
