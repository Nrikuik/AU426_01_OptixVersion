#region Using directives
using System;
using System.Linq;
using FTOptix.Core;
using FTOptix.CoreBase;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.UI;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
#endregion

/*
 * BuildRobotsLayout v3 — Design-time NetLogic
 * Usa AdvancedSVGImage con SVG inline para dibujar robots
 * Cada robot es UN solo nodo — evita bad allocation
 *
 * EJECUTAR:
 *   1. CreateRobots()          → Robots en Main
 *   2. CreateStations()        → Estaciones en Main
 *   3. CreateRobotsMonitoring()    → Robots en MainMonitoring
 *   4. CreateStationsMonitoring()  → Estaciones en MainMonitoring
 */
public class BuildRobotsLayout : BaseNetLogic
{
    const string PATH_MAIN = "UI/Screens/Main/General/Robots1";
    const string PATH_MON  = "UI/Screens/MainMonitoring/General/Robots1";

    static readonly Color COL_TEAL  = new Color(0xFF, 0x00, 0x80, 0x80);
    static readonly Color COL_WHITE = new Color(0xFF, 0xFF, 0xFF, 0xFF);
    static readonly Color COL_BLACK = new Color(0xFF, 0x00, 0x00, 0x00);
    static readonly Color COL_CELL  = new Color(0xFF, 0xF0, 0xF0, 0xF0);
    static readonly Color COL_GRAY  = new Color(0xFF, 0xD0, 0xD0, 0xD0);

    // ── SVG del robot vista frontal (brazo arriba-derecha, gun holder amarillo) ──
    static string RobotFrontSvg(string name) => @"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 120 140'>
  <rect x='35' y='120' width='50' height='18' rx='3' fill='#D48C28' stroke='#333' stroke-width='1.5'/>
  <rect x='40' y='70' width='40' height='50' rx='5' fill='#D48C28' stroke='#333' stroke-width='1.5'/>
  <circle cx='60' cy='65' r='12' fill='#D48C28' stroke='#333' stroke-width='1.5'/>
  <rect x='52' y='30' width='16' height='35' rx='4' fill='#D48C28' stroke='#333' stroke-width='1.5'/>
  <circle cx='60' cy='28' r='8' fill='#D48C28' stroke='#333' stroke-width='1.5'/>
  <rect x='60' y='22' width='35' height='12' rx='4' fill='#D48C28' stroke='#333' stroke-width='1.5'/>
  <polygon points='95,18 105,10 108,15 98,28 92,30' fill='#D48C28' stroke='#333' stroke-width='1.5'/>
  <line x1='105' y1='10' x2='112' y2='3' stroke='#FFD700' stroke-width='2.5'/>
  <path d='M75,22 Q85,5 95,8' fill='none' stroke='#333' stroke-width='1.5'/>
  <rect x='48' y='5' width='24' height='18' rx='2' fill='#FFD700' stroke='#333' stroke-width='1'/>
  <rect x='52' y='0' width='16' height='8' rx='1' fill='#FFD700' stroke='#333' stroke-width='1'/>
  <rect x='20' y='60' width='80' height='40' rx='5' fill='#CC0000' fill-opacity='0.25' stroke='#CC0000' stroke-width='2'/>
  <text x='60' y='87' text-anchor='middle' font-family='Arial' font-size='18' font-weight='bold' fill='#000'>" + name + @"</text>
</svg>";

