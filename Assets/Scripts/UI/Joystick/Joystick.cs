using System;
using UnityEngine;
using UnityEngine.EventSystems;

public enum Direction
{
    Both,
    Horizontal,
    Vertical
}
/// <summary>
/// ����ҡ��
/// </summary>
public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public bool isDraging {
        get
        {
            return this._fingerId != int.MinValue;
        }
    }
    private int _fingerId = int.MinValue;
    private Vector2 _pointerDownPosition;
    private Vector2 _backgroundOriginLocalPostion;
    //Ϊtrue backgroundλ�ø���pointerdown, Ϊfalse backgroundλ�ù̶�
    public bool dynamic;
    public Transform background;
    public Transform dot;
    public Transform arrow;
    public Direction directionAxisd;
    public bool showDirectionArrow;
    //ҡ���ƶ����뾶
    public float maxRadius = 38;

    public Action<Vector2> onPointerDown;
    public Action<Vector2> onPointerUp;
    public Action<Vector2> onPointerMove;
    void Start()
    {
        this._backgroundOriginLocalPostion = this.background.localPosition;
    }
    void Update()
    {
        if (this.onPointerMove != null)
            this.onPointerMove.Invoke(this.dot.localPosition / this.maxRadius);
    }
    void OnDisable()
    {
        this.RestJoystick();
    }
    void OnValidate()
    {
        this.ConfigJoystick();
    }
    private void ConfigJoystick()
    {
        if (!dynamic) _backgroundOriginLocalPostion = background.localPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //���� Touch��ֻ��Ӧһ��Touch��������꣺ֻ��Ӧ���
        if (eventData.pointerId < -1 || this.isDraging) return;
        _fingerId = eventData.pointerId;
        _pointerDownPosition = eventData.position;
        if (dynamic)
        {
            //_pointerDownPosition[2] = eventData.pressEventCamera?.WorldToScreenPoint(background.position).z ?? background.position.z;
            background.position = eventData.pressEventCamera?.ScreenToWorldPoint(_pointerDownPosition) ?? _pointerDownPosition;
        }
        if (this.onPointerDown != null)
            this.onPointerDown.Invoke(eventData.position);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        //��ȷ����ָ̧��ʱ�Ż�����ҡ�ˣ�
        if (_fingerId != eventData.pointerId) return;
        RestJoystick();

        if (this.onPointerUp != null)
            this.onPointerUp.Invoke(eventData.position);
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (_fingerId != eventData.pointerId) return;
        //�õ�backgroundָ��dot������
        Vector2 direction = eventData.position - _pointerDownPosition;
        //��ȡ�����������ĳ����Կ���Handle�뾶
        float radius = Mathf.Clamp(Vector3.Magnitude(direction), 0, maxRadius); 
        Vector2 localPosition = new Vector2()
        {
            //ȷ���Ƿ񼤻�ˮƽ����
            x = (directionAxisd == Direction.Both || directionAxisd == Direction.Horizontal) ? (direction.normalized * radius).x : 0,
            //ȷ���Ƿ񼤻ֱ����
            y = (directionAxisd == Direction.Both || directionAxisd == Direction.Vertical) ? (direction.normalized * radius).y : 0       
        };
        dot.localPosition = localPosition;
        if (showDirectionArrow)
        {
            if (!arrow.gameObject.activeInHierarchy) arrow.gameObject.SetActive(true);
            arrow.localEulerAngles = new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, localPosition));
        }
    }
    private void RestJoystick()
    {
        background.localPosition = _backgroundOriginLocalPostion;
        dot.localPosition = Vector3.zero;
        arrow.gameObject.SetActive(false);
        _fingerId = int.MinValue;
    }
}