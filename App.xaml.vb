Imports System.Windows

Namespace SidhawanForStudents
    Partial Public Class App
        Inherits System.Windows.Application

        Protected Overrides Sub OnStartup(e As StartupEventArgs)
            Dim [error] As String = Db.TestConnection()

            If [error] IsNot Nothing Then
                System.Diagnostics.Trace.TraceError("DB connection failed: " & [error])
                MessageBox.Show("Could not connect to the server. Please check your connection and try again.",
                                "Connection problem", MessageBoxButton.OK, MessageBoxImage.Error)
                Shutdown()
                Return
            End If

            GradesService.Start()
            MyBase.OnStartup(e)
        End Sub
    End Class
End Namespace