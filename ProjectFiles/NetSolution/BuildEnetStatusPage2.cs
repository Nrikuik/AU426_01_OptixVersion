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
 * BuildEnetStatusPage2 — Design-time NetLogic
 * ENET Status Page 2 — 1280x800
 *
 * EJECUTAR EN ORDEN:
 *   1. CreatePanel()   → Panel base
 *   2. CreateCol1()    → Columna 1 (izquierda)
 *   3. CreateCol2()    → Columna 2 (centro)
 *   4. CreateCol3()    → Columna 3 (derecha, solo unos pocos)
 *   5. CreateButtons() → Botones + leyenda
 */
public class BuildEnetStatusPage2 : BaseNetLogic
{
    const int PW = 1280;
    const int PH = 800;
    const int HDR_Y = 95;
    const int DATA_Y = 135;
    const int ROW_H = 21;
    const int IP_W = 105;
    const int GAP = 8;

    const int C1_X = 15;
    const int C2_X = 310;
    const int C3_X = 585;

    const int C1_NAME_W = 180;
    const int C2_NAME_W = 180;
    const int C3_NAME_W = 180;

    static readonly Color COL_BG     = new Color(0xFF, 0xC0, 0xC0, 0xC0);
    static readonly Color COL_BLUE   = new Color(0xFF, 0x00, 0x00, 0xCC);
    static readonly Color COL_WHITE  = new Color(0xFF, 0xFF, 0xFF, 0xFF);
    static readonly Color COL_BLACK  = new Color(0xFF, 0x00, 0x00, 0x00);
    static readonly Color COL_RED_BG = new Color(0xFF, 0x80, 0x00, 0x00);
    static readonly Color COL_GREEN  = new Color(0xFF, 0x00, 0x80, 0x00);
    static readonly Color COL_RED    = new Color(0xFF, 0xFF, 0x00, 0x00);

    // ════════════════════════════════════════════════════════════════
    //  1. Panel base
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreatePanel()
    {
        var uiRoot = Project.Current.Get("UI");
        if (uiRoot == null) return;

        var existing = uiRoot.Get("EnetStatusPage2");
        if (existing != null) uiRoot.Remove(existing);

        var panel = InformationModel.MakeObject<Panel>("EnetStatusPage2");
        panel.Width = PW; panel.Height = PH;

        var bg = InformationModel.MakeObject<Rectangle>("Background");
        bg.HorizontalAlignment = HorizontalAlignment.Stretch;
        bg.VerticalAlignment   = VerticalAlignment.Stretch;
        bg.LeftMargin = 0; bg.TopMargin = 0; bg.RightMargin = 0; bg.BottomMargin = 0;
        SP(bg, "FillColor", COL_BG);
        SP(bg, "BorderThickness", new UAValue(0));
        panel.Add(bg);

        // Franja roja
        var topBar = InformationModel.MakeObject<Rectangle>("TopBar");
        topBar.Width = PW; topBar.Height = 80;
        topBar.TopMargin = 0; topBar.LeftMargin = 0;
        SP(topBar, "FillColor", COL_RED_BG);
        SP(topBar, "BorderThickness", new UAValue(0));
        panel.Add(topBar);

        // Franja azul
        var blueBar = InformationModel.MakeObject<Rectangle>("BlueBar");
        blueBar.Width = PW; blueBar.Height = 25;
        blueBar.TopMargin = 55; blueBar.LeftMargin = 0;
        SP(blueBar, "FillColor", COL_BLUE);
        SP(blueBar, "BorderThickness", new UAValue(0));
        panel.Add(blueBar);

        AddLbl(panel, "LblPantalla", "Pantalla #  ****", 10, 58, 160, 20, COL_WHITE, 11, false);

        uiRoot.Add(panel);
        Log.Info("BuildEnetStatusPage2", "1/5 Panel base creado.");
    }

