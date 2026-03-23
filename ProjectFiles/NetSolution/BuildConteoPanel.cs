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
 * BuildConteoPanel — Design-time NetLogic
 * Migración de FT View "Monitoreo de Conteo de Puntos" a FactoryTalk Optix
 * Panel bajo UI como tipo reutilizable
 *
 * USO: CreateModel() → CreateUI()
 */
public class BuildConteoPanel : BaseNetLogic
{
    const int PANEL_W = 1280;
    const int PANEL_H = 1024;

    static readonly Color COL_BG        = new Color(0xFF, 0x2D, 0x2D, 0x2D);  // Fondo oscuro
    static readonly Color COL_ORANGE    = new Color(0xFF, 0xFF, 0x66, 0x00);  // Header naranja
    static readonly Color COL_WHITE     = new Color(0xFF, 0xFF, 0xFF, 0xFF);
    static readonly Color COL_BLACK     = new Color(0xFF, 0x00, 0x00, 0x00);
    static readonly Color COL_DARK_GRAY = new Color(0xFF, 0x4A, 0x4A, 0x4A);  // Header tabla
    static readonly Color COL_ROW_LIGHT = new Color(0xFF, 0xC0, 0xC0, 0xC0);  // Fila clara
    static readonly Color COL_ROW_DARK  = new Color(0xFF, 0xA0, 0xA0, 0xA0);  // Fila oscura
    static readonly Color COL_BLUE      = new Color(0xFF, 0x00, 0x00, 0xAA);  // Botón Error
    static readonly Color COL_GRAY_BTN  = new Color(0xFF, 0x99, 0x99, 0x99);  // Botón Reset

    // Robots
    static readonly string[] ROBOTS = { "_80R1", "_80R2", "_90R1", "_100R1", "_100R2", "_110R1" };

    // ════════════════════════════════════════════════════════════════
    //  PASO 1: Modelo
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreateModel()
    {
        var model = Project.Current.Get("Model");
        if (model == null) { Log.Error("BuildConteoPanel", "No se encontró Model."); return; }

        var conteo = model.Get("ConteoPuntos");
        if (conteo == null) conteo = CreateFolder(model, "ConteoPuntos");

        foreach (var robot in ROBOTS)
        {
            var name = robot.Replace(" ", "").TrimStart('_');
            AddVarIfMissing(conteo, "ContadorActual_" + name, OpcUa.DataTypes.String, new UAValue("??? / ???"));
            AddVarIfMissing(conteo, "PuntosOk_" + name,      OpcUa.DataTypes.String, new UAValue("?????"));
            AddVarIfMissing(conteo, "PuntosNoOk_" + name,    OpcUa.DataTypes.String, new UAValue("?????"));
            AddVarIfMissing(conteo, "Error_" + name,         OpcUa.DataTypes.String, new UAValue("Error"));
        }

        Log.Info("BuildConteoPanel", "Modelo creado: Model/ConteoPuntos/ con " + ROBOTS.Length + " robots.");
    }

