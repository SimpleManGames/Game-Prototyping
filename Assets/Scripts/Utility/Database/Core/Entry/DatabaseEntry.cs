using System.ComponentModel;
using System.Xml.Serialization;

namespace Core.XmlDatabase
{
    public abstract class DatabaseEntry
    {
        [XmlIgnore]
        public ID DatabaseID { get; private set; }

        [XmlIgnore]
        public Database Database { get; private set; }

        [XmlAttribute("ID"), EditorBrowsable(EditorBrowsableState.Never)]
        public string _DatabaseIDSurrogate
        {
            get
            {
                return ID.IsNullOrNoID(DatabaseID) ? ID.NoID.ToString() : DatabaseID.ToString();
            }

            set
            {
                DatabaseID = ID.CreateID(value);
            }
        }

        public void PostLoad(Database db)
        {
            Database = db;
            OnPostLoad();
        }

        protected virtual void OnPostLoad() { }
    }
}