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
using OpcUa = UAManagedCore.OpcUa;
#endregion

/*
 * BuildEnetStatusPanel — Design-time NetLogic v2
 * ENET Status panel 1280x800
 *
 * EJECUTAR EN ORDEN:
 *   1. CreatePanel()   → borra anterior y crea panel base
 *   2. CreateCol1()    → IP ADDRESS STATUS MCE (izquierda)
 *   3. CreateCol2()    → IP ADDRESS STATUS (centro-izq)
 *   4. CreateCol3()    → IP ADDRESS STATUS (centro-der)
 *   5. CreateCol4()    → IP ADDRESS STATUS (derecha)
 *   6. CreateButtons() → Botones + leyenda
 *
 * CORRECCIONES v2:
 *   - Columnas redistribuidas con espacio correcto
 *   - IP labels más anchos (110px)
 *   - Separación clara entre IP y nombre
 *   - Row height ajustado a 22px para que quepan 30 filas
 */
public class BuildEnetStatusPanel : BaseNetLogic
{
    const int PW = 1280;
    const int PH = 800;
    const int HDR_Y = 95;   // Y del header de columna
    const int DATA_Y = 135; // Y donde empiezan los datos
    const int ROW_H = 21;   // Altura de cada fila
    const int IP_W = 105;   // Ancho del campo IP
    const int GAP = 8;      // Espacio entre IP y nombre

    // Posiciones X de cada columna
    const int C1_X = 15;
    const int C2_X = 295;
    const int C3_X = 585;
    const int C4_X = 895;

    // Anchos de nombre por columna
    const int C1_NAME_W = 145;
    const int C2_NAME_W = 160;
    const int C3_NAME_W = 175;
    const int C4_NAME_W = 165;

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

        var existing = uiRoot.Get("EnetStatusPanel");
        if (existing != null) uiRoot.Remove(existing);

        var panel = InformationModel.MakeObject<Panel>("EnetStatusPanel");
        panel.Width = PW; panel.Height = PH;

        // Fondo gris
        var bg = InformationModel.MakeObject<Rectangle>("Background");
        bg.HorizontalAlignment = HorizontalAlignment.Stretch;
        bg.VerticalAlignment   = VerticalAlignment.Stretch;
        bg.LeftMargin = 0; bg.TopMargin = 0; bg.RightMargin = 0; bg.BottomMargin = 0;
        SP(bg, "FillColor", COL_BG);
        SP(bg, "BorderThickness", new UAValue(0));
        panel.Add(bg);

        // Franja roja superior
        var topBar = InformationModel.MakeObject<Rectangle>("TopBar");
        topBar.Width = PW; topBar.Height = 80;
        topBar.TopMargin = 0; topBar.LeftMargin = 0;
        SP(topBar, "FillColor", COL_RED_BG);
        SP(topBar, "BorderThickness", new UAValue(0));
        panel.Add(topBar);

        // "Pantalla # ****"
        AddLbl(panel, "LblPantalla", "Pantalla #  ****", 10, 50, 160, 22, COL_WHITE, 11, false);

