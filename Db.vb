Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Runtime.CompilerServices
Imports MySqlConnector

Namespace SidhawanForStudents

    Public NotInheritable Class Db

        ' Prevent instantiation
        Private Sub New()
        End Sub

        ' ---------- private internals ----------

        Private Shared Function GetConnectionString() As String
            Return AppConfiguration.ConnectionString
        End Function

        Private Shared Function CreateCommand(conn As MySqlConnection, sql As String, args As (String, Object)()) As MySqlCommand
            Dim cmd = New MySqlCommand(sql, conn)
            If args IsNot Nothing Then
                For Each field In args
                    cmd.Parameters.AddWithValue(field.Item1, If(field.Item2, DBNull.Value))
                Next
            End If
            Return cmd
        End Function

        ' ---------- public API ----------

        ' Runs a SELECT and maps each row to a T using the supplied function.
        Public Shared Function Query(Of T)(sql As String,
                                           map As Func(Of MySqlDataReader, T),
                                           ParamArray args As (String, Object)()) As List(Of T)
            Dim rows = New List(Of T)()
            Using conn = New MySqlConnection(GetConnectionString())
                conn.Open()
                Using cmd = CreateCommand(conn, sql, args)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            rows.Add(map(reader))
                        End While
                    End Using
                End Using
            End Using
            Return rows
        End Function

        ' Runs INSERT/UPDATE/DELETE and returns the number of affected rows.
        Public Shared Function Execute(sql As String, ParamArray args As (String, Object)()) As Integer
            Using conn = New MySqlConnection(GetConnectionString())
                conn.Open()
                Using cmd = CreateCommand(conn, sql, args)
                    Return cmd.ExecuteNonQuery()
                End Using
            End Using
        End Function

        ' Network testing
        Public Shared Function TestConnection() As String
            Try
                Using conn = New MySqlConnection(GetConnectionString())
                    conn.Open()
                End Using
                Return Nothing
            Catch ex As Exception
                Return ex.Message
            End Try
        End Function

    End Class

    ' Reading of Num and Str data
    Public Module DbReaderExtensions

        <Extension()>
        Public Function Str(r As MySqlDataReader, column As String) As String
            Dim i = r.GetOrdinal(column)
            If r.IsDBNull(i) Then Return ""
            Return If(Convert.ToString(r.GetValue(i)), "")
        End Function

        <Extension()>
        Public Function Num(r As MySqlDataReader, column As String) As String
            Dim i = r.GetOrdinal(column)
            If r.IsDBNull(i) Then Return ""
            Dim text As String = If(Convert.ToString(r.GetValue(i)), "")
            Dim d As Decimal
            If Decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, d) Then
                Return d.ToString("0.00", CultureInfo.InvariantCulture)
            End If
            Return text
        End Function

    End Module

End Namespace