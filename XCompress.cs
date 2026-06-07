using System;
using System.Runtime.InteropServices;


namespace XeXtractor
{

    public static class XCompress
    {
        private static readonly bool IsMachine64Bit = IntPtr.Size == 8;

        private static bool Is64Bit => XCompress.IsMachine64Bit;

        [DllImport("xcompress32.dll", EntryPoint = "LDICreateDecompression")]
        private static extern int LDICreateDecompression32(
          ref int pcbDataBlockMax,
          ref XCompress.LzxDecompress pvConfiguration,
          int pfnma,
          int pfnmf,
          IntPtr pcbSrcBufferMin,
          ref int unknown,
          ref int ldiContext);

        [DllImport("xcompress32.dll", EntryPoint = "LDIDecompress")]
        private static extern int LDIDecompress32(
          int context,
          byte[] pbSrc,
          int cbSrc,
          byte[] pbDst,
          ref int pcbDecompressed);

        [DllImport("xcompress32.dll", EntryPoint = "LDIDestroyDecompression")]
        private static extern int LDIDestroyDecompression32(int context);

        [DllImport("xcompress32.dll", EntryPoint = "LDISetWindowData")]
        private static extern int LDISetWindowData32(int context, byte[] window, int size);

        [DllImport("xcompress32.dll", EntryPoint = "LDIResetDecompression")]
        private static extern int LDIResetDecompression32(int context);

        [DllImport("xcompress64.dll", EntryPoint = "LDICreateDecompression")]
        private static extern int LDICreateDecompression64(
          ref int pcbDataBlockMax,
          ref XCompress.LzxDecompress pvConfiguration,
          int pfnma,
          int pfnmf,
          IntPtr pcbSrcBufferMin,
          ref int unknown,
          ref int ldiContext);

        [DllImport("xcompress64.dll", EntryPoint = "LDIDecompress")]
        private static extern int LDIDecompress64(
          int context,
          byte[] pbSrc,
          int cbSrc,
          byte[] pbDst,
          ref int pcbDecompressed);

        [DllImport("xcompress64.dll", EntryPoint = "LDIDestroyDecompression")]
        private static extern int LDIDestroyDecompression64(int context);

        [DllImport("xcompress64.dll", EntryPoint = "LDISetWindowData")]
        private static extern int LDISetWindowData64(int context, byte[] window, int size);

        [DllImport("xcompress64.dll", EntryPoint = "LDIResetDecompression")]
        private static extern int LDIResetDecompression64(int context);

        public static int LDICreateDecompression(
          ref int pcbDataBlockMax,
          ref XCompress.LzxDecompress pvConfiguration,
          int pfnma,
          int pfnmf,
          IntPtr pcbSrcBufferMin,
          ref int unknown,
          ref int ldiContext)
        {
            return XCompress.Is64Bit ? XCompress.LDICreateDecompression64(ref pcbDataBlockMax, ref pvConfiguration, pfnma, pfnmf, pcbSrcBufferMin, ref unknown, ref ldiContext) : XCompress.LDICreateDecompression32(ref pcbDataBlockMax, ref pvConfiguration, pfnma, pfnmf, pcbSrcBufferMin, ref unknown, ref ldiContext);
        }

        public static int LDIDecompress(
          int context,
          byte[] pbSrc,
          int cbSrc,
          byte[] pbDst,
          ref int pcbDecompressed)
        {
            return XCompress.Is64Bit ? XCompress.LDIDecompress64(context, pbSrc, cbSrc, pbDst, ref pcbDecompressed) : XCompress.LDIDecompress32(context, pbSrc, cbSrc, pbDst, ref pcbDecompressed);
        }

        public static int LDIDestroyDecompression(int context)
        {
            return XCompress.Is64Bit ? XCompress.LDIDestroyDecompression64(context) : XCompress.LDIDestroyDecompression32(context);
        }

        public static int LDISetWindowData(int context, byte[] window, int size)
        {
            return XCompress.Is64Bit ? XCompress.LDISetWindowData64(context, window, size) : XCompress.LDISetWindowData32(context, window, size);
        }

        public static int LDIResetDecompression(int context)
        {
            return XCompress.Is64Bit ? XCompress.LDIResetDecompression64(context) : XCompress.LDIResetDecompression32(context);
        }

        public struct LzxDecompress
        {
            public long WindowSize;
            public long CpuType;
        }
    }
}