    // ════════════════════════════════════════════════════════════════
    //  PASO 2: UI
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreateUI()
    {
        var uiRoot = Project.Current.Get("UI");
        if (uiRoot == null) { Log.Error("BuildConteoPanel", "No se encontró UI."); return; }

        var existing = uiRoot.Get("ConteoPanel");
        if (existing != null) uiRoot.Remove(existing);

        var conteoNode = Project.Current.Get("Model/ConteoPuntos");
        if (conteoNode == null) { Log.Error("BuildConteoPanel", "Ejecuta CreateModel() primero."); return; }

        // ── Panel principal ──
        var panel = InformationModel.MakeObject<Panel>("ConteoPanel");
        panel.Width = PANEL_W; panel.Height = PANEL_H;

        // ── Fondo oscuro ──
        var bg = InformationModel.MakeObject<Rectangle>("Background");
        bg.HorizontalAlignment = HorizontalAlignment.Stretch;
        bg.VerticalAlignment   = VerticalAlignment.Stretch;
        bg.LeftMargin = 0; bg.TopMargin = 0; bg.RightMargin = 0; bg.BottomMargin = 0;
        SetProp(bg, "FillColor", COL_BG);
        SetProp(bg, "BorderThickness", new UAValue(0));
        panel.Add(bg);

        // ══════════════════════════════════════════════════════════
        //  HEADER naranja
        // ══════════════════════════════════════════════════════════
        var headerRect = InformationModel.MakeObject<Rectangle>("HeaderBg");
        headerRect.Width = PANEL_W; headerRect.Height = 70;
        headerRect.TopMargin = 0; headerRect.LeftMargin = 0;
        SetProp(headerRect, "FillColor", COL_ORANGE);
        SetProp(headerRect, "BorderThickness", new UAValue(0));
        panel.Add(headerRect);

        // Fecha
        var lblDate = InformationModel.MakeObject<Label>("LblDate");
        lblDate.Width = 120; lblDate.Height = 25;
        lblDate.TopMargin = 10; lblDate.LeftMargin = 15;
        lblDate.Text = "23/03/2026";
        SetProp(lblDate, "TextColor", COL_BLACK);
        SetProp(lblDate, "FontSize", new UAValue(12));
        SetProp(lblDate, "FontWeight", new UAValue(700));
        panel.Add(lblDate);

        // Hora
        var lblTime = InformationModel.MakeObject<Label>("LblTime");
        lblTime.Width = 120; lblTime.Height = 25;
        lblTime.TopMargin = 35; lblTime.LeftMargin = 15;
        lblTime.Text = "00:00:00 a. m.";
        SetProp(lblTime, "TextColor", COL_BLACK);
        SetProp(lblTime, "FontSize", new UAValue(12));
        SetProp(lblTime, "FontWeight", new UAValue(700));
        panel.Add(lblTime);

        // Título
        var lblTitle = InformationModel.MakeObject<Label>("LblTitle");
        lblTitle.Width = 700; lblTitle.Height = 50;
        lblTitle.TopMargin = 10; lblTitle.LeftMargin = 200;
        lblTitle.Text = "Monitoreo de Conteo de Puntos";
        SetProp(lblTitle, "TextColor", COL_WHITE);
        SetProp(lblTitle, "FontSize", new UAValue(28));
        SetProp(lblTitle, "FontWeight", new UAValue(700));
        SetProp(lblTitle, "TextHorizontalAlignment", new UAValue(1));
        panel.Add(lblTitle);

        // ══════════════════════════════════════════════════════════
        //  HEADER DE TABLA
        // ══════════════════════════════════════════════════════════
        int tableX = 30;
        int tableY = 110;
        int rowH   = 45;
        int[] colW = { 100, 160, 150, 160, 170, 150 };  // Anchos de columnas
        string[] headers = { "Robot", "Contador Actual", "Gpo. Puntos Ok", "Gpo. Puntos No Ok", "Reset Conteo Gpos", "" };

        // Fondo header tabla
        int totalW = 0;
        foreach (var w in colW) totalW += w;

        var headerTableBg = InformationModel.MakeObject<Rectangle>("TableHeaderBg");
        headerTableBg.Width = totalW; headerTableBg.Height = rowH;
        headerTableBg.TopMargin = tableY; headerTableBg.LeftMargin = tableX;
        SetProp(headerTableBg, "FillColor", COL_DARK_GRAY);
        SetProp(headerTableBg, "BorderThickness", new UAValue(0));
        panel.Add(headerTableBg);

        // Labels de header
        int hx = tableX;
        for (int c = 0; c < headers.Length; c++)
        {
            if (headers[c] != "")
            {
                var lblH = InformationModel.MakeObject<Label>("LblHeader_" + c);
                lblH.Width = colW[c]; lblH.Height = rowH;
                lblH.TopMargin = tableY; lblH.LeftMargin = hx;
                lblH.Text = headers[c];
                SetProp(lblH, "TextColor", COL_WHITE);
                SetProp(lblH, "FontSize", new UAValue(12));
                SetProp(lblH, "FontWeight", new UAValue(700));
                SetProp(lblH, "TextHorizontalAlignment", new UAValue(1));
                SetProp(lblH, "TextVerticalAlignment", new UAValue(1));
                panel.Add(lblH);
            }
            hx += colW[c];
        }

        // ══════════════════════════════════════════════════════════
        //  FILAS DE DATOS — 6 robots
        // ══════════════════════════════════════════════════════════
        for (int r = 0; r < ROBOTS.Length; r++)
        {
            int ry = tableY + rowH + (r * rowH);
            Color rowColor = (r % 2 == 0) ? COL_ROW_LIGHT : COL_ROW_DARK;
            string robotName = ROBOTS[r].TrimStart('_');

            // Fondo de fila
            var rowBg = InformationModel.MakeObject<Rectangle>("RowBg_" + r);
            rowBg.Width = totalW; rowBg.Height = rowH;
            rowBg.TopMargin = ry; rowBg.LeftMargin = tableX;
            SetProp(rowBg, "FillColor", rowColor);
            SetProp(rowBg, "BorderThickness", new UAValue(0));
            panel.Add(rowBg);

            int cx = tableX;

            // Col 0: Robot name
            var lblRobot = InformationModel.MakeObject<Label>("LblRobot_" + r);
            lblRobot.Width = colW[0]; lblRobot.Height = rowH;
            lblRobot.TopMargin = ry; lblRobot.LeftMargin = cx;
            lblRobot.Text = ROBOTS[r];
            SetProp(lblRobot, "TextColor", COL_BLACK);
            SetProp(lblRobot, "FontSize", new UAValue(12));
            SetProp(lblRobot, "FontWeight", new UAValue(700));
            SetProp(lblRobot, "TextHorizontalAlignment", new UAValue(1));
            SetProp(lblRobot, "TextVerticalAlignment", new UAValue(1));
            panel.Add(lblRobot);
            cx += colW[0];

            // Col 1: Contador Actual
            var lblContador = InformationModel.MakeObject<Label>("LblContador_" + r);
            lblContador.Width = colW[1]; lblContador.Height = rowH;
            lblContador.TopMargin = ry; lblContador.LeftMargin = cx;
            lblContador.Text = "??? / ???";
            SetProp(lblContador, "TextColor", COL_BLACK);
            SetProp(lblContador, "FontSize", new UAValue(12));
            SetProp(lblContador, "TextHorizontalAlignment", new UAValue(1));
            SetProp(lblContador, "TextVerticalAlignment", new UAValue(1));
            LinkVar(lblContador, "Model/ConteoPuntos/ContadorActual_" + robotName);
            panel.Add(lblContador);
            cx += colW[1];

            // Col 2: Gpo. Puntos Ok
            var lblOk = InformationModel.MakeObject<Label>("LblPuntosOk_" + r);
            lblOk.Width = colW[2]; lblOk.Height = rowH;
            lblOk.TopMargin = ry; lblOk.LeftMargin = cx;
            lblOk.Text = "?????";
            SetProp(lblOk, "TextColor", COL_BLACK);
            SetProp(lblOk, "FontSize", new UAValue(12));
            SetProp(lblOk, "TextHorizontalAlignment", new UAValue(1));
            SetProp(lblOk, "TextVerticalAlignment", new UAValue(1));
            LinkVar(lblOk, "Model/ConteoPuntos/PuntosOk_" + robotName);
            panel.Add(lblOk);
            cx += colW[2];

            // Col 3: Gpo. Puntos No Ok
            var lblNoOk = InformationModel.MakeObject<Label>("LblPuntosNoOk_" + r);
            lblNoOk.Width = colW[3]; lblNoOk.Height = rowH;
            lblNoOk.TopMargin = ry; lblNoOk.LeftMargin = cx;
            lblNoOk.Text = "?????";
            SetProp(lblNoOk, "TextColor", COL_BLACK);
            SetProp(lblNoOk, "FontSize", new UAValue(12));
            SetProp(lblNoOk, "TextHorizontalAlignment", new UAValue(1));
            SetProp(lblNoOk, "TextVerticalAlignment", new UAValue(1));
            LinkVar(lblNoOk, "Model/ConteoPuntos/PuntosNoOk_" + robotName);
            panel.Add(lblNoOk);
            cx += colW[3];

            // Col 4: Botón Reset
            var btnReset = InformationModel.MakeObject<Button>("BtnReset_" + r);
            btnReset.Width = colW[4] - 10; btnReset.Height = rowH - 10;
            btnReset.TopMargin = ry + 5; btnReset.LeftMargin = cx + 5;
            btnReset.Text = "Reset - " + robotName;
            SetProp(btnReset, "BackgroundColor", COL_GRAY_BTN);
            SetProp(btnReset, "TextColor", COL_BLACK);
            SetProp(btnReset, "FontSize", new UAValue(11));
            panel.Add(btnReset);
            cx += colW[4];

            // Col 5: Botón Error
            var btnError = InformationModel.MakeObject<Button>("BtnError_" + r);
            btnError.Width = colW[5] - 10; btnError.Height = rowH - 10;
            btnError.TopMargin = ry + 5; btnError.LeftMargin = cx + 5;
            btnError.Text = "Error";
            SetProp(btnError, "BackgroundColor", COL_BLUE);
            SetProp(btnError, "TextColor", COL_WHITE);
            SetProp(btnError, "FontSize", new UAValue(12));
            SetProp(btnError, "FontWeight", new UAValue(700));
            panel.Add(btnError);
        }

        // ══════════════════════════════════════════════════════════
        //  BOTÓN "Principal" — abajo izquierda, naranja
        // ══════════════════════════════════════════════════════════
        var btnPrincipal = InformationModel.MakeObject<Button>("BtnPrincipal");
        btnPrincipal.Width = 110; btnPrincipal.Height = 40;
        btnPrincipal.TopMargin = 950; btnPrincipal.LeftMargin = 20;
        btnPrincipal.Text = "Principal";
        SetProp(btnPrincipal, "BackgroundColor", COL_ORANGE);
        SetProp(btnPrincipal, "TextColor", COL_BLACK);
        SetProp(btnPrincipal, "FontSize", new UAValue(14));
        SetProp(btnPrincipal, "FontWeight", new UAValue(700));
        panel.Add(btnPrincipal);

        // ── Agregar al UI ──
        uiRoot.Add(panel);

        Log.Info("BuildConteoPanel", "UI creada: ConteoPanel con " + ROBOTS.Length + " robots.");
    }

    // ════════════════════════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════════════════════════
    private void LinkVar(Label label, string varPath)
    {
        try
        {
            var v = Project.Current.GetVariable(varPath);
            if (v != null)
                label.TextVariable.SetDynamicLink(v, DynamicLinkMode.Read);
        }
        catch (Exception ex)
        {
            Log.Warning("BuildConteoPanel", "Link " + varPath + ": " + ex.Message);
        }
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
            Log.Warning("BuildConteoPanel", propName + ": " + ex.Message);
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
