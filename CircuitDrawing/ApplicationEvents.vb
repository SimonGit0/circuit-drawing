Imports System.Globalization
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices
Namespace My
    ' Für MyApplication sind folgende Ereignisse verfügbar:
    ' Startup: Wird beim Starten der Anwendung noch vor dem Erstellen des Startformulars ausgelöst.
    ' Shutdown: Wird nach dem Schließen aller Anwendungsformulare ausgelöst.  Dieses Ereignis wird nicht ausgelöst, wenn die Anwendung mit einem Fehler beendet wird.
    ' UnhandledException: Wird bei einem Ausnahmefehler ausgelöst.
    ' StartupNextInstance: Wird beim Starten einer Einzelinstanzanwendung ausgelöst, wenn die Anwendung bereits aktiv ist. 
    ' NetworkAvailabilityChanged: Wird beim Herstellen oder Trennen der Netzwerkverbindung ausgelöst.
    Partial Friend Class MyApplication
        Private Sub MyApplication_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
            CircuitDrawing.Settings.loadSettings()
            Select Case CircuitDrawing.Settings.getSettings().sprache
                Case Vektorgrafik_Sprache.Deutsch
                    Thread.CurrentThread.CurrentUICulture = New CultureInfo("de-DE")
                Case Vektorgrafik_Sprache.Englisch
                    Thread.CurrentThread.CurrentUICulture = New CultureInfo("en")
            End Select
        End Sub

        Private Sub MyApplication_Shutdown(sender As Object, e As EventArgs) Handles Me.Shutdown
            CircuitDrawing.Settings.getSettings.saveSettings()
        End Sub
    End Class
End Namespace
