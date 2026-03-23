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
 * BuildRobotPanel — Design-time NetLogic v3
 * Migración de FT View "dRBT SELECT" a FactoryTalk Optix
 *
 * CORRECCIONES v3:
 *   1. Fondo verde: Rectangle como fondo (Panel no tiene BackgroundColor directa)
 *   2. ListBox: DisplayValuePath = "Value" para mostrar texto con espacios (no BrowseName)
 *   3. Botones UP/DOWN: Código de Runtime NetLogic para navegación
 *   4. Toggle: Appearance "Bordered Circular" para botón redondo
 *   5. Helper SetProperty unificado que materializa propiedades opcionales
 *   6. Labels "CURRENT ROBOT" visibles sobre fondo verde
 */
public class BuildRobotPanel : BaseNetLogic
{
    // ════════════════════════════════════════════════════════════════
    //  PASO 1: Crear el modelo de datos
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreateModel()
    {
        var model = Project.Current.Get("Model");
        if (model == null)
        {
            Log.Error("BuildRobotPanel", "No se encontró el nodo Model en el proyecto.");
            return;
        }

        var robotMenu = model.Get("RobotMenu");
        if (robotMenu == null)
            robotMenu = CreateFolder(model, "RobotMenu");

        AddVariableIfMissing(robotMenu, "SelectedIndex", OpcUa.DataTypes.Int32,  new UAValue(0));
        AddVariableIfMissing(robotMenu, "SelectedText",  OpcUa.DataTypes.String, new UAValue(""));
        AddVariableIfMissing(robotMenu, "CurrentRobot",  OpcUa.DataTypes.String, new UAValue(""));

        var items = robotMenu.Get("Items");
        if (items == null)
            items = CreateFolder(robotMenu, "Items");

        foreach (var child in items.Children.Cast<IUANode>().ToList())
            items.Remove(child);

        string[] menuItems = {
            "ROBOT HOME",
            "ROBOT REPAIR",
            "ROBOT TIP DRESS",
            "ROBOT CAP CHANGE",
            "ROBOT CAPS CHANGED",
            "ROBOT MASTERING SWITCH",
            "ROBOT BRAKE TEST",
            "START SEALER (IF PRESENT)",
            "ROBOT TO PREHEATING",
            "ROBOT PREHEATED"
        };

        foreach (var item in menuItems)
        {
            var nodeName = item.Replace(" ", "_")
                              .Replace("(", "")
                              .Replace(")", "");
            var node = InformationModel.MakeVariable(nodeName, OpcUa.DataTypes.String);
            node.Value = new UAValue(item);
            items.Add(node);
        }

        Log.Info("BuildRobotPanel", "Modelo creado. Items: " + menuItems.Length);
    }