    // ── SVG del robot vista lateral (brazo extendido, gun hacia abajo) ──
    static string RobotSideSvg(string name) => @"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 140 120'>
  <rect x='10' y='100' width='50' height='18' rx='3' fill='#D48C28' stroke='#333' stroke-width='1.5'/>
  <rect x='18' y='55' width='35' height='45' rx='5' fill='#D48C28' stroke='#333' stroke-width='1.5'/>
  <circle cx='35' cy='50' r='10' fill='#D48C28' stroke='#333' stroke-width='1.5'/>
  <rect x='35' y='42' width='45' height='14' rx='4' fill='#D48C28' stroke='#333' stroke-width='1.5'/>
  <circle cx='80' cy='49' r='7' fill='#D48C28' stroke='#333' stroke-width='1.5'/>
  <rect x='78' y='49' width='12' height='40' rx='4' fill='#D48C28' stroke='#333' stroke-width='1.5' transform='rotate(-15,84,49)'/>
  <polygon points='88,85 95,90 100,85 95,75' fill='#D48C28' stroke='#333' stroke-width='1.5'/>
  <line x1='95' y1='90' x2='100' y2='98' stroke='#FFD700' stroke-width='2.5'/>
  <path d='M35,40 Q50,25 70,30' fill='none' stroke='#333' stroke-width='1.5'/>
  <rect x='22' y='35' width='18' height='15' rx='2' fill='#FFD700' stroke='#333' stroke-width='1'/>
  <rect x='26' y='28' width='10' height='9' rx='1' fill='#FFD700' stroke='#333' stroke-width='1'/>
  <rect x='5' y='45' width='85' height='40' rx='5' fill='#CC0000' fill-opacity='0.25' stroke='#CC0000' stroke-width='2'/>
  <text x='47' y='72' text-anchor='middle' font-family='Arial' font-size='16' font-weight='bold' fill='#000'>" + name + @"</text>
</svg>";

    // ── SVG robot compacto (para 80R3 y 110R1) ──
    static string RobotSmallSvg(string name) => @"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'>
  <rect x='25' y='82' width='40' height='16' rx='3' fill='#D48C28' stroke='#333' stroke-width='1.5'/>
  <rect x='30' y='45' width='30' height='37' rx='4' fill='#D48C28' stroke='#333' stroke-width='1.5'/>
  <circle cx='45' cy='42' r='9' fill='#D48C28' stroke='#333' stroke-width='1.5'/>
  <rect x='45' y='36' width='30' height='11' rx='3' fill='#D48C28' stroke='#333' stroke-width='1.5'/>
  <circle cx='75' cy='41' r='6' fill='#D48C28' stroke='#333' stroke-width='1.5'/>
  <rect x='71' y='41' width='10' height='28' rx='3' fill='#D48C28' stroke='#333' stroke-width='1.5'/>
  <line x1='76' y1='69' x2='80' y2='78' stroke='#FFD700' stroke-width='2'/>
  <rect x='35' y='25' width='16' height='14' rx='2' fill='#FFD700' stroke='#333' stroke-width='1'/>
  <rect x='15' y='35' width='70' height='40' rx='5' fill='#CC0000' fill-opacity='0.25' stroke='#CC0000' stroke-width='2'/>
  <text x='50' y='62' text-anchor='middle' font-family='Arial' font-size='15' font-weight='bold' fill='#000'>" + name + @"</text>
</svg>";

    // ════════════════════════════════════════════════════════════════
    //  ROBOTS — Main
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreateRobots()
    {
        var r1 = Project.Current.Get(PATH_MAIN);
        if (r1 == null) { Log.Error("BuildRobotsLayout", "No encontré " + PATH_MAIN); return; }
        BuildRobots(r1);
        Log.Info("BuildRobotsLayout", "Robots creados en Main.");
    }

    [ExportMethod]
    public void CreateRobotsMonitoring()
    {
        var r1 = Project.Current.Get(PATH_MON);
        if (r1 == null) { Log.Error("BuildRobotsLayout", "No encontré " + PATH_MON); return; }
        BuildRobots(r1);
        Log.Info("BuildRobotsLayout", "Robots creados en MainMonitoring.");
    }

