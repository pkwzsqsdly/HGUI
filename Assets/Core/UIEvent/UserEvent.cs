﻿using huqiang;
using huqiang.Core.HGUI;
using huqiang.Data;
using System;
using UnityEngine;

namespace huqiang.UIEvent
{
    public class UserEvent
    {
        public static long ClickTime = 1800000;
        public static float ClickArea = 400;
        internal static void DispatchEvent(UserAction action, HGUIElement[] pipeLine)
        {
            HGUIElement root = pipeLine[0];
            if (root.script != null)
            {
                 int c = root.childCount;
                for (int i = c; i >0; i--)
                {
                    try
                    {
                        if (DispatchEvent(pipeLine, i, Vector3.zero, Vector3.one, Quaternion.identity, action))
                            return;
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pipeLine">所有UI</param>
        /// <param name="index"></param>
        /// <param name="pos">父级位置</param>
        /// <param name="scale">父级大小</param>
        /// <param name="quate">父级旋转</param>
        /// <param name="action">用户操作指令</param>
        /// <returns></returns>
        static bool DispatchEvent(HGUIElement[] pipeLine, int index,Vector3 pos,Vector3 scale,Quaternion quate, UserAction action)
        {
            if (!pipeLine[index].active)
                return false;
            Vector3 p = quate * pipeLine[index].localPosition;
            Vector3 o = Vector3.zero;
            o.x = p.x * scale.x;
            o.y = p.y * scale.y;
            o.z = p.z * scale.z;
            o += pos;
            Vector3 s = pipeLine[index].localScale;
            Quaternion q = quate * pipeLine[index].localRotation;
            s.x *= scale.x;
            s.y *= scale.y;
            var script = pipeLine[index].script;
            if (script != null)
            {
                var ue = script.userEvent;
                if (ue == null)
                {
                    int c = pipeLine[index].childCount;
                    if(c > 0)
                    {
                        int os = pipeLine[index].childOffset + c;
                        for (int i = 0; i < c; i++)
                        {
                            os--;
                            if (DispatchEvent(pipeLine, os, o, s, q, action))
                            {
                                return true;
                            }
                        }
                    }
                }
                else if (ue.forbid)
                {
                    int c = pipeLine[index].childCount;
                    if (c > 0)
                    {
                        int os = pipeLine[index].childOffset + c;
                        for (int i = 0; i < c; i++)
                        {
                            os--;
                            if (DispatchEvent(pipeLine, os, o, s, q, action))
                            {
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    ue.pgs = scale;
                    ue.GlobalScale = s;
                    ue.GlobalPosition = o;
                    ue.GlobalRotation = q;
                    bool inside = false;
                    float w, h;
                    if (ue.UseAssignSize)
                    {
                        w = ue.boxSize.x * s.x;
                        h = ue.boxSize.y * s.y;
                    }
                    else
                    {
                        w = script.SizeDelta.x * s.x;
                        h = script.SizeDelta.y * s.y;
                    }
                    if (ue.IsCircular)
                    {
                        float x = action.CanPosition.x - o.x;
                        float y = action.CanPosition.y - o.y;
                        w *= 0.5f;
                        if (x * x + y * y < w * w)
                            inside = true;
                    }
                    else
                    {
                        float px = script.Pivot.x;
                        float py = script.Pivot.y;
                        float lx = -px * w;
                        float rx = lx + w;
                        float dy = -py * h;
                        float ty = dy + h;

                        var v = action.CanPosition;
                        var Rectangular = ue.Rectangular;
                        Rectangular[0] = q * new Vector3(lx, dy) + o;
                        Rectangular[1] = q * new Vector3(lx, ty) + o;
                        Rectangular[2] = q * new Vector3(rx, ty) + o;
                        Rectangular[3] = q * new Vector3(rx, dy) + o;
                        inside = huqiang.Physics2D.DotToPolygon(Rectangular, v);
                    }
                    if (inside)
                    {
                        action.CurrentEntry.Add(ue);
                        int c = pipeLine[index].childCount;
                        if (c > 0)
                        {
                            int os = pipeLine[index].childOffset + c;
                            for (int i = 0; i < c; i++)
                            {
                                os--;
                                if (DispatchEvent(pipeLine, os, o, s, q, action))
                                {
                                    if (ue.ForceEvent)
                                    {
                                        if (!ue.forbid)
                                            break;
                                    }
                                    return true;
                                }
                            }
                        }
                        if (action.IsLeftButtonDown | action.IsRightButtonDown | action.IsMiddleButtonDown)
                        {
                            ue.OnMouseDown(action);
                        }
                        else if (action.IsLeftButtonUp | action.IsRightButtonUp | action.IsMiddleButtonUp)
                        {
                            ue.OnMouseUp(action);
                        }
                        else
                        {
                            ue.OnMouseMove(action);
                        }
                        if (ue.Penetrate)
                            return false;
                        return true;
                    }
                    else if (!ue.CutRect)
                    {
                        int c = pipeLine[index].childCount;
                        if (c > 0)
                        {
                            int os = pipeLine[index].childOffset + c;
                            for (int i = 0; i < c; i++)
                            {
                                os--;
                                if (DispatchEvent(pipeLine, os, o, s, q, action))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                int c = pipeLine[index].childCount;
                if (c > 0)
                {
                    int os = pipeLine[index].childOffset + c;
                    for (int i = 0; i < c; i++)
                    {
                        os--;
                        if (DispatchEvent(pipeLine, os, o, s, q, action))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        int xTime;
        int yTime;
        float lastX;
        float lastY;
        public Vector2 boxSize;
        Vector2 maxVelocity;
        Vector2 sDistance;
        Vector2 mVelocity;
        protected Vector3 pgs = Vector3.one;
        public Vector3 GlobalScale = Vector3.one;
        public Vector3 GlobalPosition;
        public Quaternion GlobalRotation;
        public float ScrollDistanceX
        {
            get { return sDistance.x; }
            set
            {
                if (value == 0)
                    maxVelocity.x = 0;
                else
                    maxVelocity.x = MathH.DistanceToVelocity(DecayRateX, value);
                mVelocity.x = maxVelocity.x;
                sDistance.x = value;
                xTime = 0;
                lastX = 0;
            }
        }
        public float ScrollDistanceY
        {
            get { return sDistance.y; }
            set
            {
                if (value == 0)
                    maxVelocity.y = 0;
                else
                    maxVelocity.y = MathH.DistanceToVelocity(DecayRateY, value);
                mVelocity.y = maxVelocity.y;
                sDistance.y = value;
                yTime = 0;
                lastY = 0;
            }
        }
        public float DecayRateX = 0.998f;
        public float DecayRateY = 0.998f;
        public float speed = 1f;
        public Vector2 RawPosition { get; protected set; }
        Vector2 LastPosition;
        public int HoverTime { get; protected set; }
        public long PressTime { get; internal set; }
        public long EntryTime { get; protected set; }
        public long StayTime { get; protected set; }
        public bool Pressed { get; internal set; }
        public Vector3[] Rectangular { get; private set; }
        public float VelocityX { get { return mVelocity.x; } set { maxVelocity.x = mVelocity.x = value; RefreshRateX(); } }
        public float VelocityY { get { return mVelocity.y; } set { maxVelocity.y = mVelocity.y = value; RefreshRateY(); } }
        public bool forbid;
        /// <summary>
        /// 开启此项,范围外不会把事件传给子组件
        /// </summary>
        public bool CutRect = false;
        /// <summary>
        /// 强制事件不被子组件拦截
        /// </summary>
        public bool ForceEvent = false;
        /// <summary>
        /// 允许事件穿透
        /// </summary>
        public bool Penetrate = false;
        /// <summary>
        ///  使用指定尺寸
        /// </summary>
        public bool UseAssignSize = false;

        public bool IsCircular = false;
        public bool entry { get; protected set; }
        private int index;
        public bool AutoColor = true;
        internal Color g_color;
        public object DataContext;

        public Action<UserEvent, UserAction> PointerDown;
        public Action<UserEvent, UserAction> PointerUp;
        public Action<UserEvent, UserAction> Click;
        public Action<UserEvent, UserAction> PointerEntry;
        public Action<UserEvent, UserAction> PointerMove;
        public Action<UserEvent, UserAction> PointerLeave;
        public Action<UserEvent, UserAction> PointerHover;
        public Action<UserEvent, UserAction> MouseWheel;
        public Action<UserEvent, UserAction, Vector2> Drag;
        public Action<UserEvent, UserAction, Vector2> DragEnd;
        public Action<UserEvent, Vector2> Scrolling;
        public Action<UserEvent> ScrollEndX;
        public Action<UserEvent> ScrollEndY;
        public Action<UserEvent, UserAction> LostFocus;

        UserAction FocusAction;
        public UIElement Context;
        public UserEvent()
        {
            Rectangular = new Vector3[4];
        }
        void RefreshRateX()
        {
            xTime = 0;
            lastX = 0;
            if (maxVelocity.x == 0)
                sDistance.x = 0;
            else
                sDistance.x = (float)MathH.PowDistance(DecayRateX, maxVelocity.x, 1000000);
        }
        void RefreshRateY()
        {
            yTime = 0;
            lastY = 0;
            if (maxVelocity.y == 0)
                sDistance.y = 0;
            else
                sDistance.y = (float)MathH.PowDistance(DecayRateY, maxVelocity.y, 1000000);
        }
        public Vector3 ScreenToLocal(Vector3 v)
        {
            v -= GlobalPosition;
            if (GlobalScale.x != 0)
                v.x /= GlobalScale.x;
            else v.x = 0;
            if (GlobalScale.y != 0)
                v.y /= GlobalScale.y;
            else v.y = 0;
            if (GlobalScale.z != 0)
                v.z /= GlobalScale.z;
            else v.z = 0;
            var q = Quaternion.Inverse(GlobalRotation);
            v = q * v;
            return v;
        }
        public virtual void OnMouseDown(UserAction action)
        {
            if (!action.MultiFocus.Contains(this))
                action.MultiFocus.Add(this);
            Pressed = true;
            PressTime = action.EventTicks;
            RawPosition = action.CanPosition;
            if (AutoColor)
            {
                Color a;
                a.r = g_color.r * 0.8f;
                a.g = g_color.g * 0.8f;
                a.b = g_color.b * 0.8f;
                a.a = g_color.a;
                Context.MainColor = a;
            }
            if (PointerDown != null)
                PointerDown(this, action);
            entry = true;
            FocusAction = action;
            mVelocity = Vector2.zero;
        }
        protected virtual void OnMouseUp(UserAction action)
        {
            entry = false;
            if (AutoColor)
            {
                if (!forbid)
                    Context.MainColor = g_color;
            }
            bool press = Pressed;
            Pressed = false;
            if (PointerUp != null)
                PointerUp(this, action);
            if (press)
            {
                long r = DateTime.Now.Ticks - PressTime;
                if (r <= ClickTime)
                {
                    float x = RawPosition.x - action.CanPosition.x;
                    float y = RawPosition.y - action.CanPosition.y;
                    x *= x;
                    y *= y;
                    x += y;
                    if (x < ClickArea)
                        OnClick(action);
                }
            }
        }

        protected virtual void OnMouseMove(UserAction action)
        {
            if (!entry)
            {
                entry = true;
                EntryTime = UserAction.Ticks;
                if (PointerEntry != null)
                    PointerEntry(this, action);
                LastPosition = action.CanPosition;
            }
            else
            {
                StayTime = action.EventTicks - EntryTime;
                if (action.CanPosition == LastPosition)
                {
                    HoverTime += UserAction.TimeSlice * 10000;
                    if (HoverTime > ClickTime)
                        if (PointerHover != null)
                            PointerHover(this, action);
                }
                else
                {
                    HoverTime = 0;
                    LastPosition = action.CanPosition;
                    if (PointerMove != null)
                        PointerMove(this, action);
                }
            }
        }
        internal virtual void OnMouseLeave(UserAction action)
        {
            entry = false;
            if (AutoColor)
            {
                if (!forbid)
                    Context.MainColor = g_color;
            }
            if (PointerLeave != null)
                PointerLeave(this, action);
        }
        internal virtual void OnFocusMove(UserAction action)
        {
            if (Pressed)
                OnDrag(action);
        }
        protected virtual void OnDrag(UserAction action)
        {
            if (action.CanPosition != action.LastPosition)
                if (Drag != null)
                {
                    var v = action.Motion;
                    v.x /= pgs.x;
                    v.y /= pgs.y;
                    Drag(this, action, v);
                }
        }
        internal virtual void OnDragEnd(UserAction action)
        {
            if (Scrolling != null)
            {
                var v = action.Velocities;
                v.x /= GlobalScale.x;
                v.y /= GlobalScale.y;
                maxVelocity = mVelocity = v;
                RefreshRateX();
                RefreshRateY();
            }
            if (DragEnd != null)
            {
                var v = action.Motion;
                v.x /= pgs.x;
                v.y /= pgs.y;
                DragEnd(this, action, v);
            }
        }
        internal virtual void OnLostFocus(UserAction action)
        {
            FocusAction = null;
            if (LostFocus != null)
                LostFocus(this, action);
        }
        internal virtual void OnMouseWheel(UserAction action)
        {
            if (MouseWheel != null)
                MouseWheel(this, action);
        }
        internal virtual void OnClick(UserAction action)
        {
            if (Click != null)
                Click(this,action);
        }
        public void Initi(FakeStruct mod)
        {
            Initial(mod);
        }
        internal virtual void Initial(FakeStruct mod)
        {
            
        }
        public void RemoveFocus()
        {
            if (FocusAction != null)
            {
                Pressed = false;
                FocusAction.RemoveFocus(this);
                FocusAction = null;
            }
        }
        public void Dispose()
        {
            RemoveFocus();
        }
        internal virtual void Update()
        {
            if (!forbid)
                if (!Pressed)
                    DuringSlide(this);
        }
        static void DuringSlide(UserEvent back)
        {
            if (back.mVelocity.x == 0 & back.mVelocity.y == 0)
                return;
            back.xTime += UserAction.TimeSlice;
            back.yTime += UserAction.TimeSlice;
            float x = 0, y = 0;
            bool endx = false, endy = false;
            if (back.mVelocity.x != 0)
            {
                float t = (float)MathH.PowDistance(back.DecayRateX, back.maxVelocity.x, back.xTime);
                x = t - back.lastX;
                back.lastX = t;
                float vx = Mathf.Pow(back.DecayRateX, back.xTime) * back.maxVelocity.x;
                if (vx < 0.001f & vx > -0.001f)
                {
                    back.mVelocity.x = 0;
                    endx = true;
                }
                else back.mVelocity.x = vx;
            }
            if (back.mVelocity.y != 0)
            {
                float t = (float)MathH.PowDistance(back.DecayRateY, back.maxVelocity.y, back.yTime);
                y = t - back.lastY;
                back.lastY = t;
                float vy = Mathf.Pow(back.DecayRateY, back.yTime) * back.maxVelocity.y;
                if (vy < 0.001f & vy > -0.001f)
                {
                    back.mVelocity.y = 0;
                    endy = true;
                }
                else back.mVelocity.y = vy;
            }
            if (back.Scrolling != null)
                back.Scrolling(back, new Vector2(x, y));
            if (endx)
                if (back.ScrollEndX != null)
                    back.ScrollEndX(back);
            if (endy)
                if (back.ScrollEndY != null)
                    back.ScrollEndY(back);
        }
    }
}
