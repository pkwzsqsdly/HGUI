﻿using huqiang.UIEvent;
using System;
using System.Collections.Generic;
using UnityEngine;
using huqiang.Core.HGUI;
using huqiang.Data;

namespace huqiang.UIComposite
{
    public class TabControl:Composite
    {
        public class TableContent
        {
            public TabControl Parent;
            public UIElement Item;
            public UserEvent eventCall;
            public HText Label;
            public HImage Back;
            public UIElement Content;
            public object Context;
        }
        /// <summary>
        /// 选项头停靠方向
        /// </summary>
        public enum HeadDock
        {
            Top, Down
        }
        public UIElement Head;
        public Transform Items;
        public UIElement Content;
        public FakeStruct Item;
        public TableContent CurContent;
        public Action<TabControl> SelectChanged;
        StackPanel stackPanel;
        HeadDock dock;
        /// <summary>
        /// 头部停靠位置
        /// </summary>
        public HeadDock headDock { 
            get=>dock; 
            set {
                if (value == dock)
                    return;
                dock = value;
                switch(dock)
                {
                    case HeadDock.Top:
                        Head.anchorPointType = AnchorPointType.Top;
                        Content.margin.top = Head.SizeDelta.y;
                        Content.margin.down = 0;
                        break;
                    case HeadDock.Down:
                        Head.anchorPointType = AnchorPointType.Down;
                        Content.margin.down = Head.SizeDelta.y;
                        Content.margin.top = 0;
                        break;
                }
                UIElement.ResizeChild(Enity);
            } }
        public float headHigh = 0;
        public List<TableContent> contents;
        /// <summary>
        /// 当前被选中项的背景色
        /// </summary>
        public Color SelectColor = 0x2656FFff.ToColor();
        /// <summary>
        /// 鼠标停靠时的背景色
        /// </summary>
        public Color HoverColor = 0x5379FFff.ToColor();
        public override void Initial(FakeStruct mod,UIElement element)
        {
            base.Initial(mod,element);
            contents = new List<TableContent>();
            var trans = Enity.transform;
            Head = trans.Find("Head").GetComponent<UIElement>();
            Items = Head.transform.Find("Items");
            stackPanel = Items.GetComponent<UIElement>().composite as StackPanel;
            Item= HGUIManager.FindChild(mod, "Item");
            Content = trans.Find("Content").GetComponent<UIElement>();
            HGUIManager.GameBuffer.RecycleGameObject(trans.Find("Item").gameObject);
        }
        /// <summary>
        /// 使用默认标签页
        /// </summary>
        /// <param name="model"></param>
        /// <param name="label"></param>
        public TableContent AddContent(UIElement model, string label)
        {
            if (Item == null)
                return null;
            TableContent content = new TableContent();
            content.Parent = this;
            var mod = HGUIManager.GameBuffer.Clone(Item).transform;

            content.Item = mod.GetComponent<UIElement>();
            content.Label = mod.Find("Text").GetComponent<HText>();
            content.Label.Text = label;
            content.Back = mod.Find("Image").GetComponent<HImage>();
            content.Content = model;

            var eve = content.Item.RegEvent<UserEvent>();
            eve.Click = ItemClick;
            eve.PointerEntry = ItemPointEntry;
            eve.PointerLeave = ItemPointLeave;
            content.eventCall = eve;
            content.eventCall.DataContext = content;
            if (CurContent != null)
            {
                CurContent.Content.gameObject.SetActive(false);
                if (CurContent.Back != null)
                    CurContent.Back.gameObject.SetActive(false);
            }
            model.transform.SetParent(Content.transform);
            model.transform.localScale = Vector3.one;
            UIElement.Resize(model);
            CurContent = content;
            CurContent.Back.MainColor = SelectColor;
            contents.Add(CurContent);
            mod.SetParent(Items);
            return content;
        }
        /// <summary>
        /// 使用自定义标签页,标签模型自行管理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <param name="dat"></param>
        /// <param name="callback"></param>
        public void AddContent(TableContent table)
        {
            if (CurContent != null)
            {
                CurContent.Content.gameObject.SetActive(false);
                if (CurContent.Back != null)
                    CurContent.Back.gameObject.SetActive(false);
            }
            table.Item.transform.SetParent(Items);
            table.Item.userEvent.Click = ItemClick;
            table.Item.userEvent.PointerEntry = ItemPointEntry;
            table.Item.userEvent.PointerLeave = ItemPointLeave;
            table.Content.transform.SetParent(Content.transform);
            CurContent = table;
            CurContent.Content.gameObject.SetActive(true);
            if (CurContent.Back != null)
                CurContent.Back.gameObject.SetActive(true);
            contents.Add(table);
        }
        /// <summary>
        /// 移除某个标签和其内容
        /// </summary>
        /// <param name="table"></param>
        public void RemoveContent(TableContent table)
        {
            contents.Remove(table);
            if(contents.Count>0)
            {
                CurContent = contents[0];
                if (CurContent.Content != null)
                    CurContent.Content.gameObject.SetActive(true);
                if (CurContent.Back != null)
                    CurContent.Back.gameObject.SetActive(true);
            }
        }
        /// <summary>
        /// 释放某个标签和其内容,其对象会被回收
        /// </summary>
        /// <param name="table"></param>
        public void ReleseContent(TableContent table)
        {
            contents.Remove(table);
            if (table == CurContent)
                CurContent = null;
            HGUIManager.GameBuffer.RecycleGameObject(table.Content.gameObject);
            HGUIManager.GameBuffer.RecycleGameObject(table.Item.gameObject);
        }
        public void AddTable(TableContent table)
        {
            table.eventCall.Click = ItemClick;
            table.eventCall.PointerEntry = ItemPointEntry;
            table.eventCall.PointerLeave = ItemPointLeave;
            table.Label.transform.SetParent(Head.transform);
            table.Content.transform.SetParent(Content.transform);
        }
        public void ShowContent(TableContent content)
        {
            if (CurContent != null)
            {
                if (CurContent.Content != null)
                    CurContent.Content.gameObject.SetActive(false);
                if (CurContent.Back != null)
                    CurContent.Back.gameObject.SetActive(false);
            }
            CurContent = content;
            CurContent.Content.gameObject.SetActive(true);
            if (CurContent.Back != null)
            {
                CurContent.Back.MainColor = SelectColor;
                CurContent.Back.gameObject.SetActive(true);
            }
        }
        public bool ExistContent(TableContent content)
        {
            return contents.Contains(content);
        }
        public void ItemClick(UserEvent callBack,UserAction action)
        {
            var con = callBack.DataContext as TableContent;
            var cur = CurContent;
            ShowContent(con);
            if (con != cur)
                if (SelectChanged != null)
                    SelectChanged(this);
        }
        public void ItemPointEntry(UserEvent callBack,UserAction action)
        {
            var c = callBack.DataContext as TableContent;
            if (c == CurContent)
                return;
            if (c != null)
            {
                if (c.Back != null)
                {
                    c.Back.MainColor = HoverColor;
                    c.Back.gameObject.SetActive(true);
                }
            }
        }
        public void ItemPointLeave(UserEvent callBack,UserAction action)
        {
            var c = callBack.DataContext as TableContent;
            if (c == CurContent)
                return;
            if (c != null)
            {
                if (c != CurContent)
                    if (c.Back != null)
                    {
                        c.Back.gameObject.SetActive(false);
                    }
            }
        }
    }
}