    private void BuildRobots(IUANode parent)
    {
        // Limpiar anteriores
        string[] names = { "CellBg", "WallH", "WallV1", "WallV2",
            "Svg_80R1", "Svg_80R2", "Svg_80R3", "Svg_90R1",
            "Svg_100R1", "Svg_100R2", "Svg_110R1",
            "TD_80R1", "TD_80R2", "TD_90R1", "TD_100R1", "TD_100R2" };
        foreach (var n in names) { var e = parent.Get(n); if (e != null) parent.Remove(e); }

        // Fondo
        var bg = InformationModel.MakeObject<Rectangle>("CellBg");
        bg.Width = 500; bg.Height = 340; bg.TopMargin = 0; bg.LeftMargin = 0;
        SP(bg, "FillColor", COL_CELL); SP(bg, "BorderColor", COL_BLACK); SP(bg, "BorderThickness", new UAValue(1));
        parent.Add(bg);

        // Paredes de celda
        AddWall(parent, "WallH",  0, 165, 500, 2);
        AddWall(parent, "WallV1", 245, 0, 2, 340);
        AddWall(parent, "WallV2", 155, 165, 2, 175);

        // ── ROBOTS como AdvancedSVGImage ──
        // 80R1 — frontal, arriba izquierda
        AddSvgRobot(parent, "Svg_80R1", RobotFrontSvg("80R1"), 10, 5, 85, 100);
        AddTd(parent, "TD_80R1", "80R1 TD", 15, 105);

        // 80R2 — frontal, arriba izquierda-centro
        AddSvgRobot(parent, "Svg_80R2", RobotFrontSvg("80R2"), 100, 5, 85, 100);
        AddTd(parent, "TD_80R2", "80R2 TD", 105, 105);

        // 90R1 — lateral, centro
        AddSvgRobot(parent, "Svg_90R1", RobotSideSvg("90R1"), 165, 175, 80, 70);
        AddTd(parent, "TD_90R1", "90R1 TD", 185, 155);

        // 100R2 — frontal, arriba derecha
        AddSvgRobot(parent, "Svg_100R2", RobotFrontSvg("100R2"), 280, 5, 85, 100);
        AddTd(parent, "TD_100R2", "100R2 TD", 285, 105);

        // 100R1 — lateral, centro-derecha abajo
        AddSvgRobot(parent, "Svg_100R1", RobotSideSvg("100R1"), 260, 185, 80, 70);
        AddTd(parent, "TD_100R1", "100R1 TD", 265, 258);

        // 80R3 — compacto, abajo izquierda
        AddSvgRobot(parent, "Svg_80R3", RobotSmallSvg("80R3"), 35, 205, 65, 65);

        // 110R1 — compacto, derecha
        AddSvgRobot(parent, "Svg_110R1", RobotSmallSvg("110R1"), 395, 195, 70, 70);
    }

    // ════════════════════════════════════════════════════════════════
    //  ESTACIONES
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreateStations()
    {
        var r1 = Project.Current.Get(PATH_MAIN);
        if (r1 == null) { Log.Error("BuildRobotsLayout", "No encontré " + PATH_MAIN); return; }
        BuildStations(r1);
        Log.Info("BuildRobotsLayout", "Estaciones creadas en Main.");
    }

    [ExportMethod]
    public void CreateStationsMonitoring()
    {
        var r1 = Project.Current.Get(PATH_MON);
        if (r1 == null) { Log.Error("BuildRobotsLayout", "No encontré " + PATH_MON); return; }
        BuildStations(r1);
        Log.Info("BuildRobotsLayout", "Estaciones creadas en MainMonitoring.");
    }

    private void BuildStations(IUANode parent)
    {
        string[] names = { "Sta_B80_10", "Lbl_B80_10", "Sta_B80_20", "Lbl_B80_20",
            "Sta_B100_10", "Lbl_B100_10", "Sta_B110_10", "Lbl_B110_10",
            "Sta_B110_20", "Lbl_B110_20",
            "Msg_80", "Msg_100", "Msg_110a", "Msg_110b" };
        foreach (var n in names) { var e = parent.Get(n); if (e != null) parent.Remove(e); }

        AddStation(parent, "Sta_B80_10",  "Lbl_B80_10",  "B80 STA 10",  25, 120, 115, 22);
        AddStation(parent, "Sta_B80_20",  "Lbl_B80_20",  "B80 STA 20",  110, 225, 50, 18);
        AddStation(parent, "Sta_B100_10", "Lbl_B100_10", "B100 STA 10", 270, 120, 115, 22);
        AddStation(parent, "Sta_B110_20", "Lbl_B110_20", "B110 STA 20", 345, 290, 68, 20);
        AddStation(parent, "Sta_B110_10", "Lbl_B110_10", "B110 STA 10", 418, 290, 68, 20);

        AddMsg(parent, "Msg_80",   "Local Message", 20,  145, 125, 20);
        AddMsg(parent, "Msg_100",  "Local Message", 265, 145, 125, 20);
        AddMsg(parent, "Msg_110a", "Local Message", 345, 312, 68,  16);
        AddMsg(parent, "Msg_110b", "Local Message", 418, 312, 68,  16);
    }

