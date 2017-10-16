
namespace Core.Network
{
    public class NT
    {
        public const byte LoginT = 10;

        public class LoginS
        {
            public const ushort loginUser = 1;
            public const ushort logoutUser = 2;
            public const ushort addUser = 3;
        }
    }
}