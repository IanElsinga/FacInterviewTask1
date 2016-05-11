''' <summary>
''' The task is to convert an object containing financial transaction fields into an ISO-8583 byte array. 
'''     https://en.wikipedia.org/wiki/ISO_8583
'''     
'''     The goal is for the conversion function to convert the sample Transaction to ISO-8583.
'''     A full implementation is complex so provide as much functionality as you can in
'''     the timeframe given. 
'''     
'''     
''' </summary>
Public Class InterviewTask_ISO8583


    Public Sub RunTask()
        Dim trxn As New TransactionMessage With {.MessageID = "0100",
                                                .PAN = "4111111111111111",
                                                .Amount = "000000000199",
                                                .STAN = "123456",
                                                .ExpiryMonth = "01",
                                                .ExpiryYear = "18",
                                                .Currency = "840",
                                                .RetrievalReferenceNumber = "611216739898"}

        'MessageID is ISO 8583 Message Type Indicator
        'Only first bitmap is required

        'Use field storage specifications as specified in DHIISOMessageDefinition.vb in standard field definitions - fieldtype parameter.
        'For example, STAN is field 11 and is BCD

        'Expiration date field 14 should be MMYY

        'Use ASCII for Alphanumeric (AN) fields 



        Dim iso8583() As Byte = ConvertToISO8583(trxn)

        Console.Write(BytesToString(iso8583))


    End Sub

    Public Function ConvertToISO8583(trxn As TransactionMessage) As Byte()
        'Implement this
        Throw New NotImplementedException

    End Function


    Public Shared Function BytesToString(ByVal b() As Byte, Optional startIndex As Integer = 0, Optional length As Integer = 0) As String
        If b Is Nothing Then Return ""

        Dim endIndex As Integer = IIf(length > 0, startIndex + length - 1, b.Length - 1)

        BytesToString = ""
        For i As Integer = startIndex To endIndex
            BytesToString += b(i).ToString("X2") & " "
        Next
        BytesToString = BytesToString.TrimEnd
    End Function

End Class
