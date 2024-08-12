namespace AcadLib.Template
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AcadLib.Layers.Filter;
    using AcadLib.Layers.LayerState;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;

    /// <summary>
    /// 样板枚举项.
    /// </summary>
    [Flags]
    public enum TemplateItemEnum
    {
        /// <summary>
        /// 图层
        /// </summary>
        Layers = 1,

        /// <summary>
        /// 图层状态
        /// </summary>
        LayerStates = 2,

        /// <summary>
        /// 图层过滤器
        /// </summary>
        LayerFilters = 4,

        /// <summary>
        /// 文字样式
        /// </summary>
        TextStyles = 8,

        /// <summary>
        /// 标注样式
        /// </summary>
        DimStyles = 16,

        /// <summary>
        /// 多重引线样式
        /// </summary>
        MLeaderStyles = 32,

        /// <summary>
        /// 表格样式
        /// </summary>
        TableStyles = 64,
    }

    /// <summary>
    /// 从样板复制类.
    /// </summary>
    [PublicAPI]
    public class CopyFromTemplate
    {
        /// <summary>
        /// 复制.
        /// </summary>
        /// <param name="dbDest">目标Database.</param>
        /// <param name="sourceFile">样板源文件.</param>
        /// <param name="copyItems">(复制)样板选项.</param>
        public void Copy(Database dbDest, string sourceFile, TemplateItemEnum copyItems)
        {
            using (var dbSrc = new Database(false, false))
            {
                dbSrc.ReadDwgFile(sourceFile, FileOpenMode.OpenForReadAndAllShare, false, string.Empty);
                dbSrc.CloseInput(true);
                using (var t = dbSrc.TransactionManager.StartTransaction())
                {
                    // Layers
                    try
                    {
                        if (copyItems.HasFlag(TemplateItemEnum.LayerFilters))
                        {
                            ImportLayerFilter.ImportLayerFilterTree(dbSrc, dbDest);
                        }
                        else if (copyItems.HasFlag(TemplateItemEnum.Layers))
                        {
                            ImportLayerFilter.CopyLayers(dbSrc, dbDest);
                        }

                        if (copyItems.HasFlag(TemplateItemEnum.LayerStates))
                        {
                            ImportLayerState.ImportLayerStates(dbDest, dbSrc);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error(ex, "CopyFromTemplate layers");
                        $"Error copying layers - {ex.Message}".WriteToCommandLine();
                    }

                    if (copyItems.HasFlag(TemplateItemEnum.TextStyles))
                    {
                        this.CopySymbolTableItems(dbSrc.TextStyleTableId, dbDest.TextStyleTableId, "Text styles");
                    }

                    if (copyItems.HasFlag(TemplateItemEnum.DimStyles))
                    {
                        this.CopySymbolTableItems(dbSrc.DimStyleTableId, dbDest.DimStyleTableId, "Dimensional styles");
                    }

                    if (copyItems.HasFlag(TemplateItemEnum.TableStyles))
                    {
                        this.CopyDbDictItems(dbSrc.TableStyleDictionaryId, dbDest.TableStyleDictionaryId, "Table styles");
                    }

                    if (copyItems.HasFlag(TemplateItemEnum.MLeaderStyles))
                    {
                        this.CopyDbDictItems(dbSrc.MLeaderStyleDictionaryId, dbDest.MLeaderStyleDictionaryId, "Multileader styles");
                    }

                    t.Commit();
                }
            }
        }

        private void CopyDbDictItems(ObjectId srcDictId, ObjectId destDictId, string name)
        {
            try
            {
                var srcDict = srcDictId.GetObject<DBDictionary>();
                if (srcDict == null)
                {
                    return;
                }

                var ids = new List<ObjectId>(srcDict.Count);
                foreach (var entry in srcDict)
                {
                    ids.Add(entry.Value);
                }

                var idsCol = new ObjectIdCollection(ids.ToArray());
                srcDictId.Database.WblockCloneObjects(idsCol, destDictId, new IdMapping(), DuplicateRecordCloning.Replace, false);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, $"CopySymbolTableItems {name}.");
                $"Copy error {name} - {ex.Message}".WriteToCommandLine();
            }
        }

        private void CopySymbolTableItems(ObjectId srcSymbolTableId, ObjectId destSymbolTableId, string name)
        {
            try
            {
                var srcTable = srcSymbolTableId.GetObject<SymbolTable>();
                var idsCol = new ObjectIdCollection(srcTable.Cast<ObjectId>().ToArray());
                srcSymbolTableId.Database.WblockCloneObjects(idsCol, destSymbolTableId, new IdMapping(), DuplicateRecordCloning.Replace, false);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, $"CopySymbolTableItems {name}.");
                $"Copy error {name} - {ex.Message}".WriteToCommandLine();
            }
        }
    }
}
