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
 * BuildProductionAyerPanel — Design-time NetLogic
 * Migración de FT View "PRODUCCION DE AYER" a FactoryTalk Optix
 * Panel 1280x1024, fondo gris, bajo UI como Panel (type)
 *
 * Diferencias vs ProductionPanel (hoy):
 *   - Título: "PRODUCCION DE AYER" (no "POR HORA")
 *   - Botón superior izquierdo: "PRODUCCCION DE HOY" (navega de vuelta)
 *   - Turnos: "PRIMER TURNO DE AYER", "SEGUNDO TURNO DE AYER", "TERCER TURNO DE AYER"
 *   - Sin reloj, sin botón "FRESADO DE ROBOTS", sin logo
 *   - Variables en Model/ProductionAyer/
 */
public class BuildProductionAyerPanel : BaseNetLogic
{
    const int PANEL_W = 1280;
    const int PANEL_H = 1024;
    const int BAR_COUNT = 24;

    static readonly Color COL_BG_GRAY    = new Color(0xFF, 0x80, 0x80, 0x80);
    static readonly Color COL_BLUE       = new Color(0xFF, 0x00, 0x00, 0xCC);
    static readonly Color COL_WHITE      = new Color(0xFF, 0xFF, 0xFF, 0xFF);
    static readonly Color COL_BLACK      = new Color(0xFF, 0x00, 0x00, 0x00);
    static readonly Color COL_GREEN      = new Color(0xFF, 0x00, 0x80, 0x00);
    static readonly Color COL_BAR_GRAY   = new Color(0xFF, 0xC0, 0xC0, 0xC0);
    static readonly Color COL_BAR_BORDER = new Color(0xFF, 0x99, 0x99, 0x66);

    // ════════════════════════════════════════════════════════════════
    //  PASO 1: Modelo de datos
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreateModel()
    {
        var model = Project.Current.Get("Model");
        if (model == null) { Log.Error("BuildProductionAyerPanel", "No se encontró Model."); return; }

        var prod = model.Get("ProductionAyer");
        if (prod == null) prod = CreateFolder(model, "ProductionAyer");

        for (int i = 0; i < BAR_COUNT; i++)
            AddVarIfMissing(prod, "Hour_" + i, OpcUa.DataTypes.Int32, new UAValue(0));

        for (int i = 0; i < BAR_COUNT; i++)
            AddVarIfMissing(prod, "Count_" + i, OpcUa.DataTypes.Int32, new UAValue(0));

        AddVarIfMissing(prod, "TurnoPrimeroAyer",  OpcUa.DataTypes.Int32, new UAValue(0));
        AddVarIfMissing(prod, "TurnoSegundoAyer",  OpcUa.DataTypes.Int32, new UAValue(0));
        AddVarIfMissing(prod, "TurnoTerceroAyer",  OpcUa.DataTypes.Int32, new UAValue(0));

        Log.Info("BuildProductionAyerPanel", "Modelo creado: Model/ProductionAyer/");
    }

