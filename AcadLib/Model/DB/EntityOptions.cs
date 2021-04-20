﻿namespace AcadLib
{
    using System;
    using Autodesk.AutoCAD.Colors;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using NetLib.Monad;

    /// <summary>
    /// Настройки для объекта на чертеже
    /// </summary>
    public class EntityOptions
    {
        private LineWeight lineWeight;
        private bool isLineWeight;
        private int colorIndex;
        private bool isColorIndex;

        public EntityOptions()
        {
        }

        public EntityOptions(Entity ent)
        {
            AcadColor = ent.Color;
            Color = System.Drawing.Color.FromArgb(ent.Color.Red, ent.Color.Green, ent.Color.Blue);
            ColorIndex = ent.ColorIndex;
            Layer = ent.Layer;
            LayerId = ent.LayerId;
            LineTypeId = ent.LinetypeId;
            LineType = ent.Linetype;
            LinetypeScale = ent.LinetypeScale;
            LineWeight = ent.LineWeight;
            if (ent is Polyline pl && pl.HasWidth)
                PoliylineWidth = pl.Try(p => (double?)p.ConstantWidth, e => null);
            BlockScale = (ent as BlockReference)?.ScaleFactors.X;
            Transperency = ent.Transparency.IsByAlpha ? ent.Transparency.Alpha : (byte?) null;
        }

        public ObjectId LayerId { get; set; }

        public string Layer { get; set; }

        public ObjectId LineTypeId { get; set; }

        public string LineType { get; set; }

        public double? LinetypeScale { get; set; }

        public double? PoliylineWidth { get; set; }
        public double? BlockScale { get; set; }

        public byte? Transperency { get; set; }

        public LineWeight LineWeight
        {
            get => lineWeight;
            set
            {
                lineWeight = value;
                isLineWeight = true;
            }
        }

        public int ColorIndex
        {
            get => colorIndex;
            set
            {
                colorIndex = value;
                isColorIndex = true;
            }
        }

        public System.Drawing.Color Color { get; set; }

        public Autodesk.AutoCAD.Colors.Color AcadColor { get; set; }

        /// <summary>
        /// Создавать или копировать из шаблона отсутствующие значения в чертеже.
        /// </summary>
        public bool CheckCreateValues { get; set; }

        public virtual void SetOptions(Entity ent)
        {
            if (!ent.IsWriteEnabled)
                ent = ent.UpgradeOpenTr();
            SetLayer(ent);
            SetColor(ent);
            SetLineWeight(ent);
            SetLineType(ent);
            SetLinetypeScale(ent);
            if (PoliylineWidth != null && ent is Polyline pl) pl.ConstantWidth = PoliylineWidth.Value;
            if (BlockScale != null && ent is BlockReference blRef &&
                Math.Abs(blRef.ScaleFactors.X - BlockScale.Value) > 0.0001)
            {
                blRef.ScaleFactors = new Scale3d(BlockScale.Value);
            }

            if (Transperency != null)
            {
                ent.Transparency = new Transparency(Transperency.Value);
            }
        }

        public void SetLineType(Entity ent)
        {
            if (!LineTypeId.IsNull)
            {
                ent.LinetypeId = LineTypeId;
            }
            else if (!string.IsNullOrEmpty(LineType))
            {
                if (!IsStandartLinetypeName(LineType) && CheckCreateValues)
                {
                    ent.Database.LoadLineTypePIK(LineType);
                }

                ent.Linetype = LineType;
            }
        }

        public static bool IsStandartLinetypeName(string lineType)
        {
            return SymbolUtilityServices.IsLinetypeByBlockName(lineType) ||
                SymbolUtilityServices.IsLinetypeByLayerName(lineType) ||
                SymbolUtilityServices.IsLinetypeContinuousName(lineType);
        }

        public void SetLineWeight(Entity ent)
        {
            if (isLineWeight)
            {
                ent.LineWeight = LineWeight;
            }
        }

        public void SetColor(Entity ent)
        {
            if (isColorIndex)
            {
                ent.ColorIndex = ColorIndex;
            }
            else if (!Color.IsEmpty)
            {
                ent.Color = Autodesk.AutoCAD.Colors.Color.FromColor(Color);
            }
            else if (AcadColor != null)
            {
                ent.Color = AcadColor;
            }
        }

        public void SetLayer(Entity ent)
        {
            if (!LayerId.IsNull)
            {
                ent.LayerId = LayerId;
            }
            else if (!string.IsNullOrEmpty(Layer))
            {
                if (CheckCreateValues)
                {
                    Layers.LayerExt.CheckLayerState(Layer);
                }

                ent.Layer = Layer;
            }
        }

        private void SetLinetypeScale(Entity ent)
        {
            if (LinetypeScale != null && LinetypeScale.Value > 0)
            {
                ent.LinetypeScale = LinetypeScale.Value;
            }
        }
    }

    public static class EntityOptionsExt
    {
        public static void SetOptions(this Entity ent, EntityOptions? opt)
        {
            opt?.SetOptions(ent);
        }

        public static EntityOptions GetEntityOptions(this Entity ent)
        {
            return new EntityOptions(ent);
        }
    }
}
