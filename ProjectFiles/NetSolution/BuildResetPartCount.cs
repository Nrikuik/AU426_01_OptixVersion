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
 * BuildResetPartCount — Design-time NetLogic
 * Panel contextual "PRESS TO RESET PART COUNT"
 * Fondo verde oscuro, botón RESET circular verde brillante, botón navegación azul
 *
 * USO: CreateUI()
 */
public class BuildResetPartCount : BaseNetLogic
{
    static readonly Color COL_GREEN_BG  = new Color(0xFF, 0x0E, 0x4E, 0x0E);
    static readonly Color COL_GREEN_BTN = new Color(0xFF, 0x00, 0xFF, 0x00);
    static readonly Color COL_BLUE      = new Color(0xFF, 0x00, 0x00, 0xCC);
    static readonly Color COL_WHITE     = new Color(0xFF, 0xFF, 0xFF, 0xFF);
    static readonly Color COL_BLACK     = new Color(0xFF, 0x00, 0x00, 0x00);

    [ExportMethod]
    public void CreateUI()
    {
        var uiRoot = Project.Current.Get("UI");
        if (uiRoot == null) { Log.Error("BuildResetPartCount", "No se encontró UI."); return; }

        var existing = uiRoot.Get("ResetPartCountPanel");
        if (existing != null) uiRoot.Remove(existing);

        // ── Panel principal ──
        var panel = InformationModel.MakeObject<Panel>("ResetPartCountPanel");
        panel.Width = 480;
        panel.Height = 400;
        panel.HorizontalAlignment = HorizontalAlignment.Center;
        panel.VerticalAlignment = VerticalAlignment.Center;

        // ── Fondo verde oscuro ──
        var bg = InformationModel.MakeObject<Rectangle>("Background");
        bg.HorizontalAlignment = HorizontalAlignment.Stretch;
        bg.VerticalAlignment = VerticalAlignment.Stretch;
        bg.LeftMargin = 0; bg.TopMargin = 0; bg.RightMargin = 0; bg.BottomMargin = 0;
        SP(bg, "FillColor", COL_GREEN_BG);
        SP(bg, "BorderThickness", new UAValue(0));
        panel.Add(bg);

        // ── Título "PRESS TO RESET PART COUNT" ──
        var lblTitle = InformationModel.MakeObject<Label>("LblTitle");
        lblTitle.Width = 440; lblTitle.Height = 40;
        lblTitle.TopMargin = 25; lblTitle.LeftMargin = 20;
        lblTitle.Text = "PRESS TO RESET PART COUNT";
        SP(lblTitle, "TextColor", COL_WHITE);
        SP(lblTitle, "FontSize", new UAValue(22));
        SP(lblTitle, "FontWeight", new UAValue(700));
        SP(lblTitle, "TextHorizontalAlignment", new UAValue(1));
        panel.Add(lblTitle);

        // ── Botón RESET — circular, verde brillante con borde negro ──
        var btnReset = InformationModel.MakeObject<Button>("BtnReset");
        btnReset.Width = 140;
        btnReset.Height = 140;
        btnReset.TopMargin = 120;
        btnReset.LeftMargin = 150;
        btnReset.Text = "RESET";
        SP(btnReset, "BackgroundColor", COL_GREEN_BTN);
        SP(btnReset, "TextColor", COL_BLACK);
        SP(btnReset, "FontSize", new UAValue(24));
        SP(btnReset, "FontWeight", new UAValue(700));
        SP(btnReset, "Appearance", new UAValue("Bordered Circular"));
        panel.Add(btnReset);

        // ── Botón navegación — circular azul, abajo derecha ──
        var btnNav = InformationModel.MakeObject<Button>("BtnNavCircle");
        btnNav.Width = 60;
        btnNav.Height = 60;
        btnNav.TopMargin = 310;
        btnNav.LeftMargin = 360;
        btnNav.Text = "▲";
        SP(btnNav, "BackgroundColor", COL_BLUE);
        SP(btnNav, "TextColor", COL_WHITE);
        SP(btnNav, "FontSize", new UAValue(20));
        SP(btnNav, "Appearance", new UAValue("Bordered Circular"));
        panel.Add(btnNav);

        uiRoot.Add(panel);
        Log.Info("BuildResetPartCount", "Panel ResetPartCountPanel creado.");
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