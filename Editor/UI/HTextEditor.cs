﻿using System;
using System.Collections.Generic;
using huqiang;
using huqiang.Core.HGUI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HText), true)]
[CanEditMultipleObjects]
public class HTextEditor:UIElementEditor
{
    Vector3 pos;
    Vector3 scale;
    Vector3 angle;
    string str;
    public override void OnEnable()
    {
        base.OnEnable();
        HText img = target as HText;
        if (img != null)
        {
            var can = FindHCanvas(img.transform);
            if (can != null)
                can.Refresh();
        }
    }
    public override void OnSceneGUI()
    {
        base.OnSceneGUI();
        HText txt = target as HText;
        if (txt != null)
        {
            bool changed = false;
            if (pos != txt.transform.localPosition)
                changed = true;
            else if (scale != txt.transform.localScale)
                changed = true;
            else if (angle != txt.transform.localEulerAngles)
                changed = true;
            pos = txt.transform.localPosition;
            scale = txt.transform.localScale;
            angle = txt.transform.localEulerAngles;
            if (changed)
            {
                var can = FindHCanvas(txt.transform);
                if (can != null)
                    can.Refresh();
            }
           if (GUI.changed)
            {
                txt.Text = txt.Text;
            }
        }

    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        HText txt = target as HText;
        if (txt != null)
        {
            bool changed = false;
            if (str != txt.Text)
            {
                str = txt.Text;
                txt.Text = str;
                changed = true;
            }
            str = txt.Text;
            if(GUI.changed |changed)
            {
                txt.m_dirty = true;
                var can = FindHCanvas(txt.transform);
                if (can != null)
                    can.Refresh();
            }
        }
    }
    protected HCanvas FindHCanvas(Transform trans)
    {
        if (trans == null)
            return null;
        var can = trans.GetComponent<HCanvas>();
        if (can == null)
            return FindHCanvas(trans.parent);
        return can;
    }
}