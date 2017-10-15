using System.Collections.Generic;

namespace Core.XmlDatabase
{
    public class ID
    {

        static Dictionary<string, ID> idTable = new Dictionary<string, ID>();

        private static ID noIDInternal;
        public static ID NoID
        {
            get
            {
                if (noIDInternal == null)
                    noIDInternal = CreateID("");

                return noIDInternal;
            }
        }

        private string str = string.Empty;

        public ID()
        {
            str = string.Empty;
        }

        public ID(string idString)
        {
            str = idString;
        }

        public static ID CreateID(string idString)
        {
            ID id;
            if (!idTable.TryGetValue(idString, out id))
            {
                id = new ID(idString);
                idTable[idString] = id;
            }

            return id;
        }

        public static ID GetID(string idString)
        {
            ID id;
            if (idTable.TryGetValue(idString, out id))
                return id;

            return NoID;
        }

        public static void ClearIDs()
        {
            idTable.Clear();
        }

        public override string ToString()
        {
            return str;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return str.GetHashCode();
        }

        public static bool IsNullOrNoID(ID id)
        {
            return id == null || id == NoID;
        }
    }
}