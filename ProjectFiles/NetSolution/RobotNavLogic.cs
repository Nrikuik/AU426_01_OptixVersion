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
 * RobotNavLogic — Runtime NetLogic
 * Controla la navegación UP/DOWN en la lista de robots.
 *
 * CÓMO CONECTAR:
 *   1. Crea este NetLogic como hijo del panel RobotPanel (o de Screen1).
 *   2. En BtnUp → Events → MouseClick → Asigna el método MoveUp de este NetLogic.
 *   3. En BtnDown → Events → MouseClick → Asigna el método MoveDown de este NetLogic.
 *   4. (Opcional) En ToggleBtn → Events → Modified value → Asigna ConfirmSelection.
 *
 * FUNCIONAMIENTO:
 *   - MoveUp/MoveDown cambian SelectedIndex y actualizan SelectedText.
 *   - ConfirmSelection copia SelectedText a CurrentRobot.
 *   - El ListBox se sincroniza automáticamente vía SelectedItem.
 */
public class RobotNavLogic : BaseNetLogic
{
    public override void Start()
    {
        // Nada especial al iniciar
    }

    public override void Stop()
    {
        // Nada especial al detener
    }

    [ExportMethod]
    public void MoveUp()
    {
        try
        {
            var indexVar = Project.Current.GetVariable("Model/RobotMenu/SelectedIndex");
            var textVar  = Project.Current.GetVariable("Model/RobotMenu/SelectedText");
            var items    = Project.Current.Get("Model/RobotMenu/Items");
            if (indexVar == null || textVar == null || items == null) return;

            var children = items.Children.OfType<IUAVariable>().ToList();
            if (children.Count == 0) return;

            int idx = indexVar.Value;
            idx--;
            if (idx < 0) idx = children.Count - 1;  // Wrap around al final

            indexVar.Value = new UAValue(idx);
            textVar.Value  = children[idx].Value;

            // Actualizar selección visual del ListBox
            UpdateListBoxSelection(children[idx]);
        }
        catch (Exception ex)
        {
            Log.Error("RobotNavLogic", "MoveUp error: " + ex.Message);
        }
    }

    [ExportMethod]
    public void MoveDown()
    {
        try
        {
            var indexVar = Project.Current.GetVariable("Model/RobotMenu/SelectedIndex");
            var textVar  = Project.Current.GetVariable("Model/RobotMenu/SelectedText");
            var items    = Project.Current.Get("Model/RobotMenu/Items");
            if (indexVar == null || textVar == null || items == null) return;

            var children = items.Children.OfType<IUAVariable>().ToList();
            if (children.Count == 0) return;

            int idx = indexVar.Value;
            idx++;
            if (idx >= children.Count) idx = 0;  // Wrap around al inicio

            indexVar.Value = new UAValue(idx);
            textVar.Value  = children[idx].Value;

            // Actualizar selección visual del ListBox
            UpdateListBoxSelection(children[idx]);
        }
        catch (Exception ex)
        {
            Log.Error("RobotNavLogic", "MoveDown error: " + ex.Message);
        }
    }

    [ExportMethod]
    public void ConfirmSelection()
    {
        try
        {
            var textVar    = Project.Current.GetVariable("Model/RobotMenu/SelectedText");
            var currentVar = Project.Current.GetVariable("Model/RobotMenu/CurrentRobot");
            if (textVar == null || currentVar == null) return;

            currentVar.Value = textVar.Value;
            Log.Info("RobotNavLogic", "Robot seleccionado: " + currentVar.Value);
        }
        catch (Exception ex)
        {
            Log.Error("RobotNavLogic", "ConfirmSelection error: " + ex.Message);
        }
    }

    /// <summary>
    /// Intenta actualizar la selección visual del ListBox.
    /// Busca el ListBox1 dentro del Owner (panel padre).
    /// </summary>
    private void UpdateListBoxSelection(IUANode selectedNode)
    {
        try
        {
            // Buscar ListBox en el Owner o en la pantalla
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
            Log.Warning("RobotNavLogic", "UpdateListBoxSelection: " + ex.Message);
        }
    }
}
