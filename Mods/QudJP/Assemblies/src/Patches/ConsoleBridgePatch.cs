using System;
using System.Threading;
using ConsoleLib.Console;
using HarmonyLib;
using QudJP.ConsoleUI;

namespace QudJP.Patches
{
    [HarmonyPatch(typeof(TextConsole), nameof(TextConsole.DrawBuffer))]
    internal static class ConsoleBridgePatch
    {
        [HarmonyPrefix]
        private static bool Prefix(
            ScreenBuffer Buffer,
            IScreenBufferExtra? BufferExtra = null,
            bool bSkipIfOverlay = false)
        {
            if (Keyboard.Closed)
            {
                Thread.CurrentThread.Abort();
                throw new Exception("Stopping game thread with an exception!");
            }

            var manager = GameManager.Instance;
            var modernUI = manager != null && manager.ModernUI;
            var shouldRender = !bSkipIfOverlay || !modernUI;
            ConsoleFrame capturedFrame;

            lock (TextConsole.BufferCS)
            {
                if (shouldRender)
                {
                    TextConsole.CurrentBuffer.Copy(Buffer);
                    TextConsole.CurrentBuffer.ViewTag = manager?.CurrentGameView;
                    if (BufferExtra != null)
                    {
                        TextConsole.bufferExtras.Enqueue(BufferExtra);
                    }
                }

                capturedFrame = ConsoleBridge.Instance.CaptureFrame(Buffer);
                TextConsole.BufferUpdated = true;
            }

            TextConsole.DrawBufferEvent.Set();
            ConsoleBridge.Instance.PublishFrame(capturedFrame);

            return false;
        }
    }
}
