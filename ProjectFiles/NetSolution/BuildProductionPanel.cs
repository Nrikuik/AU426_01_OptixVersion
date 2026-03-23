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
 * BuildProductionPanel — Design-time NetLogic
 * Migración de FT View "PRODUCCION POR HORA" a FactoryTalk Optix
 * Pantalla 1280x1024, fondo gris, creada bajo UI como Panel (type)
 *
 * ELEMENTOS:
 *   - Header: botón "PRODUCCION DE AYER", título "AU426 01", botón "MAIN", reloj
 *   - Título: "PRODUCCION POR HORA" subrayado
 *   - 24 barras verticales (horas 0–23) con labels de hora
 *   - 24 celdas verdes debajo con valores numéricos (placeholders)
 *   - 3 labels de turno con valores
 *   - Botón circular azul (navegación)
 *   - Botón "FRESADO DE ROBOTS"
 *   - Label logo "Gestamp"
 *
 * USO:
 *   1. Ejecutar CreateModel() → crea variables placeholder
 *   2. Ejecutar CreateUI()    → crea el panel completo bajo UI
 *
 * NOTA: Los tags de comunicación serán vinculados después por el usuario.
 *       Las variables en Model/Production/ son placeholders locales.
 */
public class BuildProductionPanel : BaseNetLogic
{
    // Constantes de diseño
    const int PANEL_W = 1280;
    const int PANEL_H = 1024;
    const int BAR_COUNT = 24;

    // Colores
    static readonly Color COL_BG_GRAY    = new Color(0xFF, 0x80, 0x80, 0x80); // Fondo gris
    static readonly Color COL_BLUE       = new Color(0xFF, 0x00, 0x00, 0xCC); // Botones azules
    static readonly Color COL_WHITE      = new Color(0xFF, 0xFF, 0xFF, 0xFF);
    static readonly Color COL_BLACK      = new Color(0xFF, 0x00, 0x00, 0x00);
    static readonly Color COL_YELLOW_BG  = new Color(0xFF, 0xFF, 0xFF, 0x00); // Reloj
    static readonly Color COL_GREEN      = new Color(0xFF, 0x00, 0x80, 0x00); // Celdas verdes
    static readonly Color COL_BAR_GRAY   = new Color(0xFF, 0xC0, 0xC0, 0xC0); // Barras
    static readonly Color COL_BAR_BORDER = new Color(0xFF, 0x99, 0x99, 0x66); // Borde barras

    // ════════════════════════════════════════════════════════════════
    //  PASO 1: Crear modelo de datos (placeholders)
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreateModel()
    {
        var model = Project.Current.Get("Model");
        if (model == null) { Log.Error("BuildProductionPanel", "No se encontró Model."); return; }

        var prod = model.Get("Production");
        if (prod == null) prod = CreateFolder(model, "Production");

        // 24 variables de producción por hora (barras)
        for (int i = 0; i < BAR_COUNT; i++)
            AddVarIfMissing(prod, "Hour_" + i, OpcUa.DataTypes.Int32, new UAValue(0));

        // 24 variables de conteo por hora (celdas verdes)
        for (int i = 0; i < BAR_COUNT; i++)
            AddVarIfMissing(prod, "Count_" + i, OpcUa.DataTypes.Int32, new UAValue(0));

        // Turnos
        AddVarIfMissing(prod, "TurnoPrimero",  OpcUa.DataTypes.Int32, new UAValue(0));
        AddVarIfMissing(prod, "TurnoSegundo",  OpcUa.DataTypes.Int32, new UAValue(0));
        AddVarIfMissing(prod, "TurnoTercero",  OpcUa.DataTypes.Int32, new UAValue(0));

        // Título de celda
        AddVarIfMissing(prod, "CellName", OpcUa.DataTypes.String, new UAValue("AU426 01"));

        Log.Info("BuildProductionPanel", "Modelo creado: 24 horas + 24 conteos + 3 turnos.");
    }

