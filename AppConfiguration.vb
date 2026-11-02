Imports System
Imports System.IO
Imports System.Linq
Imports System.Text.Json

Namespace SidhawanForStudents
    Public Module AppConfiguration
        Public ReadOnly Property ConnectionString As String
            Get
                Dim environmentValue = Environment.GetEnvironmentVariable("SIDHAWAN_CONNECTION_STRING")
                If Not String.IsNullOrWhiteSpace(environmentValue) Then Return environmentValue

                Dim configPath = System.IO.Path.Combine(AppContext.BaseDirectory, "appsettings.json")
                If Not File.Exists(configPath) Then
                    Throw New FileNotFoundException("Database configuration file was not found.", configPath)
                End If

                Using document = JsonDocument.Parse(File.ReadAllText(configPath))
                    Dim value As JsonElement
                    If document.RootElement.TryGetProperty("ConnectionStrings", value) AndAlso
                       value.TryGetProperty("SidhawanDb", value) Then
                        Dim configuredConnectionString As String
                        If value.ValueKind = JsonValueKind.Array Then
                            configuredConnectionString = String.Join(String.Empty,
                                value.EnumerateArray().Select(Function(item) item.GetString()))
                        Else
                            configuredConnectionString = value.GetString()
                        End If
                        If Not String.IsNullOrWhiteSpace(configuredConnectionString) Then Return configuredConnectionString
                    End If
                End Using

                Throw New InvalidOperationException("The SidhawanDb connection string is missing from appsettings.json.")
            End Get
        End Property
    End Module
End Namespace
