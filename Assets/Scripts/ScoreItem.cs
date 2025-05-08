using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Coin,
    Catnip,
    Fuel,
    Magnet,
    Increaser
}

public enum ItemValue
{
    Collectible = 1,
    _5 = 5,
    _25 = 25,
    _75 = 75,
    _250 = 250,
    _500 = 500,
}

public enum ItemUsage
{
    OnGround,
    InAir
}

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class ScoreItem : MonoBehaviour
{
    [Header("Item Type and Value")]
    public ItemValue Value;
    public ItemType Type;
    public ItemUsage Usage;
    public List<Sprite> CoinSprites;

    [Header("Collect Animation")]
    public Vector3 ItemMovePosition = new Vector3(1f, 1f, 0);
    public float Speed = 1f, FadeDuration = 1.5f;

    protected SpriteRenderer _spriteRenderer;
    protected Dictionary<int, Sprite> _spritesList = new Dictionary<int, Sprite>();
    protected Sprite _currentSprite;
    protected Transform _transform;
    protected CircleCollider2D _circleCol;
    protected Vector3 _itemPosition;
    protected Color _itemColor;
    protected bool _itemTaken = false;

    protected virtual void Awake()
    {
        _spritesList.Add(5, CoinSprites[0]);
        _spritesList.Add(25, CoinSprites[1]);
        _spritesList.Add(75, CoinSprites[2]);
        _spritesList.Add(250, CoinSprites[3]);
        _spritesList.Add(500, CoinSprites[4]);

        _transform = this.transform;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _circleCol = GetComponent<CircleCollider2D>();
    }

    protected virtual void Start()
    {
        if (Type == ItemType.Coin)
        {
            _currentSprite = _spritesList[(int)Value];
            _spriteRenderer.sprite = _currentSprite;
        }
        else if (Type == ItemType.Fuel)
        {
            _spriteRenderer.enabled = false;
            _circleCol.enabled = false;
        }
        _itemPosition = _transform.localPosition;
        _itemColor = _spriteRenderer.material.color;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collider2D)
    {
        if (collider2D.CompareTag("Player"))
        {
            if (Type == ItemType.Coin)
            {
                GameManager.Instance.AddPoints((int)Value);
            }
            else if (Type == ItemType.Catnip)
            {
                GameManager.Instance.AddCatnips((int)Value);
            }

            _itemTaken = true;
            _circleCol.enabled = false;

            StartCoroutine(FadeEffect.FadeSprite(_spriteRenderer, FadeDuration, new Color(0, 0, 0, 0f)));
            StartCoroutine(MoveItem(ItemMovePosition, Speed));
        }
    }

    protected IEnumerator MoveItem(Vector3 targetPos, float moveSpeed)
    {
        float t = 0;
        while (t < 1f)
        {
            _transform.Translate(targetPos * t);
            t += Time.deltaTime * moveSpeed;

            yield return null;
        }
    }

    public virtual void ResetItem()
    {
        if (_itemTaken && !(Type == ItemType.Fuel))
        {
            _spriteRenderer.material.color = _itemColor;
            _transform.localPosition = _itemPosition;
            _circleCol.enabled = true;
            _itemTaken = false;
        }
    }

    public virtual void UpgradeCoin()
    {
        if (Type == ItemType.Coin && !(Value == ItemValue._500))
        {
            var Length = System.Enum.GetNames(typeof(ItemValue)).Length;

            Value = (ItemValue)(((int)Value + 1) % Length);
            _currentSprite = _spritesList[(int)Value];
            _spriteRenderer.sprite = _currentSprite;
        }
    }

    public virtual void FuelReset()
    {
        if (Type == ItemType.Fuel)
        {
            _spriteRenderer.enabled = true;
            _spriteRenderer.material.color = _itemColor;
            _transform.localPosition = _itemPosition;
            _circleCol.enabled = true;
            _itemTaken = false;
        }
    }

}
