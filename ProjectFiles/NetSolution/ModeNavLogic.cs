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
 * ModeNavLogic — Runtime NetLogic
 * Controla navegación UP/DOWN en PANTALLA DE SELECCION DE MODO
 *
 * COLOCAR: Como hijo directo de ModeSelectPanel
 *
 * CONECTAR:
 *   BtnUp    → MouseClick → MoveUp
 *   BtnDown  → MouseClick → MoveDown
 *   BtnConfirm → MouseClick → ConfirmSelection
 */
public class ModeNavLogic : BaseNetLogic
{
    public override void Start() { }
    public override void Stop() { }

    [ExportMethod]
    public void MoveUp()
    {
        try
        {
            var indexVar = Project.Current.GetVariable("Model/ModeMenu/SelectedIndex");
            var textVar  = Project.Current.GetVariable("Model/ModeMenu/SelectedText");
            var items    = Project.Current.Get("Model/ModeMenu/Items");
            if (indexVar == null || textVar == null || items == null) return;

            var children = items.Children.OfType<IUAVariable>().ToList();
            if (children.Count == 0) return;

            int idx = indexVar.Value;
            idx--;
            if (idx < 0) idx = children.Count - 1;

            indexVar.Value = new UAValue(idx);
            textVar.Value  = children[idx].Value;

            UpdateListBoxSelection(children[idx]);
        }
        catch (Exception ex)
        {
            Log.Error("ModeNavLogic", "MoveUp: " + ex.Message);
        }
    }

    [ExportMethod]
    public void MoveDown()
    {
        try
        {
            var indexVar = Project.Current.GetVariable("Model/ModeMenu/SelectedIndex");
            var textVar  = Project.Current.GetVariable("Model/ModeMenu/SelectedText");
            var items    = Project.Current.Get("Model/ModeMenu/Items");
            if (indexVar == null || textVar == null || items == null) return;

            var children = items.Children.OfType<IUAVariable>().ToList();
            if (children.Count == 0) return;

            int idx = indexVar.Value;
            idx++;
            if (idx >= children.Count) idx = 0;

            indexVar.Value = new UAValue(idx);
            textVar.Value  = children[idx].Value;

            UpdateListBoxSelection(children[idx]);
        }
        catch (Exception ex)
        {
            Log.Error("ModeNavLogic", "MoveDown: " + ex.Message);
        }
    }

    [ExportMethod]
    public void ConfirmSelection()
    {
        try
        {
            var textVar    = Project.Current.GetVariable("Model/ModeMenu/SelectedText");
            var currentVar = Project.Current.GetVariable("Model/ModeMenu/CurrentMode");
            if (textVar == null || currentVar == null) return;

            currentVar.Value = textVar.Value;
            Log.Info("ModeNavLogic", "Modo seleccionado: " + currentVar.Value);
        }
        catch (Exception ex)
        {
            Log.Error("ModeNavLogic", "ConfirmSelection: " + ex.Message);
        }
    }

    private void UpdateListBoxSelection(IUANode selectedNode)
    {
        try
        {
            var listBox = Owner.Get("ListBox1");
            if (listBox != null)
            {
                var selectedItemVar = listBox.GetVariable("SelectedItem");
                if (selectedItemVar != null)
                    selectedItemVar.Value = selectedNode.NodeId;
            }
        }
        catch (Exception ex)
        {
            Log.Warning("ModeNavLogic", "UpdateListBoxSelection: " + ex.Message);
        }
    }
}