    // ════════════════════════════════════════════════════════════════
    //  PASO 2: Crear la UI
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreateUI()
    {
        var uiRoot = Project.Current.Get("UI");
        if (uiRoot == null) { Log.Error("BuildProductionPanel", "No se encontró UI."); return; }

        // Limpiar anterior
        var existing = uiRoot.Get("ProductionPanel");
        if (existing != null) uiRoot.Remove(existing);

        // Validar modelo
        var prodNode = Project.Current.Get("Model/Production");
        if (prodNode == null) { Log.Error("BuildProductionPanel", "Ejecuta CreateModel() primero."); return; }

        // ══════════════════════════════════════════════════════════
        //  Panel principal
        // ══════════════════════════════════════════════════════════
        var panel = InformationModel.MakeObject<Panel>("ProductionPanel");
        panel.Width  = PANEL_W;
        panel.Height = PANEL_H;

        // Fondo gris
        var bg = InformationModel.MakeObject<Rectangle>("Background");
        bg.HorizontalAlignment = HorizontalAlignment.Stretch;
        bg.VerticalAlignment   = VerticalAlignment.Stretch;
        bg.LeftMargin = 0; bg.TopMargin = 0; bg.RightMargin = 0; bg.BottomMargin = 0;
        SetProp(bg, "FillColor", COL_BG_GRAY);
        SetProp(bg, "BorderThickness", new UAValue(0));
        panel.Add(bg);

        // ══════════════════════════════════════════════════════════
        //  HEADER — fila superior
        // ══════════════════════════════════════════════════════════

        // Botón "PRODUCCION DE AYER" — azul, arriba izquierda
        var btnAyer = MakeButton("BtnProduccionAyer", "PRODUCCION\nDE AYER",
            55, 30, 140, 55, COL_BLUE, COL_WHITE, 12);
        panel.Add(btnAyer);

        // Título "AU426 01" — centro
        var lblCell = InformationModel.MakeObject<Label>("LblCellName");
        lblCell.Width = 300; lblCell.Height = 40;
        lblCell.TopMargin = 40; lblCell.LeftMargin = 490;
        lblCell.Text = "AU426 01";
        SetProp(lblCell, "TextColor", COL_BLACK);
        SetProp(lblCell, "FontSize", new UAValue(24));
        SetProp(lblCell, "FontWeight", new UAValue(700)); // Bold
        SetProp(lblCell, "TextHorizontalAlignment", new UAValue(1)); // Center
        panel.Add(lblCell);

        // Botón "MAIN" — azul, arriba derecha
        var btnMain = MakeButton("BtnMain", "MAIN",
            1070, 30, 120, 55, COL_BLUE, COL_WHITE, 14);
        panel.Add(btnMain);

        // Reloj — fondo amarillo, arriba derecha
        var lblClock = InformationModel.MakeObject<Label>("LblClock");
        lblClock.Width = 200; lblClock.Height = 40;
        lblClock.TopMargin = 100; lblClock.LeftMargin = 980;
        lblClock.Text = "00:00:00 a. m.";
        SetProp(lblClock, "BackgroundColor", COL_YELLOW_BG);
        SetProp(lblClock, "TextColor", COL_BLACK);
        SetProp(lblClock, "FontSize", new UAValue(18));
        SetProp(lblClock, "TextHorizontalAlignment", new UAValue(1));
        panel.Add(lblClock);

        // ══════════════════════════════════════════════════════════
        //  TÍTULO "PRODUCCION POR HORA"
        // ══════════════════════════════════════════════════════════
        var lblTitle = InformationModel.MakeObject<Label>("LblTitle");
        lblTitle.Width = 600; lblTitle.Height = 50;
        lblTitle.TopMargin = 120; lblTitle.LeftMargin = 250;
        lblTitle.Text = "PRODUCCION POR HORA";
        SetProp(lblTitle, "TextColor", COL_BLACK);
        SetProp(lblTitle, "FontSize", new UAValue(28));
        SetProp(lblTitle, "FontWeight", new UAValue(700));
        SetProp(lblTitle, "FontUnderline", new UAValue(true));
        SetProp(lblTitle, "TextHorizontalAlignment", new UAValue(1));
        panel.Add(lblTitle);

        // ══════════════════════════════════════════════════════════
        //  Label "HORA" a la izquierda
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
        //  24 BARRAS + LABELS DE HORA + CELDAS VERDES
        // ══════════════════════════════════════════════════════════
        int barStartX = 95;    // X inicial de las barras
        int barY      = 230;   // Y de las barras
        int barW      = 42;    // Ancho de cada barra
        int barH      = 320;   // Alto máximo de barra
        int barGap    = 3;     // Espacio entre barras

        int hourLabelY = 200;  // Y de los labels de hora (encima de barras)
        int greenCellY = 565;  // Y de las celdas verdes (debajo de barras)
        int greenCellH = 35;   // Alto de las celdas verdes

        for (int i = 0; i < BAR_COUNT; i++)
        {
            int x = barStartX + i * (barW + barGap);

            // ── Label de hora (0, 1, 2 ... 23) ──
            var lblH = InformationModel.MakeObject<Label>("LblHour_" + i);
            lblH.Width = barW; lblH.Height = 25;
            lblH.TopMargin = hourLabelY; lblH.LeftMargin = x;
            lblH.Text = i.ToString();
            SetProp(lblH, "TextColor", COL_BLACK);
            SetProp(lblH, "FontSize", new UAValue(12));
            SetProp(lblH, "FontWeight", new UAValue(700));
            SetProp(lblH, "TextHorizontalAlignment", new UAValue(1));
            panel.Add(lblH);

            // ── Barra gris (Rectangle) ──
            var bar = InformationModel.MakeObject<Rectangle>("Bar_" + i);
            bar.Width  = barW;
            bar.Height = barH;
            bar.TopMargin  = barY;
            bar.LeftMargin = x;
            SetProp(bar, "FillColor", COL_BAR_GRAY);
            SetProp(bar, "BorderColor", COL_BAR_BORDER);
            SetProp(bar, "BorderThickness", new UAValue(1));
            panel.Add(bar);

            // ── Celda verde con valor ──
            var cell = InformationModel.MakeObject<Rectangle>("GreenCell_" + i);
            cell.Width  = barW;
            cell.Height = greenCellH;
            cell.TopMargin  = greenCellY;
            cell.LeftMargin = x;
            SetProp(cell, "FillColor", COL_GREEN);
            SetProp(cell, "BorderColor", COL_GREEN);
            SetProp(cell, "BorderThickness", new UAValue(1));
            panel.Add(cell);

            // Label del valor dentro de la celda verde
            var lblVal = InformationModel.MakeObject<Label>("LblCount_" + i);
            lblVal.Width = barW; lblVal.Height = greenCellH;
            lblVal.TopMargin  = greenCellY;
            lblVal.LeftMargin = x;
            lblVal.Text = "0";
            SetProp(lblVal, "TextColor", COL_WHITE);
            SetProp(lblVal, "FontSize", new UAValue(10));
            SetProp(lblVal, "TextHorizontalAlignment", new UAValue(1)); // Center
            SetProp(lblVal, "TextVerticalAlignment", new UAValue(1));   // Center
            panel.Add(lblVal);

            // ── Dynamic link al placeholder (Count_i) ──
            try
            {
                var countVar = Project.Current.GetVariable("Model/Production/Count_" + i);
                if (countVar != null)
                    lblVal.TextVariable.SetDynamicLink(countVar, DynamicLinkMode.Read);
            }
            catch (Exception ex)
            {
                Log.Warning("BuildProductionPanel", "Link Count_" + i + ": " + ex.Message);
            }
        }

        // ══════════════════════════════════════════════════════════
        //  PRODUCCION POR TURNO — parte inferior izquierda
        // ══════════════════════════════════════════════════════════
        string[] turnoLabels = {
            "PRODUCCION PRIMER TURNO",
            "PRODUCCION SEGUNDO TURNO",
            "PRODUCCION TERCER TURNO"
        };
        string[] turnoVars = { "TurnoPrimero", "TurnoSegundo", "TurnoTercero" };
        int turnoY = 720;

        for (int t = 0; t < 3; t++)
        {
            int y = turnoY + t * 45;

            // Label del turno
            var lblT = InformationModel.MakeObject<Label>("LblTurno_" + t);
            lblT.Width = 350; lblT.Height = 35;
            lblT.TopMargin = y; lblT.LeftMargin = 60;
            lblT.Text = turnoLabels[t];
            SetProp(lblT, "TextColor", COL_WHITE);
            SetProp(lblT, "FontSize", new UAValue(14));
            SetProp(lblT, "FontWeight", new UAValue(700));
            panel.Add(lblT);

            // Valor del turno (rectángulo azul con texto)
            var valRect = InformationModel.MakeObject<Rectangle>("TurnoRect_" + t);
            valRect.Width = 80; valRect.Height = 35;
            valRect.TopMargin = y; valRect.LeftMargin = 420;
            SetProp(valRect, "FillColor", COL_BLUE);
            SetProp(valRect, "BorderColor", COL_BLUE);
            panel.Add(valRect);

            var lblTV = InformationModel.MakeObject<Label>("LblTurnoVal_" + t);
            lblTV.Width = 80; lblTV.Height = 35;
            lblTV.TopMargin = y; lblTV.LeftMargin = 420;
            lblTV.Text = "0";
            SetProp(lblTV, "TextColor", COL_WHITE);
            SetProp(lblTV, "FontSize", new UAValue(14));
            SetProp(lblTV, "TextHorizontalAlignment", new UAValue(1));
            SetProp(lblTV, "TextVerticalAlignment", new UAValue(1));
            panel.Add(lblTV);

            // Dynamic link al placeholder
            try
            {
                var tVar = Project.Current.GetVariable("Model/Production/" + turnoVars[t]);
                if (tVar != null)
                    lblTV.TextVariable.SetDynamicLink(tVar, DynamicLinkMode.Read);
            }
            catch (Exception ex)
            {
                Log.Warning("BuildProductionPanel", "Link " + turnoVars[t] + ": " + ex.Message);
            }
        }

        // ══════════════════════════════════════════════════════════
        //  BOTÓN CIRCULAR azul (navegación) — centro inferior
        // ══════════════════════════════════════════════════════════
        var btnNav = InformationModel.MakeObject<Button>("BtnNavCircle");
        btnNav.Width = 70; btnNav.Height = 70;
        btnNav.TopMargin = 750; btnNav.LeftMargin = 830;
        btnNav.Text = "▲";
        SetProp(btnNav, "BackgroundColor", COL_BLUE);
        SetProp(btnNav, "TextColor", COL_WHITE);
        SetProp(btnNav, "FontSize", new UAValue(24));
        SetProp(btnNav, "Appearance", new UAValue("Bordered Circular"));
        panel.Add(btnNav);

        // ══════════════════════════════════════════════════════════
        //  BOTÓN "FRESADO DE ROBOTS" — abajo derecha
        // ══════════════════════════════════════════════════════════
        var btnFresado = MakeButton("BtnFresado", "FRESADO DE\nROBOTS",
            1050, 750, 140, 55, COL_BLUE, COL_WHITE, 12);
        panel.Add(btnFresado);

        // ══════════════════════════════════════════════════════════
        //  LOGO "Gestamp" — abajo izquierda (texto placeholder)
        // ══════════════════════════════════════════════════════════
        var lblLogo = InformationModel.MakeObject<Label>("LblLogo");
        lblLogo.Width = 150; lblLogo.Height = 40;
        lblLogo.TopMargin = 960; lblLogo.LeftMargin = 20;
        lblLogo.Text = "Gestamp";
        SetProp(lblLogo, "TextColor", COL_WHITE);
        SetProp(lblLogo, "FontSize", new UAValue(22));
        SetProp(lblLogo, "FontWeight", new UAValue(700));
        SetProp(lblLogo, "FontItalic", new UAValue(true));
        panel.Add(lblLogo);

        // ── Agregar panel al UI ──
        uiRoot.Add(panel);

        Log.Info("BuildProductionPanel",
            "UI creada: ProductionPanel (1280x1024) con 24 barras, 3 turnos. " +
            "Vincula los tags reales reemplazando los DynamicLinks a Model/Production/.");
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
            Log.Warning("BuildProductionPanel", propName + ": " + ex.Message);
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