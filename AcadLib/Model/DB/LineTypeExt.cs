﻿namespace AcadLib
{
    using System.IO;
    using Autodesk.AutoCAD.DatabaseServices;

    public static class LineTypeExt
    {
        public static ObjectId GetLineTypeId(this Database db, string lineTypeName)
        {
#pragma warning disable 618
            using var lt = (LinetypeTable)db.LinetypeTableId.Open(OpenMode.ForRead);
#pragma warning restore 618
            return lt.Has(lineTypeName) ? lt[lineTypeName] : ObjectId.Null;
        }

        /// <summary>
        /// Загрузка типа линии из файла поддержи lin в папке Support PIK
        /// Если файл не найден или тип линии, то вернется текущий тип линии чертежа
        /// </summary>
        /// <param name="db"></param>
        /// <param name="lineTypeName">тип линии</param>
        /// <param name="fileName">Имя файла</param>
        public static ObjectId LoadLineTypePIK(
            this Database db,
            string lineTypeName,
            string fileName = "GOST 2.303-68.lin")
        {
            var id = db.GetLineTypeId(lineTypeName);
            if (!id.IsNull)
                return id;

            var file = Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.LocalSettingsFolder,
                "Support\\" + fileName);
            if (File.Exists(file))
            {
                try
                {
                    db.LoadLineTypeFile(lineTypeName, file);
                    return db.GetLineTypeId(lineTypeName);
                }
                catch
                {
                    Logger.Log.Error($"Ошибка загрузки типа линии - LoadLineTypePIK '{lineTypeName}'");
                }
            }
            else
            {
                Logger.Log.Error($"Не найден файл типов линий '{file}'");
            }

            return db.Celtype;
        }

        /// <summary>
        /// Загрузка штриховой линии из стандартного файла типов линий GOST 2.303-68.lin
        /// </summary>
        public static ObjectId LoadLineTypeDotPIK(this Database db)
        {
            return LoadLineTypePIK(db, "Штриховая");
        }

        /// <summary>
        /// Загрузка 'Штрих-пунктирная тонкая' линии из стандартного файла типов линий GOST 2.303-68.lin
        /// </summary>
        public static ObjectId LoadLineTypeDashDotedThinPIK(this Database db)
        {
            return LoadLineTypePIK(db, "Штрих-пунктирная тонкая");
        }
    }
}
