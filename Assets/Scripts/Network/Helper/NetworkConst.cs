namespace Core.Network.Const
{
    public class NT
    {
        #region _Tags_

        public const byte StartT = 5;
        public const byte MoveT = 6;
        public const byte LoginT = 10;
        public const byte PlayerT = 11;

        #endregion

        #region _Subjects_

        public class StartS
        {
            public const ushort JoinGame = 1;
            public const ushort Spawn = 2;
        }

        public class MoveS
        {
            public const ushort Position = 1;
            public const ushort Rotation = 2;
        }

        public class PlayerS
        {
            public const ushort playerSaveData = 1;
            public const ushort playerLoadData = 2;
            public const ushort playerSavedOkData = 3;
            public const ushort playerRecieveData = 4;
        }

        public class LoginS
        {
            public const ushort loginUser = 1;
            public const ushort logoutUser = 2;
            public const ushort addUser = 3;

            public const ushort loginUserSuccess = 4;
            public const ushort loginUserFailed = 5;
            public const ushort logoutSuccess = 6;
            public const ushort addUserSuccess = 7;
            public const ushort addUserFailed = 8;
        }

        #endregion
    }
}