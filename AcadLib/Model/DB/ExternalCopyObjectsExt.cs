// <copyright file="ExternalCopyObjectsExt.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AcadLib.DB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;

    /// <summary>
    /// Copying objects from an external database.
    /// </summary>
    public static class ExternalCopyObjectsExt
    {
        /// <summary>
        /// Copying objects from an external database.
        /// </summary>
        /// <param name="dbDest">Target Database.</param>
        /// <param name="externalFile">External file.</param>
        /// <param name="mode">Copy mode.</param>
        /// <param name="getOwnerId">Getting a table containing copied elements.</param>
        /// <param name="getCopyIds">Getting a list of copied objects from a table.</param>
        /// <returns>Copied objects.</returns>
        public static List<ObjectId> Copy(this Database dbDest, string externalFile, DuplicateRecordCloning mode, Func<Database, ObjectId> getOwnerId, Func<ObjectId, List<ObjectId>> getCopyIds)
        {
            using (var dbSource = new Database(false, true))
            {
                dbSource.ReadDwgFile(externalFile, FileOpenMode.OpenForReadAndAllShare, false, string.Empty);
                dbSource.CloseInput(true);
                return Copy(dbDest, dbSource, mode, getOwnerId, getCopyIds);
            }
        }

        /// <summary>
        /// Copies objects from another drawing.
        /// </summary>
        /// <param name="dbDest">Destination drawing.</param>
        /// <param name="dbSrc">Drawing source.</param>
        /// <param name="mode">Copy mode.</param>
        /// <param name="getOwnerId">Receiving a container.</param>
        /// <param name="getCopyIds">Copy objects.</param>
        /// <returns>Copied objects.</returns>
        public static List<ObjectId> Copy(this Database dbDest, Database dbSrc, DuplicateRecordCloning mode, Func<Database, ObjectId> getOwnerId, Func<ObjectId, List<ObjectId>> getCopyIds)
        {
            List<ObjectId> idsSource;
            ObjectId ownerIdDest;
            using (var t = dbSrc.TransactionManager.StartTransaction())
            {
                var ownerIdSourse = getOwnerId(dbSrc);
                ownerIdDest = getOwnerId(dbDest);
                idsSource = getCopyIds(ownerIdSourse);
                t.Commit();
            }

            if (idsSource?.Any() != true)
            {
                return new List<ObjectId>();
            }

            using (var map = new IdMapping())
            {
                using (var ids = new ObjectIdCollection(idsSource.ToArray()))
                {
                    dbDest.WblockCloneObjects(ids, ownerIdDest, map, mode, false);
                    return idsSource.Select(s => map[s].Value).ToList();
                }
            }
        }

        /// <summary>
        /// deep clone.
        /// </summary>
        /// <param name="dbDest">Destination drawing.</param>
        /// <param name="owner">id.</param>
        /// <param name="idsCopy">Copy objects.</param>
        /// <param name="mode">copy mode.</param>
        /// <returns>Copied objects.</returns>
        public static List<ObjectId> WblockCloneObjects(this Database dbDest, ObjectId owner, List<ObjectId> idsCopy, DuplicateRecordCloning mode)
        {
            using (var map = new IdMapping())
            {
                using (var ids = new ObjectIdCollection(idsCopy.ToArray()))
                {
                    dbDest.WblockCloneObjects(ids, owner, map, mode, false);
                    return idsCopy.Select(s => map[s].Value).ToList();
                }
            }
        }
    }
}
