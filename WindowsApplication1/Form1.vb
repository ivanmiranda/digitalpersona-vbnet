Imports MySql.Data.MySqlClient

Public Class Form1
    Dim myDB As MySqlConnection
    Dim WithEvents FpReg As FlexCodeSDK.FinFPReg
    Dim WithEvents FpVer As FlexCodeSDK.FinFPVer
    Dim Template As String

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        myDB = New MySqlConnection
        ' Conexion con la base de datos (yo use un xampp para montarla por eso el root no tiene password)
        myDB.ConnectionString = "server=127.0.0.1;user id=root;password=;database=lector"
        myDB.Open()
        If myDB.State = ConnectionState.Open Then
            Button1.Enabled = True
            Button2.Enabled = True
        Else
            Button1.Enabled = False
            Button2.Enabled = False
        End If
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        ' Registrar el uso del lector
        FpReg = New FlexCodeSDK.FinFPReg
        FpReg.AddDeviceInfo("G600E013579", "80BD969C057765E", "NDT1598304C8259C23A1DL6D")
        FpReg.FPRegistrationStart("A99DD762B46940A5C7F8713CC6915C86")
    End Sub

    Private Sub FpReg_FPRegistrationStatus(ByVal Status As FlexCodeSDK.RegistrationStatus) Handles FpReg.FPRegistrationStatus
        If Status = FlexCodeSDK.RegistrationStatus.r_OK Then
            'Esto se ejecuta cuando las lecturas necesarias para capturar la huella se cumplieron sin problemas
            Dim sqlCommand As New MySqlCommand
            sqlCommand.Connection = myDB
            'El contenido de template es un medium text (por lo general unos 3000 caracteres
            sqlCommand.CommandText = "INSERT INTO empleados (nombre, huella) VALUES('" & txtName.Text & "','" & Template & "')"
            sqlCommand.ExecuteNonQuery()
            txtName.Text = ""
            Label1.Text = "GUARDADO"
        End If
    End Sub

    Private Sub FpReg_FPRegistrationTemplate(ByVal FPTemplate As String) Handles FpReg.FPRegistrationTemplate
        ' Cada que se hace un registro de nueva huella, se genera un template que pongo en esta variable global para el uso en el formulario
        Template = FPTemplate
    End Sub

    Private Sub FpReg_FPSamplesNeeded(ByVal Samples As Short) Handles FpReg.FPSamplesNeeded
        ' Esto se ejecuta cuando se inicia el proceso de lectura y muestra las lecturas pendientes para el registro correcto (4)
        Label1.Text = Str(Samples) & " intentos más"
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Call Validar()
    End Sub

    Private Sub FpVer_FPVerificationID(ByVal ID As String, ByVal FingerNr As FlexCodeSDK.FingerNumber) Handles FpVer.FPVerificationID
        'Aqui entra cuando se encontró un template que coincide con el dedo escaneado, y, como se asigno el id (numero de empleado en este caso) de cada huella al hacer
        'la carga de datos al lector, entonces lo muestro
        MsgBox("ID de empleado " & ID)
    End Sub

    Private Sub FpVer_FPVerificationStatus(ByVal Status As FlexCodeSDK.VerificationStatus) Handles FpVer.FPVerificationStatus
        Debug.Print(FlexCodeSDK.VerificationStatus.v_OK)
        If Status = FlexCodeSDK.VerificationStatus.v_OK Then
            'La funcion anterior encuentra la huella que se está leyendo y este segmento de código se ejecuta cuando se hizo un match, aqui es
            'donde se puede hacer el insert de un registro de asistencia o lo que se necesite
        End If
    End Sub

    Private Sub Validar()
        FpVer = New FlexCodeSDK.FinFPVer
        FpVer.AddDeviceInfo("G600E013579", "80BD969C057765E", "NDT1598304C8259C23A1DL6D")

        'Leo la base de datos de empleados y sus huellas
        Dim sqlCommand As New MySqlCommand
        sqlCommand.Connection = myDB
        sqlCommand.CommandText = "SELECT id, nombre, huella FROM empleados"
        Dim rd As MySqlDataReader
        rd = sqlCommand.ExecuteReader()
        'FpVer.FPListClear()
        'Debug.Print(FpVer.GetMaxTemplate())
        Do While rd.Read()
            'Y registro cada una de ellas como el dedo 0 en el lector (si se tienen mas huellas por empleado es necesario que la tabla soporte
            ' guardar cada una y su identificacion en el lector
            FpVer.FPLoad(rd.GetString(0), FlexCodeSDK.FingerNumber.Fn_RightIndex, rd.GetValue(2), "A99DD762B46940A5C7F8713CC6915C86")
            Label1.Text = "Cargando empleado " & rd.GetString(1)
        Loop
        rd.Close()
        sqlCommand = Nothing
        ' Y activo el modo de verificacion
        Label1.Text = "Listo para leer"
        FpVer.FPVerificationStart()
    End Sub
End Class
