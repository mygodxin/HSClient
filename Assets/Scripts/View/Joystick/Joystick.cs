using System;
using UnityEngine;
using UnityEngine.EventSystems;

public enum Direction
{
    Both,
    Horizontal,
    Vertical
}

public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private  bool _isDraging = false;
    private int _fingerId;
    private Vector2 _pointerDownPosition;
    private Vector2 _backgroundOriginLocalPostion;
    public bool dynamic;
    public Transform background;
    public Transform dot;
    public Transform arrow;
    public Direction activatedAxis;
    public bool showDirectionArrow;
    public float maxRadius = 38; //ҡ���ƶ����뾶

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

    public void OnPointerDown(PointerEventData eventData)
    {
        //���� Touch��ֻ��Ӧһ��Touch��������꣺ֻ��Ӧ���
        if (eventData.pointerId < -1 || this._isDraging) return;
        _fingerId = eventData.pointerId;
        _pointerDownPosition = eventData.position;
        if (dynamic)
        {
            _pointerDownPosition[2] = eventData.pressEventCamera?.WorldToScreenPoint(background.position).z ?? background.position.z;
            background.position = eventData.pressEventCamera?.ScreenToWorldPoint(_pointerDownPosition) ?? _pointerDownPosition; ;
        }
        if (this.onPointerDown != null)
            this.onPointerDown.Invoke(eventData.position);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (_fingerId != eventData.pointerId) return;//��ȷ����ָ̧��ʱ�Ż�����ҡ�ˣ�
        RestJoystick();

        if (this.onPointerUp != null)
            this.onPointerUp.Invoke(eventData.position);
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (_fingerId != eventData.pointerId) return;
        Vector2 direction = eventData.position - (Vector2)_pointerDownPosition; //�õ�BackGround ָ�� Handle ������
        float radius = Mathf.Clamp(Vector3.Magnitude(direction), 0, maxRadius); //��ȡ�����������ĳ��� �Կ��� Handle �뾶
        Vector2 localPosition = new Vector2()
        {
            x = (activatedAxis == Direction.Both || activatedAxis == Direction.Horizontal) ? (direction.normalized * radius).x : 0, //ȷ���Ƿ񼤻�ˮƽ����
            y = (activatedAxis == Direction.Both || activatedAxis == Direction.Vertical) ? (direction.normalized * radius).y : 0       //ȷ���Ƿ񼤻ֱ���򣬼���͸�����
        };
        dot.localPosition = localPosition;      //���� Handle λ��
        if (showDirectionArrow)
        {
            if (!arrow.gameObject.activeInHierarchy) arrow.gameObject.SetActive(true);
            arrow.localEulerAngles = new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, localPosition));
        }
    }
    private void RestJoystick()  //����ҡ������
    {
        background.localPosition = _backgroundOriginLocalPostion;
        dot.localPosition = Vector3.zero;
        arrow.gameObject.SetActive(false);
        _fingerId = int.MinValue;
    }

    private void ConfigJoystick() //���ö�̬/��̬ҡ��
    {
        if (!dynamic) _backgroundOriginLocalPostion = background.localPosition;
        //GetComponent<Image>().raycastTarget = dynamic;
        // handle.GetComponent<Image>().raycastTarget = !dynamic;
    }
}