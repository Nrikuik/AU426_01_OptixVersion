#region Using directives
using System;
using System.Linq;
using System.Collections.Generic;
using FTOptix.Core;
using FTOptix.CoreBase;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.UI;
using UAManagedCore;
using FTOptix.RAEtherNetIP;
using FTOptix.CommunicationDriver;
using OpcUa = UAManagedCore.OpcUa;
#endregion

/*
 * BuildModeSelectPanel — Design-time NetLogic
 * Migración de FT View "PANTALLA DE SELECCION DE MODO" a FactoryTalk Optix
 * Panel ~560x650, fondo verde oscuro, bajo UI como Panel (type)
 *
 * ELEMENTOS:
 *   - Título "PANTALLA DE SELECCION DE MODO"
 *   - Botones UP/DOWN azules con flechas
 *   - ListBox con opciones de modo
 *   - Botón circular azul grande (confirmación)
 *   - Botón circular azul pequeño (navegación)
 *
 * USO:
 *   1. Ejecutar CreateModel()
 *   2. Ejecutar CreateUI()
 *   3. Crear RuntimeNetLogic hijo del panel para UP/DOWN (mismo patrón que RobotNavLogic)
 */
public class BuildModeSelectPanel : BaseNetLogic
{
    const int PANEL_W = 560;
    const int PANEL_H = 650;

    static readonly Color COL_GREEN_BG   = new Color(0xFF, 0x0E, 0x4E, 0x0E);
    static readonly Color COL_BLUE       = new Color(0xFF, 0x00, 0x99, 0xCC);
    static readonly Color COL_WHITE      = new Color(0xFF, 0xFF, 0xFF, 0xFF);
    static readonly Color COL_BLACK      = new Color(0xFF, 0x00, 0x00, 0x00);
    static readonly Color COL_DARK_BLUE  = new Color(0xFF, 0x00, 0x00, 0xCC);

    // ════════════════════════════════════════════════════════════════
    //  PASO 1: Modelo
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreateModel()
    {
        var model = Project.Current.Get("Model");
        if (model == null) { Log.Error("BuildModeSelectPanel", "No se encontró Model."); return; }

        var modeMenu = model.Get("ModeMenu");
        if (modeMenu == null) modeMenu = CreateFolder(model, "ModeMenu");

        AddVarIfMissing(modeMenu, "SelectedIndex", OpcUa.DataTypes.Int32,  new UAValue(0));
        AddVarIfMissing(modeMenu, "SelectedText",  OpcUa.DataTypes.String, new UAValue(""));
        AddVarIfMissing(modeMenu, "CurrentMode",   OpcUa.DataTypes.String, new UAValue(""));

        var items = modeMenu.Get("Items");
        if (items == null) items = CreateFolder(modeMenu, "Items");

        // Limpiar items anteriores
        foreach (var child in items.Children.Cast<IUANode>().ToList())
            items.Remove(child);

        string[] menuItems = {
            "EN PROCESO (SOLO PANEL 2)",
            "PURGA (SOLO PANEL 2)",
            "TEST DE LAMPARAS",
            "SELECCION UNA PIEZA (SOLO PANEL 2)",
            "TODOS RBTS INICIO",
            "TODOS RBTS A REPARAR",
            "TODOS ROBOTS A TIP DRESS",
            "TODOS RBTS CAMBIAR CAP",
            "TODOS CAPS CAMBIADO",
            "TODOS RBTS MASTER REF. SWITCH",
            "TODOS ROBOT PRECALENTAMIENTO",
            "PRECALENTAMIENTO ROBOTS LISTOS"
        };

        foreach (var item in menuItems)
        {
            var nodeName = item.Replace(" ", "_")
                              .Replace("(", "")
                              .Replace(")", "")
                              .Replace(".", "");
            var node = InformationModel.MakeVariable(nodeName, OpcUa.DataTypes.String);
            node.Value = new UAValue(item);
            items.Add(node);
        }

        Log.Info("BuildModeSelectPanel", "Modelo creado. Items: " + menuItems.Length);
    }

