﻿using SuperComicLib.Core;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace SuperComicLib.XPatch
{
    using static BindingFlags;
    internal static unsafe class Helper
    {
        #region 공개 필드

        internal const BindingFlags flag0 = (BindingFlags)0x3C;
        internal const BindingFlags flag1 = (BindingFlags)0x3C3C;
        internal const BindingFlags flag2 = flag1 | DeclaredOnly;
        internal const BindingFlags mflag0 = Instance | NonPublic;
        internal const BindingFlags mflag1 = Static | NonPublic;
        internal const BindingFlags mflag2 = mflag1 | Public;

        internal const int PAGE_EXECUTE_READWRITE = 0x40;
        #endregion

        internal static Type ReturnType(MethodBase meth) => meth is ConstructorInfo ? typeof(void) : ((MethodInfo)meth).ReturnType;

        internal static void TryNoInlining(MethodBase method)
        {
            if (Type.GetType("Mono.Runtime") != null)
                *((ushort*)method.MethodHandle.Value + 1) |= (ushort)MethodImplOptions.NoInlining;
        }

        internal static byte* Write<T>(byte* source, T value) where T : unmanaged
        {
            *(T*)source = value;
            return source + sizeof(T);
        }

        internal static void WriteIL(IntPtr source, IntPtr dest)
        {
            if (Environment.OSVersion.Platform < PlatformID.Unix)
                NativeMethods.VirtualProtect(source, new IntPtr(1), PAGE_EXECUTE_READWRITE, out _);

            byte* src = (byte*)source;
            if (IntPtr.Size == sizeof(long))
            {
                // 0xE9로 시작하면 jmp relative 모드임
                if (*src == 0xE9)
                    // jmp를 타고, 지정된 주소로 이동함
                    src += *(int*)(source + 1) + 5;

                // movabs rax, <addr>
                // jmp rax
                src = Write(src, (ushort)0xB848);
                src = Write(src, dest.ToInt64());
                Write(src, (ushort)0xE0FF);
            }
            else
            {
                // push <value>
                // ret
                src = Write(src, (byte)0x68);
                src = Write(src, dest.ToInt32());
                Write(src, (byte)0xC3);
            }
        }

        internal static void ReplaceMethod(MethodBase oldmethod, IntPtr newmethod_ptr)
        {
            RuntimeMethodHandle o_handle = oldmethod.MethodHandle;
            RuntimeHelpers.PrepareMethod(o_handle);

            WriteIL(o_handle.GetFunctionPointer(), newmethod_ptr);
        }

        internal static void OriginalMethodBodyEmit(
            ILGenerator il,
            MethodBase meth,
            int offset,
            bool hasReturn)
        {
            using (MethodBodyReader reader = new MethodBodyReader(meth))
            {
                using (MethodBodyEditor editor = reader.GetEditor())
                    editor.WriteIL(il, offset, hasReturn);
                // dispose editor
            }   // dispose reader
        }
    }
}
