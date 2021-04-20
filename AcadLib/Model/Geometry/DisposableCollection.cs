﻿namespace AcadLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IDisposableCollection<T> : ICollection<T>, IDisposable
        where T : IDisposable
    {
        void AddRange(IEnumerable<T> items);

        IEnumerable<T> RemoveRange(IEnumerable<T> items);
    }

    public class DisposableSet<T> : HashSet<T>, IDisposableCollection<T>
        where T : IDisposable
    {
        public DisposableSet()
        {
        }

        public DisposableSet(IEnumerable<T> items)
        {
            AddRange(items);
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
                return;
            UnionWith(items);
        }

        public void Dispose()
        {
            if (Count > 0)
            {
                Exception? last = null;
                var list = this.ToList();
                Clear();
                foreach (var item in list)
                {
                    if (item != null)
                    {
                        try
                        {
                            item.Dispose();
                        }
                        catch (Exception ex)
                        {
                            last ??= ex;
                        }
                    }
                }

                if (last != null)
                    throw last;
            }
        }

        public IEnumerable<T> RemoveRange(IEnumerable<T> items)
        {
            if (items == null)
                return null;
            ExceptWith(items);
            return items;
        }
    }
}