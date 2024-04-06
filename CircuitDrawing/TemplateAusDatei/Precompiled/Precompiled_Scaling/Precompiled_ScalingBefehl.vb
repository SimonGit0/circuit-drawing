Imports System.IO
Public MustInherit Class Precompiled_ScalingBefehl
    Public MustOverride Function Compile(args As AusrechnenArgs, erg As Precompiled_Scaling_CompileArgs) As Boolean

    Public MustOverride Sub speichern(writer As BinaryWriter)

    Public MustOverride Sub export(writer As Export_StreamWriter)
End Class