    // ════════════════════════════════════════════════════════════════
    //  2. Columna 1 — 30 dispositivos
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreateCol1()
    {
        var panel = GetPanel(); if (panel == null) return;

        AddLbl(panel, "C1H1", "IP ADDRESS", C1_X, HDR_Y,      290, 16, COL_BLACK, 11, true);
        AddLbl(panel, "C1H2", "STATUS",     C1_X, HDR_Y + 16, 290, 16, COL_BLACK, 11, true);

        string[] names = {
            "SPARE",
            "R-xx RPW VLV VMF1",
            "R-xx RPW WELD VMF1",
            "R-xx RPW / SEALER",
            "R-xx RPW VLV VMF2",
            "R-xx RPW WELD VMF2",
            "SPARE", "SPARE", "SPARE", "SPARE",
            "STA10 VMF1", "STA10 VMF2",
            "STA20 VMF1", "STA20 VMF2",
            "STA30 VMF1", "STA30 VMF2",
            "STA40 VMF1", "STA40 VMF2",
            "SPARE", "SPARE", "SPARE", "SPARE",
            "SPARE", "SPARE", "SPARE",
            "PNW PLC", "PNW ENET SWITCH",
            "PNW HMI", "PNW WELD CONTROL",
            "PNW  STA10 I/O"
        };

        for (int i = 0; i < names.Length; i++)
        {
            int y = DATA_Y + i * ROW_H;
            AddLbl(panel, "P2C1_IP_" + i,   "???????????", C1_X, y, IP_W, ROW_H, COL_BLUE, 10, false);
            AddLbl(panel, "P2C1_Name_" + i,  names[i], C1_X + IP_W + GAP, y, C1_NAME_W, ROW_H, COL_BLACK, 10, false);
        }

        Log.Info("BuildEnetStatusPage2", "2/5 Columna 1 creada.");
    }

    // ════════════════════════════════════════════════════════════════
    //  3. Columna 2 — 10 dispositivos
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreateCol2()
    {
        var panel = GetPanel(); if (panel == null) return;

        AddLbl(panel, "C2H1", "IP ADDRESS", C2_X, HDR_Y,      290, 16, COL_BLACK, 11, true);
        AddLbl(panel, "C2H2", "STATUS",     C2_X, HDR_Y + 16, 290, 16, COL_BLACK, 11, true);

        string[] names = {
            "PNW STA20 I/O",
            "PNW VLV MANIFOLD 1",
            "SPARE", "SPARE", "SPARE",
            "SPARE", "SPARE", "SPARE",
            "SPARE", "SPARE"
        };

        for (int i = 0; i < names.Length; i++)
        {
            int y = DATA_Y + i * ROW_H;
            AddLbl(panel, "P2C2_IP_" + i,   "???????????", C2_X, y, IP_W, ROW_H, COL_BLUE, 10, false);
            AddLbl(panel, "P2C2_Name_" + i,  names[i], C2_X + IP_W + GAP, y, C2_NAME_W, ROW_H, COL_BLACK, 10, false);
        }

        Log.Info("BuildEnetStatusPage2", "3/5 Columna 2 creada.");
    }

    // ════════════════════════════════════════════════════════════════
    //  4. Columna 3 — solo labels de la tercera columna (SPARE)
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreateCol3()
    {
        var panel = GetPanel(); if (panel == null) return;

        // La tercera columna en la imagen solo tiene unos pocos SPARE visibles
        string[] names = {
            "SPARE", "SPARE", "SPARE", "SPARE", "SPARE"
        };

        for (int i = 0; i < names.Length; i++)
        {
            int y = DATA_Y + i * ROW_H;
            AddLbl(panel, "P2C3_Name_" + i, names[i], C3_X, y, C3_NAME_W, ROW_H, COL_BLACK, 10, false);
        }

        Log.Info("BuildEnetStatusPage2", "4/5 Columna 3 creada.");
    }

