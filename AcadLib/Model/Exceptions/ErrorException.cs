﻿namespace AcadLib.Exceptions
{
    using System;
    using System.Drawing;
    using Autodesk.AutoCAD.DatabaseServices;

    /// <summary>
    /// Ошибка с Error (можно показать объект на чертеже)
    /// </summary>
    public class ErrorException : Exception
    {
        public ErrorException(Errors.Error err) : base(err.Message)
        {
            Error = err;
        }

        public ErrorException(string msg, Entity ent, Icon icon) : base(msg)
        {
            Error = new Errors.Error(msg, ent, icon);
        }

        public ErrorException(string msg, Icon icon) : base(msg)
        {
            Error = new Errors.Error(msg, icon);
        }

        public Errors.Error Error { get; private set; }
    }
}