    // ════════════════════════════════════════════════════════════════
    //  PASO 2: Crear la interfaz de usuario
    // ════════════════════════════════════════════════════════════════
    [ExportMethod]
    public void CreateUI()
    {
        // ── Destino: directamente bajo UI (como tipo reutilizable) ──
        var uiRoot = Project.Current.Get("UI");
        if (uiRoot == null)
        {
            Log.Error("BuildRobotPanel", "No se encontró el nodo UI en el proyecto.");
            return;
        }

        // Limpiar panel anterior si existe
        var existing = uiRoot.Get("RobotPanel");
        if (existing != null)
            uiRoot.Remove(existing);

        var selectedTextVar = Project.Current.GetVariable("Model/RobotMenu/SelectedText");
        var currentRobotVar = Project.Current.GetVariable("Model/RobotMenu/CurrentRobot");
        var itemsNode       = Project.Current.Get("Model/RobotMenu/Items");

        if (selectedTextVar == null || currentRobotVar == null || itemsNode == null)
        {
            Log.Error("BuildRobotPanel", "Ejecuta CreateModel() primero.");
            return;
        }

        // ── Crear objetos ──
        var panel      = InformationModel.MakeObject<Panel>("RobotPanel");
        var bgRect     = InformationModel.MakeObject<Rectangle>("Background");
        var btnUp      = InformationModel.MakeObject<Button>("BtnUp");
        var btnDown    = InformationModel.MakeObject<Button>("BtnDown");
        var listBox    = InformationModel.MakeObject<ListBox>("ListBox1");
        var lblTitle   = InformationModel.MakeObject<Label>("LblTitle");
        var lblCurrent = InformationModel.MakeObject<Label>("LblCurrentValue");
        var lblMsg     = InformationModel.MakeObject<Button>("LblLocalMessage");
        var toggleBtn  = InformationModel.MakeObject<ToggleButton>("ToggleBtn");

        // ── Árbol ──
        panel.Add(bgRect);
        panel.Add(btnUp);
        panel.Add(btnDown);
        panel.Add(listBox);
        panel.Add(lblTitle);
        panel.Add(lblCurrent);
        panel.Add(lblMsg);
        panel.Add(toggleBtn);
        uiRoot.Add(panel);  // Se agrega bajo UI, al nivel de Panel1 (type)

        // ══════════════════════════════════════════════════════════
        //  PANEL contenedor
        // ══════════════════════════════════════════════════════════
        panel.Width               = 560;
        panel.Height              = 650;
        panel.HorizontalAlignment = HorizontalAlignment.Center;
        panel.VerticalAlignment   = VerticalAlignment.Center;

        // ══════════════════════════════════════════════════════════
        //  FONDO VERDE — Rectangle que se estira a todo el panel
        // ══════════════════════════════════════════════════════════
        bgRect.HorizontalAlignment = HorizontalAlignment.Stretch;
        bgRect.VerticalAlignment   = VerticalAlignment.Stretch;
        bgRect.LeftMargin   = 0;
        bgRect.TopMargin    = 0;
        bgRect.RightMargin  = 0;
        bgRect.BottomMargin = 0;
        SetProperty(bgRect, "FillColor",        new Color(0xFF, 0x0E, 0x4E, 0x0E));
        SetProperty(bgRect, "BorderColor",      new Color(0xFF, 0x0E, 0x4E, 0x0E));
        SetProperty(bgRect, "BorderThickness",  new UAValue(0));

        // ══════════════════════════════════════════════════════════
        //  BOTÓN UP — azul, flecha arriba
        // ══════════════════════════════════════════════════════════
        btnUp.Width      = 90;
        btnUp.Height     = 90;
        btnUp.TopMargin  = 40;
        btnUp.LeftMargin = 20;
        btnUp.Text       = "▲";
        SetProperty(btnUp, "BackgroundColor", new Color(0xFF, 0x00, 0x99, 0xCC));
        SetProperty(btnUp, "TextColor",       new Color(0xFF, 0xFF, 0xFF, 0xFF));
        SetProperty(btnUp, "FontSize",        new UAValue(36));

        // ══════════════════════════════════════════════════════════
        //  BOTÓN DOWN — azul, flecha abajo
        // ══════════════════════════════════════════════════════════
        btnDown.Width      = 90;
        btnDown.Height     = 90;
        btnDown.TopMargin  = 160;
        btnDown.LeftMargin = 20;
        btnDown.Text       = "▼";
        SetProperty(btnDown, "BackgroundColor", new Color(0xFF, 0x00, 0x99, 0xCC));
        SetProperty(btnDown, "TextColor",       new Color(0xFF, 0xFF, 0xFF, 0xFF));
        SetProperty(btnDown, "FontSize",        new UAValue(36));

        // ══════════════════════════════════════════════════════════
        //  LISTBOX
        //  CLAVE: DisplayValuePath = "Value" → muestra el valor
        //         de cada variable (ROBOT HOME con espacios)
        //         Si está vacío, Optix muestra el BrowseName
        //         (ROBOT_HOME con guiones bajos)
        // ══════════════════════════════════════════════════════════
        listBox.Width      = 400;
        listBox.Height     = 300;
        listBox.TopMargin  = 20;
        listBox.LeftMargin = 130;
        listBox.Model      = itemsNode.NodeId;
        listBox.DisplayValuePath = new LocalizedText("Value");

        try
        {
            listBox.SelectedValueVariable.SetDynamicLink(selectedTextVar, DynamicLinkMode.ReadWrite);
        }
        catch (Exception ex)
        {
            Log.Warning("BuildRobotPanel", "SelectedValue link: " + ex.Message);
        }

        // ══════════════════════════════════════════════════════════
        //  LABEL "CURRENT ROBOT" — texto blanco
        // ══════════════════════════════════════════════════════════
        lblTitle.Width      = 400;
        lblTitle.Height     = 40;
        lblTitle.TopMargin  = 340;
        lblTitle.LeftMargin = 130;
        lblTitle.Text       = "CURRENT ROBOT";
        SetProperty(lblTitle, "TextColor", new Color(0xFF, 0xFF, 0xFF, 0xFF));
        SetProperty(lblTitle, "FontSize",  new UAValue(18));
        SetProperty(lblTitle, "TextHorizontalAlignment", new UAValue(1)); // Center

        // ══════════════════════════════════════════════════════════
        //  LABEL valor actual — vinculado a CurrentRobot
        // ══════════════════════════════════════════════════════════
        lblCurrent.Width      = 400;
        lblCurrent.Height     = 50;
        lblCurrent.TopMargin  = 380;
        lblCurrent.LeftMargin = 130;
        lblCurrent.Text       = "";
        SetProperty(lblCurrent, "TextColor", new Color(0xFF, 0xFF, 0xFF, 0xFF));
        SetProperty(lblCurrent, "FontSize",  new UAValue(16));
        SetProperty(lblCurrent, "TextHorizontalAlignment", new UAValue(1));

        try
        {
            lblCurrent.TextVariable.SetDynamicLink(currentRobotVar, DynamicLinkMode.Read);
        }
        catch (Exception ex)
        {
            Log.Warning("BuildRobotPanel", "LblCurrentValue link: " + ex.Message);
        }

        // ══════════════════════════════════════════════════════════
        //  BOTÓN "Local Message Display" — azul
        // ══════════════════════════════════════════════════════════
        lblMsg.Width      = 400;
        lblMsg.Height     = 50;
        lblMsg.TopMargin  = 430;
        lblMsg.LeftMargin = 130;
        lblMsg.Text       = "Local Message Display";
        SetProperty(lblMsg, "BackgroundColor", new Color(0xFF, 0x00, 0x99, 0xCC));
        SetProperty(lblMsg, "TextColor",       new Color(0xFF, 0xFF, 0xFF, 0xFF));
        SetProperty(lblMsg, "FontSize",        new UAValue(14));

        // ══════════════════════════════════════════════════════════
        //  TOGGLE — circular, blanco, texto negro
        //  Nota: Si "Bordered Circular" no se aplica vía SetProperty,
        //  se puede cambiar manualmente en Properties después.
        // ══════════════════════════════════════════════════════════
        toggleBtn.Width      = 100;
        toggleBtn.Height     = 100;
        toggleBtn.TopMargin  = 500;
        toggleBtn.LeftMargin = 230;
        toggleBtn.Text       = "TOGGLE";
        SetProperty(toggleBtn, "BackgroundColor", new Color(0xFF, 0xFF, 0xFF, 0xFF));
        SetProperty(toggleBtn, "TextColor",       new Color(0xFF, 0x00, 0x00, 0x00));
        SetProperty(toggleBtn, "FontSize",        new UAValue(14));

        // Intentar Appearance circular — si falla, el usuario lo cambia manualmente
        try
        {
            var appVar = ((IUANode)toggleBtn).GetVariable("Appearance");
            if (appVar != null)
                appVar.Value = new UAValue("Bordered Circular");
            else
                Log.Info("BuildRobotPanel", "Toggle: Cambia Appearance a 'Bordered Circular' manualmente en Properties.");
        }
        catch { }

        Log.Info("BuildRobotPanel", "UI creada correctamente. Recuerda crear el Runtime NetLogic para UP/DOWN.");
    }

