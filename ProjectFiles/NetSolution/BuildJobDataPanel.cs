#region Using directives
using System;
using FTOptix.Core;
using FTOptix.CoreBase;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.UI;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
#endregion

/*
 * BuildJobDataPanel — Design-time NetLogic
 * Pantalla "CURRENT JOB DATA" con Style/Option Information
 * 1280x1024, fondo gris
 *
 * EJECUTAR EN ORDEN:
 *   1. CreatePanel()  → Panel base + header CURRENT JOB DATA
 *   2. CreateTables() → Tablas STYLE INFORMATION y OPTION INFORMATION + botones
 */
public class BuildJobDataPanel : BaseNetLogic
{
    const int PW = 1280;
    const int PH = 1024;

    static readonly Color COL_BG       = new Color(0xFF, 0xC0, 0xC0, 0xC0); // Gris fondo
    static readonly Color COL_DARK_BG  = new Color(0xFF, 0xA0, 0xA0, 0xA0); // Gris oscuro paneles
    static readonly Color COL_CYAN     = new Color(0xFF, 0x00, 0xCC, 0xFF); // Celdas cyan
    static readonly Color COL_BLUE     = new Color(0xFF, 0x00, 0x00, 0xCC);
    static readonly Color COL_WHITE    = new Color(0xFF, 0xFF, 0xFF, 0xFF);
    static readonly Color COL_BLACK    = new Color(0xFF, 0x00, 0x00, 0x00);

    // ════════════════════════════════════════════════════════════════
    //  1. Panel base + Current Job Data header
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreatePanel()
    {
        var uiRoot = Project.Current.Get("UI");
        if (uiRoot == null) return;

        var existing = uiRoot.Get("JobDataPanel");
        if (existing != null) uiRoot.Remove(existing);

        var panel = InformationModel.MakeObject<Panel>("JobDataPanel");
        panel.Width = PW; panel.Height = PH;

        // Fondo gris
        var bg = InformationModel.MakeObject<Rectangle>("Background");
        bg.HorizontalAlignment = HorizontalAlignment.Stretch;
        bg.VerticalAlignment = VerticalAlignment.Stretch;
        bg.LeftMargin = 0; bg.TopMargin = 0; bg.RightMargin = 0; bg.BottomMargin = 0;
        SP(bg, "FillColor", COL_BG);
        SP(bg, "BorderThickness", new UAValue(0));
        panel.Add(bg);

        // ── Recuadro "CURRENT JOB DATA" ──
        var jobBox = InformationModel.MakeObject<Rectangle>("JobDataBox");
        jobBox.Width = 700; jobBox.Height = 120;
        jobBox.TopMargin = 30; jobBox.LeftMargin = 170;
        SP(jobBox, "FillColor", COL_DARK_BG);
        SP(jobBox, "BorderColor", COL_BLACK);
        SP(jobBox, "BorderThickness", new UAValue(2));
        panel.Add(jobBox);

        // Título
        AddLbl(panel, "LblJobTitle", "CURRENT JOB DATA", 170, 40, 700, 30, COL_BLACK, 18, true, 1);

        // STYLE label + valor
        AddLbl(panel, "LblStyleTitle", "STYLE", 420, 80, 100, 22, COL_BLACK, 14, true, 1);
        var styleVal = InformationModel.MakeObject<Rectangle>("StyleValBox");
        styleVal.Width = 60; styleVal.Height = 28;
        styleVal.TopMargin = 105; styleVal.LeftMargin = 440;
        SP(styleVal, "FillColor", COL_CYAN);
        SP(styleVal, "BorderColor", COL_BLACK);
        SP(styleVal, "BorderThickness", new UAValue(1));
        panel.Add(styleVal);
        AddLbl(panel, "LblStyleVal", "**", 440, 108, 60, 22, COL_BLACK, 12, false, 1);

        // OPTION label + valor
        AddLbl(panel, "LblOptionTitle", "OPTION", 550, 80, 100, 22, COL_BLACK, 14, true, 1);
        var optVal = InformationModel.MakeObject<Rectangle>("OptionValBox");
        optVal.Width = 60; optVal.Height = 28;
        optVal.TopMargin = 105; optVal.LeftMargin = 565;
        SP(optVal, "FillColor", COL_CYAN);
        SP(optVal, "BorderColor", COL_BLACK);
        SP(optVal, "BorderThickness", new UAValue(1));
        panel.Add(optVal);
        AddLbl(panel, "LblOptionVal", "**", 565, 108, 60, 22, COL_BLACK, 12, false, 1);

        uiRoot.Add(panel);
        Log.Info("BuildJobDataPanel", "1/2 Panel base creado.");
    }

