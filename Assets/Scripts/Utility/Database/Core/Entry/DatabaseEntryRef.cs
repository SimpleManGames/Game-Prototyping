using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Core.XmlDatabase
{
    public sealed class DatabaseEntryRef<T> where T : DatabaseEntry
    {
        private ID id;
        private T cachedEntry;
        private bool entryHasBeenCached = false;

        private static DatabaseEntryRef<T> empty = new DatabaseEntryRef<T>();
        public static DatabaseEntryRef<T> Empty
        {
            get { return empty; }
        }

        public bool IsValid
        {
            get { return !ID.IsNullOrNoID(id); }
        }

        public T Entry
        {
            get
            {
                if (!entryHasBeenCached)
                {
                    CacheEntry();
                    entryHasBeenCached = true;
                }

                return cachedEntry;
            }
        }

        [XmlText, EditorBrowsable(EditorBrowsableState.Never)]
        public string _IDSurrogate
        {
            get
            {
                return ID.IsNullOrNoID(id) ? ID.NoID.ToString() : id.ToString();
            }

            set
            {
                id = ID.CreateID(value);
            }
        }

        public DatabaseEntryRef()
        {
            Database.Instance.OnFinishedLoading += FinishedLoadingHandler;
        }

        private void FinishedLoadingHandler()
        {
            CacheEntry();
            Database.Instance.OnFinishedLoading -= FinishedLoadingHandler;
        }

        public void CacheEntry()
        {
            entryHasBeenCached = true;

            if (ID.IsNullOrNoID(id))
            {
                cachedEntry = null;
                return;
            }

            cachedEntry = Database.Instance.GetEntry<T>(id);

            if (cachedEntry == null)
                throw new Exception("Invalid Database Entry ID: " + id.ToString());
        }

        public static implicit operator ID(DatabaseEntryRef<T> entryRef)
        {
            return entryRef.id;
        }

        public static implicit operator T(DatabaseEntryRef<T> entryRef)
        {
            return entryRef.Entry;
        }
    }
}