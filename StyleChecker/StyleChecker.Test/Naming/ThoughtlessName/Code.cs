#pragma warning disable CS0168
#pragma warning disable CS0219

namespace Application
{
    using System;
    using System.IO;
    using System.Text;

    public sealed class Code
    {
        public void Arconym()
        {
            var sb = new StringBuilder();
            //@ ^a,sb,StringBuilder
            var @ms = new MemoryStream();
            //@ ^a,@ms,MemoryStream
        }

        public void Hungarian()
        {
            var bIsTrue = false;
            //@ ^h,bIsTrue,bool

            var bRet = (byte) 255;
            //@ ^h,bRet,byte
            var sbRet = (sbyte) -1;
            //@ ^h,sbRet,sbyte

            var iRet = -1;
            //@ ^h,iRet,int
            var uiRet = 1u;
            //@ ^h,uiRet,uint

            var sRet = (short) -1;
            //@ ^h,sRet,short
            var usRet = (ushort) 65535;
            //@ ^h,usRet,ushort

            var lRet = -1L;
            //@ ^h,lRet,long
            var ulRet = 1uL;
            //@ ^h,ulRet,ulong

            var cRet = (char) 0;
            //@ ^h,cRet,char

            var fRet = 0.0f;
            //@ ^h,fRet,float
            var dRet = 0.0;
            //@ ^h,dRet,double

            var dPrice = (decimal) 100.0;
            //@ ^h,dPrice,decimal
            var sPrice = "100.0";
            //@ ^h,sPrice,string
            var oPrice = (object) sPrice;
            //@ ^h,oPrice,object

            var @iVal = -1;
            //@ ^h,@iVal,int
        }

        public void Parameter(
            StringBuilder sb,
            //@           ^a,sb,StringBuilder
            int @iVal)
            //@ ^h,@iVal,int
        {
        }

        public void Designation()
        {
            void NewStringBuilder(out StringBuilder b)
            {
                b = new StringBuilder();
            }
            NewStringBuilder(out var sb);
            //@                      ^a,sb,StringBuilder
        }

        public void PatterMathcing(Stream s)
        {
            if (s is MemoryStream ms)
            //@                   ^a,ms,MemoryStream
            {
            }
        }

        public void Catch()
        {
            try
            {
            }
            catch (NullReferenceException nre)
            //@                           ^a,nre,NullReferenceException
            {
            }
        }

        public void ForEach(StringBuilder[] all)
        {
            foreach (var sb in all)
                //@      ^a,sb,StringBuilder
            {
            }
        }
    }
}