    // ════════════════════════════════════════════════════════════════
    //  PASO 2: UI
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreateUI()
    {
        var uiRoot = Project.Current.Get("UI");
        if (uiRoot == null) { Log.Error("BuildModeSelectPanel", "No se encontró UI."); return; }

        var existing = uiRoot.Get("ModeSelectPanel");
        if (existing != null) uiRoot.Remove(existing);

        var selectedTextVar = Project.Current.GetVariable("Model/ModeMenu/SelectedText");
        var currentModeVar  = Project.Current.GetVariable("Model/ModeMenu/CurrentMode");
        var itemsNode       = Project.Current.Get("Model/ModeMenu/Items");

        if (selectedTextVar == null || currentModeVar == null || itemsNode == null)
        {
            Log.Error("BuildModeSelectPanel", "Ejecuta CreateModel() primero.");
            return;
        }

        // ── Panel principal ──
        var panel = InformationModel.MakeObject<Panel>("ModeSelectPanel");
        panel.Width  = PANEL_W;
        panel.Height = PANEL_H;
        panel.HorizontalAlignment = HorizontalAlignment.Center;
        panel.VerticalAlignment   = VerticalAlignment.Center;

        // ── Fondo verde oscuro ──
        var bg = InformationModel.MakeObject<Rectangle>("Background");
        bg.HorizontalAlignment = HorizontalAlignment.Stretch;
        bg.VerticalAlignment   = VerticalAlignment.Stretch;
        bg.LeftMargin = 0; bg.TopMargin = 0; bg.RightMargin = 0; bg.BottomMargin = 0;
        SetProp(bg, "FillColor", COL_GREEN_BG);
        SetProp(bg, "BorderThickness", new UAValue(0));
        panel.Add(bg);

        // ══════════════════════════════════════════════════════════
        //  TÍTULO
        // ══════════════════════════════════════════════════════════
        var lblTitle = InformationModel.MakeObject<Label>("LblTitle");
        lblTitle.Width = 520; lblTitle.Height = 30;
        lblTitle.TopMargin = 8; lblTitle.LeftMargin = 20;
        lblTitle.Text = "PANTALLA DE SELECCION DE MODO";
        SetProp(lblTitle, "TextColor", COL_WHITE);
        SetProp(lblTitle, "FontSize", new UAValue(14));
        SetProp(lblTitle, "FontWeight", new UAValue(700));
        panel.Add(lblTitle);

        // ══════════════════════════════════════════════════════════
        //  BOTÓN UP
        // ══════════════════════════════════════════════════════════
        var btnUp = InformationModel.MakeObject<Button>("BtnUp");
        btnUp.Width = 90; btnUp.Height = 90;
        btnUp.TopMargin = 60; btnUp.LeftMargin = 20;
        btnUp.Text = "▲";
        SetProp(btnUp, "BackgroundColor", COL_BLUE);
        SetProp(btnUp, "TextColor", COL_WHITE);
        SetProp(btnUp, "FontSize", new UAValue(36));
        panel.Add(btnUp);

        // ══════════════════════════════════════════════════════════
        //  BOTÓN DOWN
        // ══════════════════════════════════════════════════════════
        var btnDown = InformationModel.MakeObject<Button>("BtnDown");
        btnDown.Width = 90; btnDown.Height = 90;
        btnDown.TopMargin = 180; btnDown.LeftMargin = 20;
        btnDown.Text = "▼";
        SetProp(btnDown, "BackgroundColor", COL_BLUE);
        SetProp(btnDown, "TextColor", COL_WHITE);
        SetProp(btnDown, "FontSize", new UAValue(36));
        panel.Add(btnDown);

        // ══════════════════════════════════════════════════════════
        //  LISTBOX
        // ══════════════════════════════════════════════════════════
        var listBox = InformationModel.MakeObject<ListBox>("ListBox1");
        listBox.Width = 400; listBox.Height = 350;
        listBox.TopMargin = 45; listBox.LeftMargin = 130;
        listBox.Model = itemsNode.NodeId;
        listBox.DisplayValuePath = new LocalizedText("Value");

        try
        {
            listBox.SelectedValueVariable.SetDynamicLink(selectedTextVar, DynamicLinkMode.ReadWrite);
        }
        catch (Exception ex)
        {
            Log.Warning("BuildModeSelectPanel", "SelectedValue link: " + ex.Message);
        }
        panel.Add(listBox);

        // ══════════════════════════════════════════════════════════
        //  LABEL "MODO ACTUAL"
        // ══════════════════════════════════════════════════════════
        var lblModo = InformationModel.MakeObject<Label>("LblModoActual");
        lblModo.Width = 400; lblModo.Height = 35;
        lblModo.TopMargin = 410; lblModo.LeftMargin = 130;
        lblModo.Text = "MODO ACTUAL";
        SetProp(lblModo, "TextColor", COL_WHITE);
        SetProp(lblModo, "FontSize", new UAValue(16));
        SetProp(lblModo, "FontWeight", new UAValue(700));
        SetProp(lblModo, "TextHorizontalAlignment", new UAValue(1));
        panel.Add(lblModo);

        // ══════════════════════════════════════════════════════════
        //  LABEL valor modo actual — vinculado a CurrentMode
        // ══════════════════════════════════════════════════════════
        var lblModoVal = InformationModel.MakeObject<Label>("LblModoValue");
        lblModoVal.Width = 400; lblModoVal.Height = 35;
        lblModoVal.TopMargin = 445; lblModoVal.LeftMargin = 130;
        lblModoVal.Text = "";
        SetProp(lblModoVal, "TextColor", COL_WHITE);
        SetProp(lblModoVal, "FontSize", new UAValue(14));
        SetProp(lblModoVal, "TextHorizontalAlignment", new UAValue(1));

        try
        {
            lblModoVal.TextVariable.SetDynamicLink(currentModeVar, DynamicLinkMode.Read);
        }
        catch (Exception ex)
        {
            Log.Warning("BuildModeSelectPanel", "LblModoValue link: " + ex.Message);
        }
        panel.Add(lblModoVal);

        // ══════════════════════════════════════════════════════════
        //  BOTÓN CIRCULAR GRANDE — confirmación, azul
        // ══════════════════════════════════════════════════════════
        var btnConfirm = InformationModel.MakeObject<Button>("BtnConfirm");
        btnConfirm.Width = 100; btnConfirm.Height = 100;
        btnConfirm.TopMargin = 500; btnConfirm.LeftMargin = 130;
        btnConfirm.Text = "";
        SetProp(btnConfirm, "BackgroundColor", COL_DARK_BLUE);
        SetProp(btnConfirm, "TextColor", COL_WHITE);
        SetProp(btnConfirm, "Appearance", new UAValue("Bordered Circular"));
        panel.Add(btnConfirm);

        // ══════════════════════════════════════════════════════════
        //  BOTÓN CIRCULAR PEQUEÑO — navegación, azul
        // ══════════════════════════════════════════════════════════
        var btnNav = InformationModel.MakeObject<Button>("BtnNavCircle");
        btnNav.Width = 60; btnNav.Height = 60;
        btnNav.TopMargin = 520; btnNav.LeftMargin = 420;
        btnNav.Text = "▲";
        SetProp(btnNav, "BackgroundColor", COL_DARK_BLUE);
        SetProp(btnNav, "TextColor", COL_WHITE);
        SetProp(btnNav, "FontSize", new UAValue(20));
        SetProp(btnNav, "Appearance", new UAValue("Bordered Circular"));
        panel.Add(btnNav);

        // ── Agregar al UI ──
        uiRoot.Add(panel);

        Log.Info("BuildModeSelectPanel",
            "UI creada: ModeSelectPanel. Crea un RuntimeNetLogic hijo para UP/DOWN.");
    }

    // ════════════════════════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════════════════════════
    private void SetProp(IUANode node, string propName, object value)
    {
        try
        {
            var variable = node.GetVariable(propName);
            if (variable != null)
            {
                if (value is Color c) variable.Value = c;
                else if (value is UAValue u) variable.Value = u;
                else variable.Value = new UAValue(value.ToString());
            }
            else
            {
                var nv = InformationModel.MakeVariable(propName, OpcUa.DataTypes.BaseDataType);
                if (value is Color c2) nv.Value = c2;
                else if (value is UAValue u2) nv.Value = u2;
                else nv.Value = new UAValue(value.ToString());
                node.Add(nv);
            }
        }
        catch (Exception ex)
        {
            Log.Warning("BuildModeSelectPanel", propName + ": " + ex.Message);
        }
    }

    private IUANode CreateFolder(IUANode parent, string name)
    {
        var folder = InformationModel.Make<Folder>(name);
        parent.Add(folder);
        return folder;
    }

    private void AddVarIfMissing(IUANode parent, string name, NodeId dataType, UAValue value)
    {
        if (parent.Get(name) == null)
        {
            var v = InformationModel.MakeVariable(name, dataType);
            v.Value = value;
            parent.Add(v);
        }
    }
}
