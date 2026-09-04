// SPDX-License-Identifier: MIT

namespace Fahrenheit.Mods.Debug;

/// <summary>
///     Restores the bodies of stubbed-out debug print calls within the game. The output
///     is logged to the Stage0 console and to disk.
///     <para/>
///     Do not interface with this module directly. It is self-contained.
/// </summary>
[FhLoad(FhGameId.FFX | FhGameId.FFX2 | FhGameId.FFX2LM)]
public unsafe class FhDebugPrintModule : FhModule {

    /* [fkelava 17/7/25 02:33]
     * Taken experimentally from PhyrePrintf. If this is not large enough, increase it.
     */
    private const int _buf_sz = 16384;

    public FhDebugPrintModule() { }

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private void h_pprintf(int rc, string fmt, nint va0, nint va1, nint va2, nint va3, nint va4, nint va5, nint va6, nint va7, nint va8, nint va9, nint va10, nint va11, nint va12, nint va13, nint va14, nint va15) {
        if (fmt.StartsWith("[FFX_section", StringComparison.InvariantCulture)) return; // EFL logs supersede Phyre load prints

        fmt      = fmt.Trim();
        nint buf = Marshal.AllocHGlobal(_buf_sz);

        try {
            int rv = FhPInvoke._vsnprintf_s(buf, _buf_sz, 0xFFFF_FFFF, fmt, &va0);
            _logger.Log(FhLogLevel.Info, $"[{rc}] {Marshal.PtrToStringAnsi(buf)!}");
        }
        finally {
            Marshal.FreeHGlobal(buf);
        }
    }

    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private void h_printf(string fmt, nint va0, nint va1, nint va2, nint va3, nint va4, nint va5, nint va6, nint va7, nint va8, nint va9, nint va10, nint va11, nint va12, nint va13, nint va14, nint va15) {
        fmt      = fmt.Trim();
        nint buf = Marshal.AllocHGlobal(_buf_sz);

        try {
            int rv = FhPInvoke._vsnprintf_s(buf, _buf_sz, 0xFFFF_FFFF, fmt, &va0);
            _logger.Log(FhLogLevel.Info, Marshal.PtrToStringAnsi(buf)!);
        }
        finally {
            Marshal.FreeHGlobal(buf);
        }
    }

    public override bool init(FhModContext mod_context, FileStream global_state_file) {
        return FhCall.Phyre_PhyrePrintf  .hook(this, h_pprintf) &&
               FhCall.rcPrint            .hook(this, h_printf)  &&
               FhCall.dbgPrintf          .hook(this, h_printf)  &&
               FhCall.scePrintf          .hook(this, h_printf)  &&
               FhCall.AtelPs2DebugString .hook(this, h_printf)  &&
               FhCall.AtelPs2DebugString2.hook(this, h_printf);
    }
}
