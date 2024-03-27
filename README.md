# MemoryLeakTest
A test project to try reproduce a memory leak in .NET7-8 when EF and ServiceProvider used.

This program tries to reproduce the Unmanaged (and heap gen 2) memory leak, described in this issue: https://github.com/dotnet/runtime/issues/95922