    // ════════════════════════════════════════════════════════════════
    //  5. Botones y leyenda
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreateButtons()
    {
        var panel = GetPanel(); if (panel == null) return;

        int bottomY = 720;
        int legX = 960;

        // LEYENDA
        AddLbl(panel, "LblLegend", "LEGEND", legX, bottomY - 25, 130, 20, COL_BLACK, 11, true);

        // NOT USED
        var rNU = InformationModel.MakeObject<Rectangle>("LegNotUsed");
        rNU.Width = 130; rNU.Height = 22; rNU.TopMargin = bottomY; rNU.LeftMargin = legX;
        SP(rNU, "FillColor", COL_WHITE); SP(rNU, "BorderColor", COL_BLUE); SP(rNU, "BorderThickness", new UAValue(2));
        panel.Add(rNU);
        AddLbl(panel, "LblNotUsed", "NOT USED", legX, bottomY, 130, 22, COL_BLACK, 10, false);

        // ACTIVE
        var rAC = InformationModel.MakeObject<Rectangle>("LegActive");
        rAC.Width = 130; rAC.Height = 22; rAC.TopMargin = bottomY + 24; rAC.LeftMargin = legX;
        SP(rAC, "FillColor", COL_WHITE); SP(rAC, "BorderColor", COL_GREEN); SP(rAC, "BorderThickness", new UAValue(2));
        panel.Add(rAC);
        AddLbl(panel, "LblActive", "ACTIVE", legX, bottomY + 24, 130, 22, COL_GREEN, 10, true);

        // FAULTED
        var rFA = InformationModel.MakeObject<Rectangle>("LegFaulted");
        rFA.Width = 130; rFA.Height = 22; rFA.TopMargin = bottomY + 48; rFA.LeftMargin = legX;
        SP(rFA, "FillColor", COL_WHITE); SP(rFA, "BorderColor", COL_RED); SP(rFA, "BorderThickness", new UAValue(2));
        panel.Add(rFA);
        AddLbl(panel, "LblFaulted", "FAULTED", legX, bottomY + 48, 130, 22, COL_RED, 10, true);

        // Botón circular navegación
        var btnNav = InformationModel.MakeObject<Button>("BtnNavCircle");
        btnNav.Width = 65; btnNav.Height = 65;
        btnNav.TopMargin = bottomY - 20; btnNav.LeftMargin = 1185;
        btnNav.Text = "▲";
        SP(btnNav, "BackgroundColor", COL_BLUE);
        SP(btnNav, "TextColor", COL_WHITE);
        SP(btnNav, "FontSize", new UAValue(22));
        SP(btnNav, "Appearance", new UAValue("Bordered Circular"));
        panel.Add(btnNav);

        // Botón "INICIO"
        var btnI = InformationModel.MakeObject<Button>("BtnInicio");
        btnI.Width = 75; btnI.Height = 40;
        btnI.TopMargin = bottomY + 30; btnI.LeftMargin = 1180;
        btnI.Text = "INICIO";
        SP(btnI, "BackgroundColor", COL_BLUE);
        SP(btnI, "TextColor", COL_WHITE);
        SP(btnI, "FontSize", new UAValue(11));
        SP(btnI, "FontWeight", new UAValue(700));
        panel.Add(btnI);

        Log.Info("BuildEnetStatusPage2", "5/5 Botones y leyenda. ¡Panel completo!");
    }

    // ════════════════════════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════════════════════════
    private IUANode GetPanel()
    {
        var p = Project.Current.Get("UI/EnetStatusPage2");
        if (p == null) Log.Error("BuildEnetStatusPage2", "Ejecuta CreatePanel() primero.");
        return p;
    }

    private void AddLbl(IUANode parent, string name, string text,
        int left, int top, int width, int height,
        Color textColor, int fontSize, bool bold)
    {
        var lbl = InformationModel.MakeObject<Label>(name);
        lbl.Width = width; lbl.Height = height;
        lbl.TopMargin = top; lbl.LeftMargin = left;
        lbl.Text = text;
        SP(lbl, "TextColor", textColor);
        SP(lbl, "FontSize", new UAValue(fontSize));
        if (bold) SP(lbl, "FontWeight", new UAValue(700));
        parent.Add(lbl);
    }

    private void SP(IUANode node, string prop, object val)
    {
        try
        {
            var v = node.GetVariable(prop);
            if (v != null)
            {
                if (val is Color c) v.Value = c;
                else if (val is UAValue u) v.Value = u;
                else v.Value = new UAValue(val.ToString());
            }
            else
            {
                var nv = InformationModel.MakeVariable(prop, OpcUa.DataTypes.BaseDataType);
                if (val is Color c2) nv.Value = c2;
                else if (val is UAValue u2) nv.Value = u2;
                else nv.Value = new UAValue(val.ToString());
                node.Add(nv);
            }
        }
        catch { }
    }
}
