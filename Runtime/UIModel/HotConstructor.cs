﻿using System;
using System.Linq;
using System.Text;
using huqiang.Core.HGUI;
using huqiang.Data;
using huqiang.UIComposite;

namespace huqiang.UIModel
{
    public class HotConstructor<T, U> where T : class, new()
    {
        public HotMiddleware middle;
        public HotConstructor(Action<T, U, int> action)
        {
            middle = new HotMiddleware();
            middle.Context = this;
            middle.initializer = new UIInitializer(UIBase.ObjectFields(typeof(T)));
            middle.creator = Create;
            middle.caller = Call;
            Invoke = action;
        }
        U u;
        public Action<T, U, int> Invoke;
        public object Create()
        {
            var t = new T();
            middle.initializer.Reset(t);
            return t;
        }
        public void Call(object obj, object dat, int index)
        {
            if (Invoke != null)
            {
                try
                {
                    u = (U)dat;
                }
                catch
                {
                }
                try
                {
                    Invoke(obj as T, u, index);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError(ex.StackTrace);
                }
            }
        }
    }
    public class HotTVConstructor<T, U> where T : TreeViewItem, new() where U : TreeViewNode, new()
    {
        public HotTVMiddleware middle;
        public HotTVConstructor(Action<T, U> action)
        {
            middle = new HotTVMiddleware();
            middle.Context = this;
            middle.initializer = new UIInitializer(UIBase.ObjectFields(typeof(T)));
            middle.creator = Create;
            middle.caller = Call;
            Invoke = action;
        }
        public Action<T, U> Invoke;
        public TreeViewItem Create()
        {
            var t = new T();
            middle.initializer.Reset(t);
            return t;
        }
        public void Call(TreeViewItem obj, TreeViewNode dat)
        {
            if (Invoke != null)
            {
                try
                {
                    Invoke(obj as T, dat as U);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError(ex.StackTrace);
                }
            }
        }
    }
}