using huqiang.Core.HGUI;
using System;
using UnityEngine;
using huqiang.UIComposite;
using huqiang.UIEvent;
using huqiang;
using huqiang.Data;
using Assets.Scripts;
using huqiang.UIModel;

public class DrawingPage:UIPage
{
    //反射UI界面上的物体
    class View
    {
        public Paint paint;
        public UIPalette palette;
        public UISlider paintSize;
        public HText size;
        public HImage color;
        public UserEvent last;
        public UserEvent next;
    }
    View view;
    public override void Initial(Transform parent, object dat = null)
    {
        base.Initial(parent, dat);
        view = LoadUI<View>("baseUI", "drawing");//"baseUI"创建的bytes文件名,"page"为创建的页面名
        InitialUI();
        view.last.Click = (o, e) => { LoadPage<ChatPage>(); };
        view.next.Click = (o, e) => { LoadPage<ScrollPage>(); };
    }
    void InitialUI()
    {
        view.paint.BrushColor = Color.black;
        view.paint.BrushSize = 36;
        view.palette.TemplateChanged=
        view.palette.ColorChanged = (o) => {
            view.color.MainColor = o.SelectColor;
            view.paint.BrushColor = o.SelectColor;
        };
        view.paintSize.OnValueChanged = (o) => {
            var v = o.Percentage * 36;
            if (v < 1)
                v = 1;
            view.size.Text = v.ToString();
            view.paint.BrushSize = v;
        };
    }
}