    // ════════════════════════════════════════════════════════════════
    //  PASO 2: UI
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreateUI()
    {
        var uiRoot = Project.Current.Get("UI");
        if (uiRoot == null) { Log.Error("BuildProductionAyerPanel", "No se encontró UI."); return; }

        var existing = uiRoot.Get("ProductionAyerPanel");
        if (existing != null) uiRoot.Remove(existing);

        var prodNode = Project.Current.Get("Model/ProductionAyer");
        if (prodNode == null) { Log.Error("BuildProductionAyerPanel", "Ejecuta CreateModel() primero."); return; }

        // ── Panel principal ──
        var panel = InformationModel.MakeObject<Panel>("ProductionAyerPanel");
        panel.Width  = PANEL_W;
        panel.Height = PANEL_H;

        // ── Fondo gris ──
        var bg = InformationModel.MakeObject<Rectangle>("Background");
        bg.HorizontalAlignment = HorizontalAlignment.Stretch;
        bg.VerticalAlignment   = VerticalAlignment.Stretch;
        bg.LeftMargin = 0; bg.TopMargin = 0; bg.RightMargin = 0; bg.BottomMargin = 0;
        SetProp(bg, "FillColor", COL_BG_GRAY);
        SetProp(bg, "BorderThickness", new UAValue(0));
        panel.Add(bg);

        // ══════════════════════════════════════════════════════════
        //  HEADER
        // ══════════════════════════════════════════════════════════

        // Botón "PRODUCCCION DE HOY" — navega de vuelta a producción de hoy
        var btnHoy = MakeButton("BtnProduccionHoy", "PRODUCCCION\nDE HOY",
            55, 30, 160, 60, COL_BLUE, COL_WHITE, 12);
        panel.Add(btnHoy);

        // Título "AU426 01"
        var lblCell = InformationModel.MakeObject<Label>("LblCellName");
        lblCell.Width = 300; lblCell.Height = 40;
        lblCell.TopMargin = 45; lblCell.LeftMargin = 490;
        lblCell.Text = "AU426 01";
        SetProp(lblCell, "TextColor", COL_BLACK);
        SetProp(lblCell, "FontSize", new UAValue(24));
        SetProp(lblCell, "FontWeight", new UAValue(700));
        SetProp(lblCell, "TextHorizontalAlignment", new UAValue(1));
        panel.Add(lblCell);

        // Botón "MAIN"
        var btnMain = MakeButton("BtnMain", "MAIN",
            1070, 30, 120, 60, COL_BLUE, COL_WHITE, 14);
        panel.Add(btnMain);

        // ══════════════════════════════════════════════════════════
        //  TÍTULO "PRODUCCION DE AYER"
        // ══════════════════════════════════════════════════════════
        var lblTitle = InformationModel.MakeObject<Label>("LblTitle");
        lblTitle.Width = 600; lblTitle.Height = 50;
        lblTitle.TopMargin = 120; lblTitle.LeftMargin = 340;
        lblTitle.Text = "PRODUCCION DE AYER";
        SetProp(lblTitle, "TextColor", COL_BLACK);
        SetProp(lblTitle, "FontSize", new UAValue(28));
        SetProp(lblTitle, "FontWeight", new UAValue(700));
        SetProp(lblTitle, "FontUnderline", new UAValue(true));
        SetProp(lblTitle, "TextHorizontalAlignment", new UAValue(1));
        panel.Add(lblTitle);

        // ══════════════════════════════════════════════════════════
        //  "HORA"
        // ══════════════════════════════════════════════════════════
        var lblHora = InformationModel.MakeObject<Label>("LblHora");
        lblHora.Width = 60; lblHora.Height = 25;
        lblHora.TopMargin = 200; lblHora.LeftMargin = 30;
        lblHora.Text = "HORA";
        SetProp(lblHora, "TextColor", COL_BLACK);
        SetProp(lblHora, "FontSize", new UAValue(12));
        SetProp(lblHora, "FontItalic", new UAValue(true));
        panel.Add(lblHora);

        // ══════════════════════════════════════════════════════════
        //  24 BARRAS + LABELS HORA + CELDAS VERDES
        // ══════════════════════════════════════════════════════════
        int barStartX = 95;
        int barY      = 230;
        int barW      = 42;
        int barH      = 320;
        int barGap    = 3;
        int hourLabelY = 200;
        int greenCellY = 565;
        int greenCellH = 35;

        for (int i = 0; i < BAR_COUNT; i++)
        {
            int x = barStartX + i * (barW + barGap);

            // Label de hora
            var lblH = InformationModel.MakeObject<Label>("LblHour_" + i);
            lblH.Width = barW; lblH.Height = 25;
            lblH.TopMargin = hourLabelY; lblH.LeftMargin = x;
            lblH.Text = i.ToString();
            SetProp(lblH, "TextColor", COL_BLACK);
            SetProp(lblH, "FontSize", new UAValue(12));
            SetProp(lblH, "FontWeight", new UAValue(700));
            SetProp(lblH, "TextHorizontalAlignment", new UAValue(1));
            panel.Add(lblH);

            // Barra gris
            var bar = InformationModel.MakeObject<Rectangle>("Bar_" + i);
            bar.Width = barW; bar.Height = barH;
            bar.TopMargin = barY; bar.LeftMargin = x;
            SetProp(bar, "FillColor", COL_BAR_GRAY);
            SetProp(bar, "BorderColor", COL_BAR_BORDER);
            SetProp(bar, "BorderThickness", new UAValue(1));
            panel.Add(bar);

            // Celda verde
            var cell = InformationModel.MakeObject<Rectangle>("GreenCell_" + i);
            cell.Width = barW; cell.Height = greenCellH;
            cell.TopMargin = greenCellY; cell.LeftMargin = x;
            SetProp(cell, "FillColor", COL_GREEN);
            SetProp(cell, "BorderColor", COL_GREEN);
            SetProp(cell, "BorderThickness", new UAValue(1));
            panel.Add(cell);

            // Label valor
            var lblVal = InformationModel.MakeObject<Label>("LblCount_" + i);
            lblVal.Width = barW; lblVal.Height = greenCellH;
            lblVal.TopMargin = greenCellY; lblVal.LeftMargin = x;
            lblVal.Text = "0";
            SetProp(lblVal, "TextColor", COL_WHITE);
            SetProp(lblVal, "FontSize", new UAValue(10));
            SetProp(lblVal, "TextHorizontalAlignment", new UAValue(1));
            SetProp(lblVal, "TextVerticalAlignment", new UAValue(1));
            panel.Add(lblVal);

            // Dynamic link
            try
            {
                var countVar = Project.Current.GetVariable("Model/ProductionAyer/Count_" + i);
                if (countVar != null)
                    lblVal.TextVariable.SetDynamicLink(countVar, DynamicLinkMode.Read);
            }
            catch (Exception ex)
            {
                Log.Warning("BuildProductionAyerPanel", "Link Count_" + i + ": " + ex.Message);
            }
        }

        // ══════════════════════════════════════════════════════════
        //  TURNOS DE AYER
        // ══════════════════════════════════════════════════════════
        string[] turnoLabels = {
            "PRIMER TURNO DE AYER",
            "SEGUNDO TURNO DE AYER",
            "TERCER TURNO DE AYER"
        };
        string[] turnoVars = { "TurnoPrimeroAyer", "TurnoSegundoAyer", "TurnoTerceroAyer" };
        int turnoY = 650;

        for (int t = 0; t < 3; t++)
        {
            int y = turnoY + t * 45;

            var lblT = InformationModel.MakeObject<Label>("LblTurno_" + t);
            lblT.Width = 300; lblT.Height = 35;
            lblT.TopMargin = y; lblT.LeftMargin = 60;
            lblT.Text = turnoLabels[t];
            SetProp(lblT, "TextColor", COL_WHITE);
            SetProp(lblT, "FontSize", new UAValue(14));
            SetProp(lblT, "FontWeight", new UAValue(700));
            panel.Add(lblT);

            var valRect = InformationModel.MakeObject<Rectangle>("TurnoRect_" + t);
            valRect.Width = 80; valRect.Height = 35;
            valRect.TopMargin = y; valRect.LeftMargin = 370;
            SetProp(valRect, "FillColor", COL_BLUE);
            SetProp(valRect, "BorderColor", COL_BLUE);
            panel.Add(valRect);

            var lblTV = InformationModel.MakeObject<Label>("LblTurnoVal_" + t);
            lblTV.Width = 80; lblTV.Height = 35;
            lblTV.TopMargin = y; lblTV.LeftMargin = 370;
            lblTV.Text = "0";
            SetProp(lblTV, "TextColor", COL_WHITE);
            SetProp(lblTV, "FontSize", new UAValue(14));
            SetProp(lblTV, "TextHorizontalAlignment", new UAValue(1));
            SetProp(lblTV, "TextVerticalAlignment", new UAValue(1));
            panel.Add(lblTV);

            try
            {
                var tVar = Project.Current.GetVariable("Model/ProductionAyer/" + turnoVars[t]);
                if (tVar != null)
                    lblTV.TextVariable.SetDynamicLink(tVar, DynamicLinkMode.Read);
            }
            catch (Exception ex)
            {
                Log.Warning("BuildProductionAyerPanel", "Link " + turnoVars[t] + ": " + ex.Message);
            }
        }

        // ══════════════════════════════════════════════════════════
        //  BOTÓN circular azul (navegación)
        // ══════════════════════════════════════════════════════════
        var btnNav = InformationModel.MakeObject<Button>("BtnNavCircle");
        btnNav.Width = 70; btnNav.Height = 70;
        btnNav.TopMargin = 700; btnNav.LeftMargin = 780;
        btnNav.Text = "▲";
        SetProp(btnNav, "BackgroundColor", COL_BLUE);
        SetProp(btnNav, "TextColor", COL_WHITE);
        SetProp(btnNav, "FontSize", new UAValue(24));
        SetProp(btnNav, "Appearance", new UAValue("Bordered Circular"));
        panel.Add(btnNav);

        // ── Agregar al UI ──
        uiRoot.Add(panel);

        Log.Info("BuildProductionAyerPanel",
            "UI creada: ProductionAyerPanel. Vincula tags reales a Model/ProductionAyer/.");
    }

    // ════════════════════════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════════════════════════
    private Button MakeButton(string name, string text,
        int left, int top, int width, int height,
        Color bgColor, Color textColor, int fontSize)
    {
        var btn = InformationModel.MakeObject<Button>(name);
        btn.Width = width; btn.Height = height;
        btn.TopMargin = top; btn.LeftMargin = left;
        btn.Text = text;
        SetProp(btn, "BackgroundColor", bgColor);
        SetProp(btn, "TextColor", textColor);
        SetProp(btn, "FontSize", new UAValue(fontSize));
        SetProp(btn, "WordWrap", new UAValue(true));
        return btn;
    }

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
            Log.Warning("BuildProductionAyerPanel", propName + ": " + ex.Message);
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