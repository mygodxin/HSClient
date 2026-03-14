using System;
using UnityEngine;
using UnityEngine.UI;

namespace HS
{
    /// <summary>
    /// 序列帧动画
    /// </summary>
    public class MovieClip : Image
    {
        /// <summary>
        /// 动画帧
        /// </summary>
        public class Frame
        {
            public Sprite texture;
            public float addDelay;
        }

        /// <summary>
        /// 图片数组 
        /// </summary>
        public Sprite[] Sprites;


        /// <summary>
        /// 
        /// </summary>
        public bool Swing;

        /// <summary>
        /// 
        /// </summary>
        public float RepeatDelay;

        /// <summary>
        /// 时间缩放系数
        /// </summary>
        public float TimeScale = 1;

        /// <summary>
        /// 忽略引擎的时间缩放系数
        /// </summary>
        public bool ignoreEngineTimeScale = false;

        public float Interval = 0.2f;
        Frame[] _frames;
        int _frameCount;
        int _frame;
        [SerializeField]
        bool _playing = true;
        int _start;
        int _end;
        int _times;
        int _endAt;
        int _status; //0-none, 1-next _loop, 2-ending, 3-ended

        float _frameElapsed; //当前帧延迟
        bool _reversed;
        int _repeatedCount;

        Action _onPlayEnd;
        public Image _image;


        protected override void Awake()
        {
            base.Awake();
        }

        public void Init()
        {
            _image = this;

            _frameCount = Sprites != null ? Sprites.Length : 0;
            _frames = new Frame[_frameCount];
            for (int i = 0; i < _frameCount; i++)
            {
                _frames[i] = new Frame();
                _frames[i].addDelay = 0;
                _frames[i].texture = Sprites[i];
            }
        }

        /// <summary>
        /// 总帧
        /// </summary>
        public Frame[] Frames
        {
            get
            {
                return _frames;
            }
            set
            {
                _frames = value;
                if (_frames == null)
                {
                    _frameCount = 0;
                    _image.sprite = null;
                    return;
                }
                _frameCount = Frames.Length;

                if (_end == -1 || _end > _frameCount - 1)
                    _end = _frameCount - 1;
                if (_endAt == -1 || _endAt > _frameCount - 1)
                    _endAt = _frameCount - 1;

                if (_frame < 0 || _frame > _frameCount - 1)
                    _frame = _frameCount - 1;
                _frameElapsed = 0;
                _repeatedCount = 0;
                _reversed = false;

                DrawFrame();
            }
        }

        /// <summary>
        /// 是否播放
        /// </summary>
        public bool Playing
        {
            get { return _playing; }
            set
            {
                if (_playing != value)
                {
                    _playing = value;
                }
            }
        }

        /// <summary>
        /// 当前帧
        /// </summary>
        public int CurrentFrame
        {
            get { return _frame; }
            set
            {
                if (_frame != value)
                {
                    if (_frames != null && value >= _frameCount)
                        value = _frameCount - 1;

                    _frame = value;
                    _frameElapsed = 0;
                    DrawFrame();
                }
            }
        }

        /// <summary>
        /// 回退
        /// </summary>
        public void Rewind()
        {
            _frame = 0;
            _frameElapsed = 0;
            _reversed = false;
            _repeatedCount = 0;
            DrawFrame();
        }

        /// <summary>
        /// 同步状态
        /// </summary>
        /// <param name="anotherMc"></param>
        public void SyncStatus(MovieClip anotherMc)
        {
            _frame = anotherMc._frame;
            _frameElapsed = anotherMc._frameElapsed;
            _reversed = anotherMc._reversed;
            _repeatedCount = anotherMc._repeatedCount;
            DrawFrame();
        }

