﻿namespace AcadLib
{
    using Autodesk.AutoCAD.ApplicationServices.Core;
    using Autodesk.AutoCAD.Colors;
    using Autodesk.AutoCAD.DatabaseServices;

    public static class Draw
    {
        public static void Polyline(
            Layers.LayerInfo? layer = null,
            Color? color = null,
            LineWeight? lineWeight = null,
            string? lineType = null,
            double? lineTypeScale = null)
        {
            // Обертка запуска команды рисования полилинии с заданными свойствами.
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;

            // Вызов команды рисования полилинии
            using (new DrawParameters(db, layer, color, lineWeight, lineType, lineTypeScale))
            {
                doc.Editor.Command("_PLINE");
            }
        }
    }
}