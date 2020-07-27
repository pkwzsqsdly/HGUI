﻿using huqiang.Core.HGUI;
using huqiang.UIComposite;
using huqiang.UIEvent;
using huqiang.UIModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class StartPage:UIPage
    {
        class View
        {
            public ScrollY scrolly;
            public ScrollX scrollx;
            public GridScroll grid;
            public UserEvent last;
            public UserEvent next;
        }
        View view;
        class ItemView
        {
            public UserEvent img;
            public HText t1;
        }
        public override void Initial(Transform parent, object dat = null)
        {
            base.Initial(parent, dat);
            view = LoadUI<View>("baseUI", "start");
            List<string> data = new List<string>();
            for (int i = 1000; i < 1200; i++)
                data.Add(i.ToString()+"😄");
            var scrolly = view.scrolly;
            scrolly.BindingData = data;
            scrolly.SetItemUpdate<ItemView, string>(ItemUpdate);
            scrolly.Refresh();
            var scrollx = view.scrollx;
            scrollx.BindingData = data;
            scrollx.SetItemUpdate<ItemView, string>(ItemUpdate);
            scrollx.Refresh();
            var grid = view.grid;
            grid.Column = 10;
            grid.BindingData = data;
            grid.SetItemUpdate<ItemView, string>(ItemUpdate);
            grid.Refresh();
            view.last.Click = (o, e) => { LoadPage<TreeViewPage>(); };
            view.next.Click = (o, e) => { LoadPage<TestUPage>(); };
        }
        void ItemUpdate(ItemView item,string dat,int index)
        {
            item.t1.Text = dat;
            item.img.DataContext = index;
            item.img.Click = ItemClick;
        }
        void ItemClick(UserEvent user, UserAction action)
        {
            Debug.Log("Item: " + (int)user.DataContext + " Click !");
        }
    }
}