        uiRoot.Add(panel);
        Log.Info("BuildEnetStatusPanel", "1/6 Panel base creado.");
    }

    // ════════════════════════════════════════════════════════════════
    //  2. Columna 1 — STATUS MCE
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreateCol1()
    {
        var panel = GetPanel(); if (panel == null) return;

        // Header
        AddLbl(panel, "C1H1", "IP ADDRESS",  C1_X, HDR_Y,      260, 16, COL_BLACK, 11, true);
        AddLbl(panel, "C1H2", "STATUS MCE",  C1_X, HDR_Y + 16, 260, 16, COL_BLACK, 11, true);

        string[] names = {
            "SUBNET MASK", "GATEWAY", "MCE EN2TR-1", "MCE ENBT",
            "SPARE", "ETAP", "SPARE", "SPARE", "LAPTOP", "SPARE"
        };

        for (int i = 0; i < names.Length; i++)
        {
            int y = DATA_Y + i * ROW_H;
            AddLbl(panel, "C1_IP_" + i,   "???????????", C1_X, y, IP_W, ROW_H, COL_BLUE, 10, false);
            AddLbl(panel, "C1_Name_" + i,  names[i], C1_X + IP_W + GAP, y, C1_NAME_W, ROW_H, COL_BLACK, 10, true);
        }

        Log.Info("BuildEnetStatusPanel", "2/6 Columna 1 creada.");
    }

    // ════════════════════════════════════════════════════════════════
    //  3. Columna 2
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreateCol2()
    {
        var panel = GetPanel(); if (panel == null) return;

        AddLbl(panel, "C2H1", "IP ADDRESS", C2_X, HDR_Y,      270, 16, COL_BLACK, 11, true);
        AddLbl(panel, "C2H2", "STATUS",     C2_X, HDR_Y + 16, 270, 16, COL_BLACK, 11, true);

        string[] names = {
            "BPE ETAP", "BPE ENET SWITCH", "BPE SAFE I/O", "SPARE",
            "SPARE", "HMI", "HMI POINT I/O", "SPARE",
            "SPARE", "OSE-01 SAFE I/O", "OSE-02 SAFE I/O",
            "OSE-03 SAFE I/O", "OSE-04 SAFE I/O", "SPARE",
            "SPARE", "TTE CONTROL", "TTE POINT I/O", "SPARE",
            "SPARE", "PSE-01 POINT I/O", "PSE-02 POINT I/O",
            "PSE-03 POINT I/O", "PSE-04 POINT I/O", "PSE-05 POINT I/O",
            "PSE-06 POINT I/O", "TOOLING STA10", "TOOLING STA20",
            "TOOLING STA30", "TOOLING STA40", "SPARE"
        };

        for (int i = 0; i < names.Length; i++)
        {
            int y = DATA_Y + i * ROW_H;
            AddLbl(panel, "C2_IP_" + i,   "???????????", C2_X, y, IP_W, ROW_H, COL_BLUE, 10, false);
            AddLbl(panel, "C2_Name_" + i,  names[i], C2_X + IP_W + GAP, y, C2_NAME_W, ROW_H, COL_BLACK, 10, false);
        }

        Log.Info("BuildEnetStatusPanel", "3/6 Columna 2 creada.");
    }

    // ════════════════════════════════════════════════════════════════
    //  4. Columna 3
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreateCol3()
    {
        var panel = GetPanel(); if (panel == null) return;

        AddLbl(panel, "C3H1", "IP ADDRESS", C3_X, HDR_Y,      290, 16, COL_BLACK, 11, true);
        AddLbl(panel, "C3H2", "STATUS",     C3_X, HDR_Y + 16, 290, 16, COL_BLACK, 11, true);

        string[] names = {
            "SPARE", "VISION-1", "VISION-2", "VISION-3", "VISION-4",
            "SPARE", "SPARE", "RSE I/O MODULE", "SPARE",
            "SPARE", "ROLL DOOR SAFE I/O", "SPARE", "SPARE",
            "SPARE", "SPARE", "SPARE", "SPARE",
            "SPARE", "SPARE", "SPARE",
            "R01 CONTROL/SAFE", "R01 POINT I/O (ABB)",
            "R01 TIP DRESS VMF1", "R01 SRW/TD", "R01 EOAT",
            "R01 WELDER", "R01 RPW/SEALER", "R01 PSE",
            "R01 REAMER", "R01 EOAT VLV MF"
        };

        for (int i = 0; i < names.Length; i++)
        {
            int y = DATA_Y + i * ROW_H;
            AddLbl(panel, "C3_IP_" + i,   "???????????", C3_X, y, IP_W, ROW_H, COL_BLUE, 10, false);
            AddLbl(panel, "C3_Name_" + i,  names[i], C3_X + IP_W + GAP, y, C3_NAME_W, ROW_H, COL_BLACK, 10, false);
        }

        Log.Info("BuildEnetStatusPanel", "4/6 Columna 3 creada.");
    }

    // ════════════════════════════════════════════════════════════════
    //  5. Columna 4
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreateCol4()
    {
        var panel = GetPanel(); if (panel == null) return;

        AddLbl(panel, "C4H1", "IP ADDRESS", C4_X, HDR_Y,      280, 16, COL_BLACK, 11, true);
        AddLbl(panel, "C4H2", "STATUS",     C4_X, HDR_Y + 16, 280, 16, COL_BLACK, 11, true);

        string[] names = {
            "R02 CONTROL/SAFE", "R02 POINT I/O (ABB)",
            "R02 TIP DRESS VMF1", "R02 SRW/TD", "R02 EOAT",
            "R02 WELDER", "R02 RPW/SEALER", "R02 PSE",
            "R02 REAMER", "R02 EOAT VLV MF",
            "R03 CONTROL/SAFE", "R03 POINT I/O (ABB)",
            "R03 TIP DRESS VMF1", "R03 SRW/TD", "R03 EOAT",
            "R03 WELDER", "R03 RPW/SEALER", "R03 PSE",
            "R03 REAMER", "R03 EOAT VLV MF",
            "R04 CONTROL/SAFE", "R04 POINT I/O (ABB)",
            "R04 TIP DRESS VMF1", "R04 SRW/TD", "R04 EOAT",
            "R04 WELDER", "R04 RPW/SEALER", "R04 PSE",
            "R04 REAMER", "R04 EOAT VLV MF"
        };

        for (int i = 0; i < names.Length; i++)
        {
            int y = DATA_Y + i * ROW_H;
            AddLbl(panel, "C4_IP_" + i,   "???????????", C4_X, y, IP_W, ROW_H, COL_BLUE, 10, false);
            AddLbl(panel, "C4_Name_" + i,  names[i], C4_X + IP_W + GAP, y, C4_NAME_W, ROW_H, COL_BLACK, 10, false);
        }

        Log.Info("BuildEnetStatusPanel", "5/6 Columna 4 creada.");
    }

    // ════════════════════════════════════════════════════════════════
    //  6. Botones y leyenda
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreateButtons()
    {
        var panel = GetPanel(); if (panel == null) return;

        int bottomY = 720;

        // Botón "ENET PAGE 2"
        var btnP2 = InformationModel.MakeObject<Button>("BtnEnetPage2");
        btnP2.Width = 110; btnP2.Height = 50;
        btnP2.TopMargin = bottomY; btnP2.LeftMargin = 285;
        btnP2.Text = "ENET\nPAGE 2";
        SP(btnP2, "BackgroundColor", COL_BLUE);
        SP(btnP2, "TextColor", COL_WHITE);
        SP(btnP2, "FontSize", new UAValue(12));
        SP(btnP2, "FontWeight", new UAValue(700));
        SP(btnP2, "WordWrap", new UAValue(true));
        panel.Add(btnP2);

        // 4 rectángulos de estatus vacíos
        for (int i = 0; i < 4; i++)
        {
            var rect = InformationModel.MakeObject<Rectangle>("StatusBox_" + i);
            rect.Width = 80; rect.Height = 50;
            rect.TopMargin = bottomY; rect.LeftMargin = 420 + i * 100;
            SP(rect, "FillColor", COL_WHITE);
            SP(rect, "BorderColor", COL_BLACK);
            SP(rect, "BorderThickness", new UAValue(1));
            panel.Add(rect);
        }

        // ── LEYENDA ──
        int legX = 960;
        AddLbl(panel, "LblLeyenda", "LEYENDA", legX, bottomY - 25, 130, 20, COL_BLACK, 11, true);

        // NO USADO
        var rNU = InformationModel.MakeObject<Rectangle>("LegNoUsado");
        rNU.Width = 130; rNU.Height = 22; rNU.TopMargin = bottomY; rNU.LeftMargin = legX;
        SP(rNU, "FillColor", COL_WHITE); SP(rNU, "BorderColor", COL_BLUE); SP(rNU, "BorderThickness", new UAValue(2));
        panel.Add(rNU);
        AddLbl(panel, "LblNoUsado", "NO USADO", legX, bottomY, 130, 22, COL_BLACK, 10, false);

        // ACTIVO
        var rAC = InformationModel.MakeObject<Rectangle>("LegActivo");
        rAC.Width = 130; rAC.Height = 22; rAC.TopMargin = bottomY + 24; rAC.LeftMargin = legX;
        SP(rAC, "FillColor", COL_WHITE); SP(rAC, "BorderColor", COL_GREEN); SP(rAC, "BorderThickness", new UAValue(2));
        panel.Add(rAC);
        AddLbl(panel, "LblActivo", "ACTIVO", legX, bottomY + 24, 130, 22, COL_GREEN, 10, true);

        // FALLA
        var rFA = InformationModel.MakeObject<Rectangle>("LegFalla");
        rFA.Width = 130; rFA.Height = 22; rFA.TopMargin = bottomY + 48; rFA.LeftMargin = legX;
        SP(rFA, "FillColor", COL_WHITE); SP(rFA, "BorderColor", COL_RED); SP(rFA, "BorderThickness", new UAValue(2));
        panel.Add(rFA);
        AddLbl(panel, "LblFalla", "FALLA", legX, bottomY + 48, 130, 22, COL_RED, 10, true);

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

        Log.Info("BuildEnetStatusPanel", "6/6 Botones y leyenda creados. ¡Panel completo!");
    }

    // ════════════════════════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════════════════════════
    private IUANode GetPanel()
    {
        var p = Project.Current.Get("UI/EnetStatusPanel");
        if (p == null) Log.Error("BuildEnetStatusPanel", "Ejecuta CreatePanel() primero.");
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
