namespace Unify2D
{

    class ToolsUI
    {
        public static uint ToColor32(byte r, byte g, byte b, byte a) { uint ret = a; ret <<= 8; ret += b; ret <<= 8; ret += g; ret <<= 8; ret += r; return ret; }



    }
}
