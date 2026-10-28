using UnityEngine;
using UnityEngine.UI;

namespace PixelForge
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        static readonly Color Background = new Color(0.035f, 0.055f, 0.11f);
        static readonly Color Cyan = new Color(0.18f, 0.92f, 0.94f);
        static readonly Color Pink = new Color(1f, 0.28f, 0.52f);

        Camera cam;
        Transform player;
        Text scoreText;
        Text hintText;
        int score;
        float spawnTimer;
        bool ended;

        void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;
            BuildWorld();
        }

        void BuildWorld()
        {
            cam = Camera.main;
            cam.orthographic = true;
            cam.orthographicSize = 5.8f;
            cam.transform.position = new Vector3(0, 0, -10);
            cam.backgroundColor = Background;

            var playerObj = MakeShape("Explorer", new Vector2(0, -3.45f), new Vector2(.72f, .72f), Cyan);
            player = playerObj.transform;
            playerObj.AddComponent<CircleCollider2D>().isTrigger = true;
            var relay = playerObj.AddComponent<PlayerCollisionRelay>();
            relay.Owner = this;

            MakeShape("Left boundary", new Vector2(-3.35f, 0), new Vector2(.08f, 10f), new Color(.12f,.2f,.34f));
            MakeShape("Right boundary", new Vector2(3.35f, 0), new Vector2(.08f, 10f), new Color(.12f,.2f,.34f));

            var canvasObject = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scoreText = MakeText(canvas.transform, "SHARDS  0", 34, new Vector2(.5f, .94f), new Vector2(0, -50), Cyan);
            hintText = MakeText(canvas.transform, "DRAG TO MOVE  •  COLLECT CYAN  •  DODGE PINK", 20, new Vector2(.5f, .88f), new Vector2(0, -25), Color.white);
        }

        GameObject MakeShape(string name, Vector2 position, Vector2 scale, Color color)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            go.transform.localScale = scale;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSprite();
            renderer.color = color;
            return go;
        }

        Sprite RuntimeSprite()
        {
            return Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        }

        Text MakeText(Transform parent, string value, int size, Vector2 anchor, Vector2 offset, Color color)
        {
            var go = new GameObject("HUD Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(1000, 70);
            rect.anchoredPosition = offset;
            var text = go.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            return text;
        }

        void Update()
        {
            if (ended)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.touchCount > 0)
                    UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
                return;
            }

            MovePlayer();
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0)
            {
                SpawnFallingObject();
                spawnTimer = Random.Range(.45f, .9f);
            }

            foreach (var item in FindObjectsByType<FallingObject>(FindObjectsSortMode.None))
            {
                item.transform.Translate(Vector3.down * item.Speed * Time.deltaTime, Space.World);
                if (item.transform.position.y < -6.5f) Destroy(item.gameObject);
            }
        }

        void MovePlayer()
        {
            Vector3 world = Vector3.zero;
            bool hasPointer = false;
            if (Input.touchCount > 0)
            {
                world = cam.ScreenToWorldPoint(Input.GetTouch(0).position);
                hasPointer = true;
            }
            else if (Input.GetMouseButton(0))
            {
                world = cam.ScreenToWorldPoint(Input.mousePosition);
                hasPointer = true;
            }
            if (hasPointer)
            {
                var targetX = Mathf.Clamp(world.x, -2.8f, 2.8f);
                player.position = new Vector3(Mathf.Lerp(player.position.x, targetX, Time.deltaTime * 16f), player.position.y, 0);
            }
        }

        void SpawnFallingObject()
        {
            bool hazard = Random.value < .3f;
            var go = MakeShape(hazard ? "Hazard" : "Shard",
                new Vector2(Random.Range(-2.75f, 2.75f), 6f),
                hazard ? new Vector2(.58f,.58f) : new Vector2(.34f,.34f),
                hazard ? Pink : Cyan);
            go.AddComponent<CircleCollider2D>().isTrigger = true;
            var falling = go.AddComponent<FallingObject>();
            falling.Speed = Random.Range(2.7f, 4.4f) + score * .025f;
            falling.Hazard = hazard;
        }

        public void HandleCollision(Collider2D other)
        {
            var falling = other.GetComponent<FallingObject>();
            if (falling == null || ended) return;
            if (falling.Hazard)
            {
                ended = true;
                hintText.text = "OOPS! TAP TO PLAY AGAIN";
                hintText.color = Pink;
            }
            else
            {
                score++;
                scoreText.text = "SHARDS  " + score;
                Destroy(other.gameObject);
            }
        }
    }

    public sealed class PlayerCollisionRelay : MonoBehaviour
    {
        public GameBootstrap Owner;
        void OnTriggerEnter2D(Collider2D other) => Owner?.HandleCollision(other);
    }

    public sealed class FallingObject : MonoBehaviour
    {
        public float Speed = 3f;
        public bool Hazard;
    }
}
