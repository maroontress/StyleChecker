#pragma warning disable CS0219

namespace Application
{
    using System.IO;
    using System.Text;

    public sealed class Code
    {
        public void Arconym()
        {
            var sb = new StringBuilder();
            var ms = new MemoryStream();
        }

        public void Hungarian()
        {
            var bIsTrue = false;

            var bRet = (byte) 255;
            var sbRet = (sbyte) -1;

            var iRet = -1;
            var uiRet = 1u;

            var sRet = (short) -1;
            var usRet = (ushort) 65535;

            var lRet = -1L;
            var ulRet = 1uL;

            var cRet = (char) 0;

            var fRet = 0.0f;
            var dRet = 0.0;

            var dPrice = (decimal) 100.0;
            var sPrice = "100.0";
            var oPrice = (object) sPrice;
        }
    }
}