    // ════════════════════════════════════════════════════════════════
    //  2. Tablas Style/Option + botones
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreateTables()
    {
        var panel = Project.Current.Get("UI/JobDataPanel");
        if (panel == null) { Log.Error("BuildJobDataPanel", "Ejecuta CreatePanel() primero."); return; }

        // ── Recuadro contenedor de tablas ──
        var tableBox = InformationModel.MakeObject<Rectangle>("TableBox");
        tableBox.Width = 750; tableBox.Height = 380;
        tableBox.TopMargin = 175; tableBox.LeftMargin = 145;
        SP(tableBox, "FillColor", COL_DARK_BG);
        SP(tableBox, "BorderColor", COL_BLACK);
        SP(tableBox, "BorderThickness", new UAValue(2));
        panel.Add(tableBox);

        // ── Headers ──
        AddLbl(panel, "LblStyleInfo", "STYLE INFORMATION", 200, 185, 300, 25, COL_BLACK, 14, true, 1);
        AddLbl(panel, "LblOptionInfo", "OPTION INFORMATION", 600, 185, 280, 25, COL_BLACK, 14, true, 1);

        // ── STYLE INFORMATION — 10 filas ──
        string[] styles = {
            "AU426 ESTILO 1", "AU436 ESTILO 2",
            "STYLE PART DATA-3", "STYLE PART DATA-4", "STYLE PART DATA-5",
            "STYLE PART DATA-6", "STYLE PART DATA-7", "STYLE PART DATA-8",
            "STYLE PART DATA-9", "STYLE PART DATA-10"
        };

        int tableY = 220;
        int rowH = 30;
        int numX = 175;    // Número
        int numW = 30;
        int cellX = 207;   // Celda cyan
        int cellW = 290;

        for (int i = 0; i < 10; i++)
        {
            int y = tableY + i * (rowH + 3);
            string idx = (i + 1).ToString("D2");

            // Número
            AddLbl(panel, "StyleNum_" + i, idx, numX, y, numW, rowH, COL_BLACK, 12, true, 2);

            // Celda cyan
            var cell = InformationModel.MakeObject<Rectangle>("StyleCell_" + i);
            cell.Width = cellW; cell.Height = rowH;
            cell.TopMargin = y; cell.LeftMargin = cellX;
            SP(cell, "FillColor", COL_CYAN);
            SP(cell, "BorderColor", COL_BLACK);
            SP(cell, "BorderThickness", new UAValue(1));
            panel.Add(cell);

            // Texto
            AddLbl(panel, "StyleText_" + i, styles[i], cellX + 5, y, cellW - 10, rowH, COL_BLACK, 11, false, 0);
        }

        // ── OPTION INFORMATION — 10 filas ──
        string[] options = {
            "Standard", "OPTION PART DATA-2",
            "OPTION PART DATA-3", "OPTION PART DATA-4", "OPTION PART DATA-5",
            "OPTION PART DATA-6", "OPTION PART DATA-7", "OPTION PART DATA-8",
            "OPTION PART DATA-9", "OPTION PART DATA-10"
        };

        int optNumX = 535;
        int optCellX = 567;
        int optCellW = 290;

        for (int i = 0; i < 10; i++)
        {
            int y = tableY + i * (rowH + 3);
            string idx = (i + 1).ToString("D2");

            // Número
            AddLbl(panel, "OptNum_" + i, idx, optNumX, y, numW, rowH, COL_BLACK, 12, true, 2);

            // Celda cyan
            var cell = InformationModel.MakeObject<Rectangle>("OptCell_" + i);
            cell.Width = optCellW; cell.Height = rowH;
            cell.TopMargin = y; cell.LeftMargin = optCellX;
            SP(cell, "FillColor", COL_CYAN);
            SP(cell, "BorderColor", COL_BLACK);
            SP(cell, "BorderThickness", new UAValue(1));
            panel.Add(cell);

            // Texto
            AddLbl(panel, "OptText_" + i, options[i], optCellX + 5, y, optCellW - 10, rowH, COL_BLACK, 11, false, 0);
        }

        // ── Botón navegación circular azul — abajo derecha ──
        var btnNav = InformationModel.MakeObject<Button>("BtnNavCircle");
        btnNav.Width = 65; btnNav.Height = 65;
        btnNav.TopMargin = 880; btnNav.LeftMargin = 1170;
        btnNav.Text = "▲";
        SP(btnNav, "BackgroundColor", COL_BLUE);
        SP(btnNav, "TextColor", COL_WHITE);
        SP(btnNav, "FontSize", new UAValue(22));
        SP(btnNav, "Appearance", new UAValue("Bordered Circular"));
        panel.Add(btnNav);

        // ── Botón INICIO — abajo derecha ──
        var btnInicio = InformationModel.MakeObject<Button>("BtnInicio");
        btnInicio.Width = 75; btnInicio.Height = 40;
        btnInicio.TopMargin = 955; btnInicio.LeftMargin = 1165;
        btnInicio.Text = "INICIO";
        SP(btnInicio, "BackgroundColor", COL_BLUE);
        SP(btnInicio, "TextColor", COL_WHITE);
        SP(btnInicio, "FontSize", new UAValue(12));
        SP(btnInicio, "FontWeight", new UAValue(700));
        panel.Add(btnInicio);

        Log.Info("BuildJobDataPanel", "2/2 Tablas y botones creados. Panel completo.");
    }

    // ════════════════════════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════════════════════════
    // align: 0=Left, 1=Center, 2=Right
    private void AddLbl(IUANode parent, string name, string text,
        int left, int top, int width, int height,
        Color textColor, int fontSize, bool bold, int align)
    {
        var lbl = InformationModel.MakeObject<Label>(name);
        lbl.Width = width; lbl.Height = height;
        lbl.TopMargin = top; lbl.LeftMargin = left;
        lbl.Text = text;
        SP(lbl, "TextColor", textColor);
        SP(lbl, "FontSize", new UAValue(fontSize));
        if (bold) SP(lbl, "FontWeight", new UAValue(700));
        SP(lbl, "TextHorizontalAlignment", new UAValue(align));
        SP(lbl, "TextVerticalAlignment", new UAValue(1));
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