        /// <summary>
        /// 快进
        /// </summary>
        /// <param name="time"></param>
        public void Advance(float time)
        {
            int beginFrame = _frame;
            bool beginReversed = _reversed;
            float backupTime = time;
            while (true)
            {
                float tt = Interval + _frames[_frame].addDelay;
                if (_frame == 0 && _repeatedCount > 0)
                    tt += RepeatDelay;
                if (time < tt)
                {
                    _frameElapsed = 0;
                    break;
                }

                time -= tt;

                if (Swing)
                {
                    if (_reversed)
                    {
                        _frame--;
                        if (_frame <= 0)
                        {
                            _frame = 0;
                            _repeatedCount++;
                            _reversed = !_reversed;
                        }
                    }
                    else
                    {
                        _frame++;
                        if (_frame > _frameCount - 1)
                        {
                            _frame = Mathf.Max(0, _frameCount - 2);
                            _repeatedCount++;
                            _reversed = !_reversed;
                        }
                    }
                }
                else
                {
                    _frame++;
                    if (_frame > _frameCount - 1)
                    {
                        _frame = 0;
                        _repeatedCount++;
                    }
                }

                if (_frame == beginFrame && _reversed == beginReversed) //走了一轮了
                {
                    float roundTime = backupTime - time; //这就是一轮需要的时间
                    time -= Mathf.FloorToInt(time / roundTime) * roundTime; //跳过
                }
            }

            DrawFrame();
        }

        /// <summary>
        /// 设置循环播放
        /// </summary>
        public void SetPlaySettings(Action onPlayEnd = null)
        {
            SetPlaySettings(0, -1, 0, -1, onPlayEnd);
        }

        /// <summary>
        /// 从start帧开始，播放到end帧（-1表示结尾），重复times次（0表示无限循环），循环结束后，停止在endAt帧（-1表示参数end）
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="times"></param>
        /// <param name="endAt"></param>
        /// <param name="onPlayEnd"></param>
        public void SetPlaySettings(int start, int end, int times, int endAt, Action onPlayEnd)
        {
            _start = start;
            _end = end;
            if (_end == -1 || _end > _frameCount - 1)
                _end = _frameCount - 1;
            _times = times;
            _endAt = endAt;
            if (_endAt == -1)
                _endAt = _end;
            _status = 0;
            this._frame = start;
            this._onPlayEnd = onPlayEnd;
        }

        void Update()
        {
            OnTimer();
        }

        public void OnTimer(float dt = 0)
        {
            if (!_playing || _frameCount == 0 || _status == 3)
                return;

            if (dt == 0)
            {
                if (ignoreEngineTimeScale)
                {
                    dt = Time.unscaledDeltaTime;
                    if (dt > 0.1f)
                        dt = 0.1f;
                }
                else
                    dt = Time.deltaTime;
            }
            if (TimeScale != 1)
                dt *= TimeScale;

            _frameElapsed += dt;
            float tt = Interval + _frames[_frame].addDelay;
            if (_frame == 0 && _repeatedCount > 0)
                tt += RepeatDelay;
            if (_frameElapsed < tt)
                return;

            _frameElapsed -= tt;
            if (_frameElapsed > Interval)
                _frameElapsed = Interval;

            if (Swing)
            {
                if (_reversed)
                {
                    _frame--;
                    if (_frame <= 0)
                    {
                        _frame = 0;
                        _repeatedCount++;
                        _reversed = !_reversed;
                    }
                }
                else
                {
                    _frame++;
                    if (_frame > _frameCount - 1)
                    {
                        _frame = Mathf.Max(0, _frameCount - 2);
                        _repeatedCount++;
                        _reversed = !_reversed;
                    }
                }
            }
            else
            {
                _frame++;
                if (_frame > _frameCount - 1)
                {
                    _frame = 0;
                    _repeatedCount++;
                }
            }

            if (_status == 1) //new _loop
            {
                _frame = _start;
                _frameElapsed = 0;
                _status = 0;
                DrawFrame();
            }
            else if (_status == 2) //ending
            {
                _frame = _endAt;
                _frameElapsed = 0;
                _status = 3; //ended
                DrawFrame();

                _onPlayEnd?.Invoke();
            }
            else
            {
                DrawFrame();
                if (_frame == _end)
                {
                    if (_times > 0)
                    {
                        _times--;
                        if (_times == 0)
                            _status = 2;  //ending
                        else
                            _status = 1; //new _loop
                    }
                    else if (_start != 0)
                        _status = 1; //new _loop
                }
            }
        }

        void DrawFrame()
        {
            if (_frameCount > 0)
            {
                Frame frame = _frames[_frame];
                _image.sprite = frame.texture;
            }
        }
    }
}