    // ════════════════════════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════════════════════════

    private void AddSvgRobot(IUANode parent, string name, string svgContent,
        int x, int y, int w, int h)
    {
        var svg = InformationModel.MakeObject<AdvancedSVGImage>(name);
        svg.Width = w; svg.Height = h;
        svg.TopMargin = y; svg.LeftMargin = x;
        SP(svg, "HitTestVisible", new UAValue(true));
        parent.Add(svg);

        // Inyectar el SVG inline
        try
        {
            var method = svg.GetType().GetMethod("SetImageContent");
            if (method != null)
            {
                method.Invoke(svg, new object[] { svgContent });
            }
            else
            {
                // Fallback: intentar vía OPC UA method
                var setContent = svg.Get("SetImageContent");
                if (setContent != null)
                    Log.Info("BuildRobotsLayout", name + ": SetImageContent encontrado, invocar manualmente.");
                else
                    Log.Warning("BuildRobotsLayout", name + ": SetImageContent no disponible en design-time. Importa el SVG como archivo.");
            }
        }
        catch (Exception ex)
        {
            Log.Warning("BuildRobotsLayout", name + " SVG: " + ex.Message);
        }
    }

    private void AddWall(IUANode parent, string name, int x, int y, int w, int h)
    {
        var r = InformationModel.MakeObject<Rectangle>(name);
        r.Width = w; r.Height = h; r.TopMargin = y; r.LeftMargin = x;
        SP(r, "FillColor", COL_GRAY); SP(r, "BorderThickness", new UAValue(0));
        parent.Add(r);
    }

    private void AddTd(IUANode parent, string name, string text, int x, int y)
    {
        var lbl = InformationModel.MakeObject<Label>(name);
        lbl.Width = 60; lbl.Height = 18; lbl.TopMargin = y; lbl.LeftMargin = x;
        lbl.Text = text;
        SP(lbl, "TextColor", COL_BLACK); SP(lbl, "FontSize", new UAValue(8));
        parent.Add(lbl);
    }

    private void AddStation(IUANode parent, string rName, string lName,
        string text, int x, int y, int w, int h)
    {
        var r = InformationModel.MakeObject<Rectangle>(rName);
        r.Width = w; r.Height = h; r.TopMargin = y; r.LeftMargin = x;
        SP(r, "FillColor", COL_TEAL); SP(r, "BorderThickness", new UAValue(1));
        SP(r, "BorderColor", COL_TEAL);
        parent.Add(r);

        var lbl = InformationModel.MakeObject<Label>(lName);
        lbl.Width = w; lbl.Height = h; lbl.TopMargin = y; lbl.LeftMargin = x;
        lbl.Text = text;
        SP(lbl, "TextColor", COL_WHITE); SP(lbl, "FontSize", new UAValue(9));
        SP(lbl, "FontWeight", new UAValue(700));
        SP(lbl, "TextHorizontalAlignment", new UAValue(1));
        SP(lbl, "TextVerticalAlignment", new UAValue(1));
        parent.Add(lbl);
    }

    private void AddMsg(IUANode parent, string name, string text, int x, int y, int w, int h)
    {
        var lbl = InformationModel.MakeObject<Label>(name);
        lbl.Width = w; lbl.Height = h; lbl.TopMargin = y; lbl.LeftMargin = x;
        lbl.Text = text;
        SP(lbl, "BackgroundColor", COL_WHITE); SP(lbl, "TextColor", COL_BLACK);
        SP(lbl, "FontSize", new UAValue(8));
        SP(lbl, "TextHorizontalAlignment", new UAValue(1));
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