﻿namespace AcadLib.Layers
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;

    /// <summary>
    /// Состояние слоев - для проверки видимости объектов на чертеже
    /// </summary>
    public class LayerVisibleState
    {
        private Dictionary<string, bool> layerVisibleDict;

        /// <summary>
        /// Нужно создавать новый объект LayerVisibleState после возмоного изменения состояния слоев пользователем.
        /// </summary>
        /// <param name="db"></param>
        public LayerVisibleState(Database db)
        {
            layerVisibleDict = GetLayerVisibleState(db);
        }

        /// <summary>
        /// Объект на видим - не скрыт, не на выключенном или замороженном слое
        /// </summary>
        /// <param name="ent"></param>
        public bool IsVisible(Entity ent)
        {
            bool res;
            if (!ent.Visible)
            {
                res = false;
            }
            else
            {
                // Слой выключен или заморожен
                layerVisibleDict.TryGetValue(ent.Layer, out res);
            }

            return res;
        }

        private Dictionary<string, bool> GetLayerVisibleState(Database db)
        {
            var res = new Dictionary<string, bool>();
            var lt = (LayerTable)db.LayerTableId.GetObject(OpenMode.ForRead);
            foreach (var idLayer in lt)
            {
                var layer = (LayerTableRecord)idLayer.GetObject(OpenMode.ForRead);
                res.Add(layer.Name, !layer.IsOff && !layer.IsFrozen);
            }

            return res;
        }
    }
}