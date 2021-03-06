﻿Imports System.Data.SqlClient
Public Class Batchs
    Public con As New Conexion
    Public batchsnum As Double
    Dim cantidadR As Double
    Dim objectCode As Integer
    Public Shared SQL_Conexion As SqlConnection = New SqlConnection()
    Dim connectionString As String = Conexion.ObtenerConexion.ConnectionString
    Private Const CP_NOCLOSE_BUTTON As Integer = &H200

    Protected Overloads Overrides ReadOnly Property CreateParams() As CreateParams
        Get
            Dim myCp As CreateParams = MyBase.CreateParams
            myCp.ClassStyle = myCp.ClassStyle Or CP_NOCLOSE_BUTTON
            Return myCp
        End Get
    End Property
    Friend Sub load(itemcode As String, cantidad As Double, objectcodes As Integer)
        Label4.Text = itemcode
        Label2.Text = cantidad
        cantidadR = cantidad
        objectCode = objectcodes
        CargaItems(itemcode)
    End Sub

    Public Function CargaItems(itemcode As String)

        Dim SQL_da As SqlDataAdapter = New SqlDataAdapter("WITH batchs AS
(
SELECT T0.ItemCode,T0.BatchNum,SUM(CASE T0.Direction when 0 then 1 else -1 end * T0.Quantity) as Disponible,'' as A_Utilizar
FROM IBT1 T0 INNER JOIN OWHS T1 ON T0.WhsCode = T1.WhsCode
GROUP BY T0.BatchNum, T1.WhsCode,  T0.ItemCode)
SELECT ItemCode,BatchNum, Disponible, A_Utilizar  from batchs where Disponible > 0 and ItemCode ='" + itemcode + "' 
", con.ObtenerConexion())
        Dim i As DataGridViewCheckBoxColumn = New DataGridViewCheckBoxColumn()
        Dim existe As Boolean = DGV.Columns.Cast(Of DataGridViewColumn).Any(Function(x) x.Name = "CHK")
        If existe = False Then
            DGV.Columns.Add(i)
            i.HeaderText = "CHK"
            i.Name = "CHK"
            i.Width = 32
            i.DisplayIndex = 0
        End If
        Dim DT_dat As System.Data.DataTable = New System.Data.DataTable()
        SQL_da.Fill(DT_dat)
        DGV.DataSource = DT_dat
        con.ObtenerConexion.Close()
    End Function

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        If Decimal.Round(Label6.Text, 3, MidpointRounding.AwayFromZero) = 0 Then
            For Each row As DataGridViewRow In DGV.Rows
                Dim chk As DataGridViewCheckBoxCell = row.Cells("CHK")
                If chk.Value IsNot Nothing AndAlso chk.Value = True Then
                    If DGV.Rows(chk.RowIndex).Cells.Item(4).Value > DGV.Rows(chk.RowIndex).Cells.Item(3).Value Then
                        MessageBox.Show("Verifique consistencia en linea: " & chk.RowIndex + 1)
                        Exit Sub
                    Else
                        FrmP.ba.Add(DGV.Rows(chk.RowIndex).Cells.Item(2).Value.ToString)
                        FrmP.quantity.Add(DGV.Rows(chk.RowIndex).Cells.Item(4).Value.ToString)
                    End If

                End If
            Next
            Me.Hide()
        Else
            MessageBox.Show("Verifique que la cantidad requerida concuerde con el total consumido")
        End If
    End Sub


    Private Sub DGV_KeyUp(sender As Object, e As KeyEventArgs) Handles DGV.KeyUp

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Try


            Dim suma As Double
            Dim queda As Double
            For Each row As DataGridViewRow In DGV.Rows
                suma += Val(row.Cells(4).Value)
            Next

            queda = Decimal.Round((Convert.ToDouble(Label2.Text)), 3, MidpointRounding.AwayFromZero)
            If queda = suma Then
                Label6.Text = queda - suma
                Label6.Refresh()
                Button2.Visible = True
                Button1.Visible = True
            Else
                'queda = Convert.ToDouble(Label2.Text) - suma
                Label6.Text = queda - suma
                Label6.Refresh()
            End If
        Catch ex As Exception
            MessageBox.Show("Verifique que los CHECK esten correctos o no existan campos vacios seleccionados")
        End Try
    End Sub



    Private Sub DGV_CellBeginEdit(sender As Object, e As DataGridViewCellCancelEventArgs) Handles DGV.CellBeginEdit
        Dim i, j As Integer
        i = DGV.CurrentRow.Index
        DGV.Rows(i).Cells(DGV.Columns("CHK").Index).Value = True
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim result As Integer = MessageBox.Show("Desea salir del modulo?", "Atencion", MessageBoxButtons.YesNo)
        If result = DialogResult.No Then
            MessageBox.Show("Puede continuar")
        ElseIf result = DialogResult.Yes Then
            Try
                con.oCompany.Disconnect()
            Catch
            End Try
            MessageBox.Show("Cancele el Objeto e inicie nuevamente")
            Me.Hide()
        End If
    End Sub
End Class