    // ════════════════════════════════════════════════════════════════
    //  HELPER UNIVERSAL
    //  Maneja propiedades que pueden o no estar materializadas
    // ════════════════════════════════════════════════════════════════
    private void SetProperty(IUANode node, string propertyName, object value)
    {
        try
        {
            var variable = node.GetVariable(propertyName);
            if (variable != null)
            {
                if (value is Color c)
                    variable.Value = c;
                else if (value is UAValue u)
                    variable.Value = u;
                else
                    variable.Value = new UAValue(value.ToString());
            }
            else
            {
                IUAVariable newVar = InformationModel.MakeVariable(
                    propertyName, OpcUa.DataTypes.BaseDataType);

                if (value is Color c2)
                    newVar.Value = c2;
                else if (value is UAValue u2)
                    newVar.Value = u2;
                else
                    newVar.Value = new UAValue(value.ToString());

                node.Add(newVar);
            }
        }
        catch (Exception ex)
        {
            string nodeName = "?";
            try { nodeName = (node as IUAObject)?.BrowseName ?? node.ToString(); } catch { }
            Log.Warning("BuildRobotPanel", nodeName + "." + propertyName + ": " + ex.Message);
        }
    }

    private IUANode CreateFolder(IUANode parent, string name)
    {
        var folder = InformationModel.Make<Folder>(name);
        parent.Add(folder);
        return folder;
    }

    private void AddVariableIfMissing(IUANode parent, string name, NodeId dataType, UAValue value)
    {
        if (parent.Get(name) == null)
        {
            var v = InformationModel.MakeVariable(name, dataType);
            v.Value = value;
            parent.Add(v);
        }
    }
}
