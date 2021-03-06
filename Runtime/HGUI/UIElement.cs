﻿using huqiang;
using huqiang.Data;
using huqiang.UIComposite;
using huqiang.UIEvent;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace huqiang.Core.HGUI
{
    public enum HEventType
    {
        None,
        UserEvent,
        TextInput,
        GestureEvent
    }
    public enum CompositeType
    {
        None,
        Slider,
        ScrollX,
        ScrollY,
        GridScroll,
        Paint,
        Rocker,
        UIContainer,
        TreeView,
        UIDate,
        UIPalette,
        ScrollYExtand,
        DropDown,
        StackPanel,
        TabControl,
        DockPanel,
        DesignedDockPanel, 
        DragContent,
        DataGrid,
        InputBox
    }
    [DisallowMultipleComponent]
    /// <summary>
    /// UI基本元素组件
    /// </summary>
    public class UIElement:MonoBehaviour
    {
        /// <summary>
        /// 最后更新的帧数,防止同一帧内重复更新
        /// </summary>
        internal int LateFrame;
        #region static method
        static Transform[] buff = new Transform[64];
        public static Coordinates GetGlobaInfo(Transform trans, bool Includeroot = true)
        {
            buff[0] = trans;
            var parent = trans.parent;
            int max = 1;
            if (parent != null)
                for (; max < 64; max++)
                {
                    buff[max] = parent;
                    parent = parent.parent;
                    if (parent == null)
                        break;
                }
            Vector3 pos, scale;
            Quaternion quate;
            if (Includeroot)
            {
                var p = buff[max];
                pos = p.localPosition;
                scale = p.localScale;
                quate = p.localRotation;
                max--;
            }
            else
            {
                pos = Vector3.zero;
                scale = Vector3.one;
                quate = Quaternion.identity;
                max--;
            }
            for (; max >= 0; max--)
            {
                var rt = buff[max];
                Vector3 p = rt.localPosition;
                Vector3 o = Vector3.zero;
                o.x = p.x * scale.x;
                o.y = p.y * scale.y;
                o.z = p.z * scale.z;
                pos += quate * o;
                quate *= rt.localRotation;
                Vector3 s = rt.localScale;
                scale.x *= s.x;
                scale.y *= s.y;
            }
            Coordinates coord = new Coordinates();
            coord.Postion = pos;
            coord.quaternion = quate;
            coord.Scale = scale;
            return coord;
        }
        public static Vector3 ScreenToLocal(Transform trans, Vector3 v)
        {
            var g = GetGlobaInfo(trans,false);
            v -= g.Postion;
            if (g.Scale.x != 0)
                v.x /= g.Scale.x;
            else v.x = 0;
            if (g.Scale.y != 0)
                v.y /= g.Scale.y;
            else v.y = 0;
            if (g.Scale.z != 0)
                v.z /= g.Scale.z;
            else v.z = 0;
            var q = Quaternion.Inverse(g.quaternion);
            v = q * v;
            return v;
        }
        public static Vector2[] Anchors = new[] { new Vector2(0.5f, 0.5f), new Vector2(0, 0.5f),new Vector2(1, 0.5f),
        new Vector2(0.5f, 1),new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1)};
        public static void Scaling(UIElement script, ScaleType type, Vector2 pSize, Vector2 ds)
        {
            var rect = script.transform;
            switch (type)
            {
                case ScaleType.None:
                    break;
                case ScaleType.FillX:
                    float sx = pSize.x / ds.x;
                    rect.localScale = new Vector3(sx, sx, sx);
                    break;
                case ScaleType.FillY:
                    float sy = pSize.y / ds.y;
                    rect.localScale = new Vector3(sy, sy, sy);
                    break;
                case ScaleType.FillXY:
                    sx = pSize.x / ds.x;
                    sy = pSize.y / ds.y;
                    if (sx < sy)
                        rect.localScale = new Vector3(sx, sx, sx);
                    else rect.localScale = new Vector3(sy, sy, sy);
                    break;
                case ScaleType.Cover:
                    sx = pSize.x / ds.x;
                    sy = pSize.y / ds.y;
                    if (sx < sy)
                        rect.localScale = new Vector3(sy, sy, sy);
                    else rect.localScale = new Vector3(sx, sx, sx);
                    break;
            }
        }
        public static void AnchorEx(UIElement script, AnchorPointType type, Vector2 offset, Vector2 p, Vector2 psize)
        {
            Vector2 pivot = Anchors[(int)type];
            float x = psize.x;
            float y = psize.y;
            float px = p.x;
            float py = p.y;

            float lx = x * -px;
            float dy = y * -py;

            float tx = lx + pivot.x * psize.x;//锚点x
            float ty = dy + pivot.y * psize.y;//锚点y
            offset.x += tx;//偏移点x
            offset.y += ty;//偏移点y
            script.transform.localPosition = new Vector3(offset.x, offset.y, 0);
        }
        public static void AlignmentEx(UIElement script, AnchorPointType type, Vector2 offset, Vector2 p, Vector2 psize)
        {
            Vector2 pivot = Anchors[(int)type];
            float ax = psize.x;
            float ay = psize.y;
            float apx = p.x;
            float apy = p.y;
            float alx = ax * -apx;
            float ady = ay * -apy;
            //float aox = ax * apx;//原点x
            //float aoy = ay * apy;//原点y

            float x = script.SizeDelta.x;
            float y = script.SizeDelta.y;
            float px = script.Pivot.x;
            float py = script.Pivot.y;
            float lx = x * -px;
            float dy = y * -py;

            //float ox = x * px;//原点x
            //float oy = y * py;//原点y

            switch (type)
            {
                case AnchorPointType.Left:
                    x = alx - lx;
                    y = (ady + ay * 0.5f) - (dy + y * 0.5f);
                    break;
                case AnchorPointType.Right:
                    x = (ax + alx) - (x + lx);
                    y = (ady + ay * 0.5f) - (dy + y * 0.5f);
                    break;
                case AnchorPointType.Top:
                    x = (alx + ax * 0.5f) - (lx + x * 0.5f);
                    y = (ay + ady) - (y + dy);
                    break;
                case AnchorPointType.Down:
                    x = (alx + ax * 0.5f) - (lx + x * 0.5f);
                    y = ady - dy;
                    break;
                case AnchorPointType.LeftDown:
                    x = alx - lx;
                    y = ady - dy;
                    break;
                case AnchorPointType.LeftTop:
                    x = alx - lx;
                    y = (ay + ady) - (y + dy);
                    break;
                case AnchorPointType.RightDown:
                    x = (ax + alx) - (x + lx);
                    y = ady - dy;
                    break;
                case AnchorPointType.RightTop:
                    x = (ax + alx) - (x + lx);
                    y = (ay + ady) - (y + dy);
                    break;
                default:
                    x = (alx + ax * 0.5f) - (lx + x * 0.5f);
                    y = (ady + ay * 0.5f) - (dy + y * 0.5f);
                    break;
            }
            x += offset.x;
            y += offset.y;
            script.transform.localPosition = new Vector3(x, y, 0);
        }
        public static void MarginEx(UIElement script, Margin margin, Vector2 parentPivot, Vector2 parentSize)
        {
            var rect = script.transform;
            float w = parentSize.x - margin.left - margin.right;
            float h = parentSize.y - margin.top - margin.down;
            var m_pivot = script.Pivot;
            float ox = w * m_pivot.x - parentPivot.x * parentSize.x + margin.left;
            float oy = h * m_pivot.y - parentPivot.y * parentSize.y + margin.down;
            float sx = rect.localScale.x;
            float sy = rect.localScale.y;
            script.SizeDelta = new Vector2(w / sx, h / sy);
            rect.localPosition = new Vector3(ox, oy, 0);
        }
        public static void MarginX(UIElement script, Margin margin, Vector2 parentPivot, Vector2 parentSize)
        {
            var rect = script.transform;
            float w = parentSize.x - margin.left - margin.right;
            var m_pivot = script.Pivot;
            float ox = w * m_pivot.x - parentPivot.x * parentSize.x + margin.left;
            float sx = rect.localScale.x;
            float y = script.SizeDelta.y;
            script.SizeDelta = new Vector2(w / sx, y);
            float py = rect.localPosition.y;
            rect.localPosition = new Vector3(ox, py, 0);
        }
        public static void MarginY(UIElement script, Margin margin, Vector2 parentPivot, Vector2 parentSize)
        {
            var rect = script.transform;
            float h = parentSize.y - margin.top - margin.down;
            var m_pivot = script.Pivot;
            float oy = h * m_pivot.y - parentPivot.y * parentSize.y + margin.down;
            float sy = rect.localScale.y;
            float x = script.SizeDelta.x;
            script.SizeDelta = new Vector2(x, h / sy);
            float px = rect.localPosition.x;
            rect.localPosition = new Vector3(px, oy, 0);
        }
        public static void Resize(UIElement script,bool child = true)
        {
            Transform rect = script.transform;
            Vector2 psize = Vector2.zero;
            Vector2 v = script.m_sizeDelta;
            var pp = Anchors[0];
            if (script.parentType == ParentType.Tranfrom)
            {
                var p = rect.parent;
                if(p!=null)
                {
                    var t = p.GetComponent<UIElement>();
                    if (t != null)
                    {
                        psize = t.SizeDelta;
                        pp = t.Pivot;
                    }
                }
            }
            else
            {
                var t = rect.root.GetComponent<UIElement>();
                if (t != null)
                    psize = t.SizeDelta;
            }
            switch (script.marginType)
            {
                case MarginType.None:
                    break;
                case MarginType.Margin:
                    MarginEx(script, script.margin, pp, psize);
                    break;
                case MarginType.MarginRatio:
                    var mar = new Margin();
                    mar.left = script.margin.left * psize.x;
                    mar.right = script.margin.right * psize.x;
                    mar.top = script.margin.top * psize.y;
                    mar.down = script.margin.down * psize.y;
                    MarginEx(script, mar, pp, psize);
                    break;
                case MarginType.MarginX:
                    MarginX(script, script.margin, pp, psize);
                    break;
                case MarginType.MarginY:
                    MarginY(script, script.margin, pp, psize);
                    break;
                case MarginType.MarginRatioX:
                    mar = new Margin();
                    mar.left = script.margin.left * psize.x;
                    mar.right = script.margin.right * psize.x;
                    MarginX(script, mar, pp, psize);
                    break;
                case MarginType.MarginRatioY:
                    mar = new Margin();
                    mar.top = script.margin.top * psize.y;
                    mar.down = script.margin.down * psize.y;
                    MarginY(script, mar, pp, psize);
                    break;
                case MarginType.Size:
                    script.m_sizeDelta.x = psize.x - script.margin.left;
                    script.m_sizeDelta.y = psize.y - script.margin.down;
                    break;
                case MarginType.Ratio:
                    script.m_sizeDelta.x = psize.x * (1 - script.margin.left);
                    script.m_sizeDelta.y = psize.y * (1 - script.margin.down);
                    break;
                case MarginType.SizeX:
                    script.m_sizeDelta.x = psize.x - script.margin.left;
                    break;
                case MarginType.SizeY:
                    script.m_sizeDelta.y = psize.y - script.margin.down;
                    break;
                case MarginType.RatioX:
                    script.m_sizeDelta.x = psize.x * (1 - script.margin.left);
                    break;
                case MarginType.RatioY:
                    script.m_sizeDelta.y = psize.y * (1 - script.margin.down);
                    break;
            }
            switch (script.anchorType)
            {
                case AnchorType.None:
                    break;
                case AnchorType.Anchor:
                    AnchorEx(script, script.anchorPointType, script.anchorOffset, pp, psize);
                    break;
                case AnchorType.Alignment:
                    AlignmentEx(script, script.anchorPointType, script.anchorOffset, pp, psize);
                    break;
            }
            if (script.scaleType != ScaleType.None)
                Scaling(script, script.scaleType, psize, script.m_sizeDelta);
            if (child)
                ResizeChild(rect, child);
            if (script.scaleType != ScaleType.None | script.anchorType != AnchorType.None | script.marginType != MarginType.None)
            {
                ResizeChild(rect, false);
                if (v != script.m_sizeDelta)
                    script.ReSized();
            }
        }
        public static void ResizeChild(Transform trans, bool child = true)
        {
            for (int i = 0; i < trans.childCount; i++)
            {
                var son = trans.GetChild(i);
                var ss =son.GetComponent<UIElement>();
                if (ss != null)
                    Resize(ss, child);
                else if (child)
                    ResizeChild(son,child);
            }
        }
        public static void ResizeChild(UIElement script, bool child = true)
        {
            var rect = script.transform;
            for (int i = 0; i < rect.childCount; i++)
            {
                var ss = rect.GetChild(i).GetComponent<UIElement>();
                if (ss != null)
                    Resize(ss, child);
            }
        }
        public static void Margin(UIElement script)
        {
            Transform rect = script.transform;
            Vector3 loclpos = rect.localPosition;
            Vector2 psize = Vector2.zero;
            Vector2 v = script.m_sizeDelta;
            var pp = Anchors[0];
            if (script.parentType == ParentType.Tranfrom)
            {
                var p = rect.parent;
                if (p != null)
                {
                    var t = p.GetComponent<UIElement>();
                    if (t != null)
                    {
                        psize = t.SizeDelta;
                        pp = t.Pivot;
                    }
                }
            }
            else
            {
                var t = rect.root.GetComponent<UIElement>();
                if (t != null)
                    psize = t.SizeDelta;
            }
            switch (script.marginType)
            {
                case MarginType.None:
                    break;
                case MarginType.Margin:
                    var mar = script.margin;
                    MarginEx(script, mar, pp, psize);
                    break;
                case MarginType.MarginRatio:
                    mar = new Margin();
                    mar.left = script.margin.left * psize.x;
                    mar.right = script.margin.right * psize.x;
                    mar.top = script.margin.top * psize.y;
                    mar.down = script.margin.down * psize.y;
                    MarginEx(script, mar, pp, psize);
                    break;
                case MarginType.MarginX:
                    mar = script.margin;
                    MarginX(script, mar, pp, psize);
                    break;
                case MarginType.MarginY:
                    mar = script.margin;
                    MarginY(script, mar, pp, psize);
                    break;
                case MarginType.MarginRatioX:
                    mar = new Margin();
                    mar.left = script.margin.left * psize.x;
                    mar.right = script.margin.right * psize.x;
                    MarginX(script, mar, pp, psize);
                    break;
                case MarginType.MarginRatioY:
                    mar = new Margin();
                    mar.top = script.margin.top * psize.y;
                    mar.down = script.margin.down * psize.y;
                    MarginY(script, mar, pp, psize);
                    break;
                case MarginType.Size:
                    script.m_sizeDelta.x = psize.x - script.margin.left;
                    script.m_sizeDelta.y = psize.y - script.margin.down;
                    break;
                case MarginType.Ratio:
                    script.m_sizeDelta.x = psize.x * (1 - script.margin.left);
                    script.m_sizeDelta.y = psize.y * (1 - script.margin.down);
                    break;
                case MarginType.SizeX:
                    script.m_sizeDelta.x = psize.x - script.margin.left;
                    break;
                case MarginType.SizeY:
                    script.m_sizeDelta.y = psize.y - script.margin.down;
                    break;
                case MarginType.RatioX:
                    script.m_sizeDelta.x = psize.x * (1 - script.margin.left);
                    break;
                case MarginType.RatioY:
                    script.m_sizeDelta.y = psize.y * (1 - script.margin.down);
                    break;
            }
        }
        public static void Dock(UIElement script)
        {
            Transform rect = script.transform;
            Vector3 loclpos = rect.localPosition;
            Vector2 psize = Vector2.zero;
            Vector2 v = script.m_sizeDelta;
            var pp = Anchors[0];
            if (script.parentType == ParentType.Tranfrom)
            {
                var p = rect.parent;
                if (p != null)
                {
                    var t = p.GetComponent<UIElement>();
                    if (t != null)
                    {
                        psize = t.SizeDelta;
                        pp = t.Pivot;
                    }
                }
            }
            else
            {
                var t = rect.root.GetComponent<UIElement>();
                if (t != null)
                    psize = t.SizeDelta;
            }
            switch (script.anchorType)
            {
                case AnchorType.None:
                    break;
                case AnchorType.Anchor:
                    AnchorEx(script, script.anchorPointType, script.anchorOffset, pp, psize);
                    break;
                case AnchorType.Alignment:
                    AlignmentEx(script, script.anchorPointType, script.anchorOffset, pp, psize);
                    break;
            }
        }
        public static Vector2 GetSize(UIElement parent,FakeStruct ele)
        {
            unsafe
            {
                Vector2 psize = Vector2.zero;
                UIElementData* up = (UIElementData*)ele.ip;
                switch(up->parentType)
                {
                    case ParentType.Screen:
                        psize = HCanvas.MainCanvas.m_sizeDelta;
                        break;
                    case ParentType.Tranfrom:
                        if(parent!=null)
                            psize = parent.m_sizeDelta;
                        break;
                }
                switch(up->marginType)
                {
                    case MarginType.None:
                        return up->m_sizeDelta;
                    case MarginType.Margin:
                        psize.x -= up->margin.left + up->margin.right;
                        psize.y -= up->margin.top + up->margin.down;
                        return psize;
                    case MarginType.MarginRatio:
                        psize.x *=(1- up->margin.left - up->margin.right);
                        psize.y *=(1-up->margin.top - up->margin.down);
                        return psize;
                    case MarginType.MarginX:
                        psize.x -= up->margin.left + up->margin.right;
                        psize.y = up->m_sizeDelta.y;
                        return psize;
                    case MarginType.MarginY:
                        psize.x = up->m_sizeDelta.x;
                        psize.y -= up->margin.top + up->margin.down;
                        return psize;
                    case MarginType.MarginRatioX:
                        psize.x *= (1 - up->margin.left - up->margin.right);
                        psize.y = up->m_sizeDelta.y;
                        return psize;
                    case MarginType.MarginRatioY:
                        psize.x = up->m_sizeDelta.x;
                        psize.y *= (1 - up->margin.top - up->margin.down);
                        return psize;
                    case MarginType.Size:
                        psize.x -= up->margin.left;
                        psize.y -= up->margin.down;
                        return psize;
                    case MarginType.Ratio:
                        psize.x *= (1 - up->margin.left);
                        psize.y *= (1 - up->margin.down);
                        return psize;
                    case MarginType.SizeX:
                        psize.x -= up->margin.left;
                        psize.y = up->m_sizeDelta.y;
                        return psize;
                    case MarginType.SizeY:
                        psize.x = up->m_sizeDelta.x;
                        psize.y -= up->margin.down;
                        return psize;
                    case MarginType.RatioX:
                        psize.x *= (1 - up->margin.left);
                        psize.y = up->m_sizeDelta.y;
                        return psize;
                    case MarginType.RatioY:
                        psize.x = up->m_sizeDelta.x;
                        psize.y *= (1 - up->margin.down);
                        return psize;
                }
                return up->m_sizeDelta;
            }
        }
        #endregion
        [SerializeField]
        internal Vector2 m_sizeDelta = new Vector2(100,100);
        public virtual Vector2 SizeDelta { get => m_sizeDelta; set => m_sizeDelta = value; }
        /// <summary>
        /// 轴心
        /// </summary>
        public Vector2 Pivot = new Vector2(0.5f, 0.5f);
        //public Vector2 DesignSize;
        /// <summary>
        /// 缩放类型
        /// </summary>
        public ScaleType scaleType;
        /// <summary>
        /// 停靠类型
        /// </summary>
        public AnchorType anchorType;
        /// <summary>
        /// 停靠的点类型
        /// </summary>
        public AnchorPointType anchorPointType;
        /// <summary>
        /// 停靠的偏移位置
        /// </summary>
        public Vector2 anchorOffset;
        /// <summary>
        /// 相对与父物体的剔除尺寸的类型
        /// </summary>
        public MarginType marginType;
        /// <summary>
        /// 父物体类型
        /// </summary>
        public ParentType parentType;
        /// <summary>
        /// 相对与父物体的剔除尺寸
        /// </summary>
        public Margin margin;
        /// <summary>
        /// 用户事件类型
        /// </summary>
        public HEventType eventType;
        /// <summary>
        /// 复合型UI类型
        /// </summary>
        public CompositeType compositeType;
        /// <summary>
        /// 主线脚本更新
        /// </summary>
        public virtual void MainUpdate()
        {
            if (userEvent != null)
            {
                if (userEvent.Frame != LateFrame)
                {
                    userEvent.Frame = LateFrame;
                    userEvent.Update();
                }
                else
                {
                    Debug.Log("事件重复调用");
                }
            }
            if (composite != null)
            {
                if(composite.Frame!=LateFrame)
                {
                    composite.Frame = LateFrame;
                    composite.Update(UserAction.TimeSlice);
                }
                else
                {
                    Debug.Log("组合件重复调用");
                }
            }
        }
        /// <summary>
        /// 分线程脚本更新
        /// </summary>
        public virtual void SubUpdate()
        {

        }
        /// <summary>
        /// 重新计算尺寸
        /// </summary>
        public virtual void ReSized()
        {
            if (SizeChanged != null)
                SizeChanged(this);
        }
        /// <summary>
        /// 是否开启遮罩
        /// </summary>
        public bool Mask;
        /// <summary>
        /// 用户事件
        /// </summary>
        public UserEvent userEvent;
        /// <summary>
        /// 复合ui组件实体
        /// </summary>
        public Composite composite;
        /// <summary>
        /// 数据模型
        /// </summary>
        public FakeStruct mod;
        /// <summary>
        /// 联系上下文
        /// </summary>
        public object DataContext;
        /// <summary>
        /// 在流水线中的位置
        /// </summary>
        internal int PipelineIndex;
        /// <summary>
        /// 主颜色
        /// </summary>
        public virtual Color32 MainColor { get; set; }
        /// <summary>
        /// 注册用户事件
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <param name="fake">数据模型,可空</param>
        /// <returns></returns>
        public T RegEvent<T>(FakeStruct fake) where T : UserEvent, new()
        {
            var t = userEvent as T;
            if (t != null)
                return t;
            t = new T();
            t.Context = this;
            t.Initial(fake);
            userEvent = t;
            t.g_color = MainColor;
            return t;
        }
        /// <summary>
        /// 注册事件
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="collider">事件检测碰撞器,可空</param>
        /// <returns></returns>
        public T RegEvent<T>(EventCollider collider = null) where T : UserEvent, new()
        {
            var t = userEvent as T;
            if (t != null)
                return t;
            t = new T();
            t.Context = this;
            if (collider == null)
                t.collider = new UIBoxCollider();
            else t.collider = collider;
            userEvent = t;
            t.g_color = MainColor;
            return t;
        }
        /// <summary>
        /// 当UI尺寸被改变时,执行此委托
        /// </summary>
        public Action<UIElement> SizeChanged;
        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="ex">数据模型</param>
        public void Initial(FakeStruct ex,Initializer initializer)
        {
            switch(eventType)
            {
                case HEventType.None: break;
                case HEventType.UserEvent:RegEvent<UserEvent>(ex); break;
                case HEventType.TextInput: RegEvent<InputBoxEvent>(ex); break;
                case HEventType.GestureEvent: RegEvent<GestureEvent>(ex); break;
            }
            CreateUIComposite(this,ex, initializer);
        }
        /// <summary>
        /// 创建复合型UI实体
        /// </summary>
        /// <param name="script">ui元素实体</param>
        /// <param name="ex">数据模型</param>
        public static void CreateUIComposite(UIElement script,FakeStruct ex,Initializer initializer)
        {
            switch(script.compositeType)
            {
                case CompositeType.None:
                    break;
                case CompositeType.ScrollY:
                    new ScrollY().Initial(ex,script,initializer);
                    break;
                case CompositeType.ScrollX:
                    new ScrollX().Initial(ex, script,initializer);
                    break;
                case CompositeType.Slider:
                    new UISlider().Initial(ex, script,initializer);
                    break;
                case CompositeType.GridScroll:
                    new GridScroll().Initial(ex,script,initializer);
                    break;
                case CompositeType.Paint:
                    new Paint().Initial(ex,script,initializer);
                    break;
                case CompositeType.Rocker:
                    new UIRocker().Initial(ex,script,initializer);
                    break;
                case CompositeType.UIContainer:
                    new UIContainer().Initial(ex,script,initializer);
                    break;
                case CompositeType.TreeView:
                    new TreeView().Initial(ex,script,initializer);
                    break;
                case CompositeType.UIDate:
                    new UIDate().Initial(ex,script,initializer);
                    break;
                case CompositeType.UIPalette:
                    new UIPalette().Initial(ex,script,initializer);
                    break;
                case CompositeType.ScrollYExtand:
                    new ScrollYExtand().Initial(ex,script,initializer);
                    break;
                case CompositeType.DropDown:
                    new DropdownEx().Initial(ex,script,initializer);
                    break;
                case CompositeType.StackPanel:
                    new StackPanel().Initial(ex,script,initializer);
                    break;
                case CompositeType.DragContent:
                    new DragContent().Initial(ex,script,initializer);
                    break;
                case CompositeType.TabControl:
                    new TabControl().Initial(ex,script,initializer);
                    break;
                case CompositeType.DockPanel:
                    new DockPanel().Initial(ex,script,initializer);
                    break;
                case CompositeType.DesignedDockPanel:
                    new DesignedDockPanel().Initial(ex,script,initializer);
                    break;
                case CompositeType.DataGrid:
                    new DataGrid().Initial(ex,script,initializer);
                    break;
                case CompositeType.InputBox:
                    new InputBox().Initial(ex,script,initializer);
                    break;
            }
        }
        /// <summary>
        /// 清除资源
        /// </summary>
        public virtual void Clear()
        {
        }
        protected virtual void Start()
        {
            if (eventType != HEventType.None)
            {
                if (userEvent == null)
                    switch (eventType)
                    {
                        case HEventType.UserEvent:
                            RegEvent<UserEvent>(mod);
                            break;
                        case HEventType.GestureEvent:
                            RegEvent<GestureEvent>(mod);
                            break;
                    }
            }
            if (compositeType != CompositeType.None)
            {
                if (composite == null)
                {
                    if (mod == null)
                        mod = HGUIManager.GetFakeData(this.transform);
                    CreateUIComposite(this, mod, null);
                }
            }
        }
    }
}
