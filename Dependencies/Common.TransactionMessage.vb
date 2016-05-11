Imports System.Configuration
Imports System.Text
Imports System.Linq


#Region "Class TransactionMessage"

''' <summary>
''' An instance of this class represents a card transaction in non-binary format.  
''' Instances of this class will be passed to and from the translator components and plugins. Note 
''' that this class has evolved to hold more than just DHI ISO information.  It has become a general 
''' financial message class. 
''' </summary>
''' <remarks></remarks>
''' 
<Serializable()> _
Public Class TransactionMessage
    Public MessageID As String = ""
    Private fieldValues As Dictionary(Of Integer, String)
    Private binaryFieldValues As Dictionary(Of Integer, Byte())


    'Key of Field Definitions to use for this message.  If this is not initialized, then use the static DHIISOMessageDefinition fields as normal.
    Public FieldDefinitionSetKey As String = Nothing 'This should be null for normal multithreaded transaction processing for performance reasons


    ''' <summary>
    ''' Number of digits to the right of the decimal point in the currency. 
    ''' </summary>
    ''' <remarks></remarks>
    Public CurrencyDecimalDigits As Integer = -1





    Public Sub New()
        fieldValues = New Dictionary(Of Integer, String)
        binaryFieldValues = New Dictionary(Of Integer, Byte())
    End Sub



#Region "Field properties"
    'These will get and set from the base string representation of the fields in the fieldValues dictionary
    'In some cases, further parsing will be done to return a portion of the base string

    ''' <summary>
    ''' ISO Field 2.  Credit card number. 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PAN() As String
        Get
            PAN = GetField(2)
        End Get
        Set(ByVal value As String)
            SetField(2, value)
        End Set
    End Property

    ''' <summary>
    ''' ISO Field 3.  Identifies the cardholder transaction type and account type. 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ProcessingCode() As String
        Get
            ProcessingCode = GetField(3)
        End Get
        Set(ByVal value As String)
            If value.Length <> 6 Then Throw New Exception("ProcessingCode must be 6 digits")
            SetField(3, value)
        End Set
    End Property

    ''' <summary>
    ''' ISO Field 4.  String decimal format. 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>When this is used to set the amount, it will also derive the currency
    ''' decimal digits from the passed in value.  1.000 for example would result
    ''' in the CurrencyDecimalDigits being set to 3. 
    ''' When this is used to get the amount, the currency decimal digits needs to have
    ''' already been set or an exception will be thrown. </remarks>
    Public Property AmountWithDecimal() As String
        Get
            Dim s As String = GetField(4)

            CurrencyDecimalDigits = 2


            s = s.TrimStart("0")


            AmountWithDecimal = Format(Val(s) / (10 ^ CurrencyDecimalDigits), New String("0.").PadRight(CurrencyDecimalDigits + 2, "0"))
            If CurrencyDecimalDigits = 0 And AmountWithDecimal.EndsWith(".") Then AmountWithDecimal = AmountWithDecimal.TrimEnd(".")

        End Get
        Set(ByVal value As String)
            If value.Length = 0 Then Throw New Exception("TransactionMessage.AmountWithDecimal - value cannot be a blank string")

            If CurrencyDecimalDigits < 0 Then  'Don't change the decimal digits unless they are not set yet
                If value.IndexOf(".") = -1 Then
                    CurrencyDecimalDigits = 0
                Else
                    CurrencyDecimalDigits = value.Length - value.IndexOf(".") - 1
                End If
            End If

            value = Format(Val(value), New String("0.").PadRight(CurrencyDecimalDigits + 2, "0"))

            SetField(4, value.Replace(".", "").PadLeft(12, "0"))
        End Set
    End Property



    ''' <summary>
    ''' ISO Field 4.  The string representation of the 12 digit amount padded left with 0's. 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Amount() As String
        Get
            Amount = GetField(4)
        End Get
        Set(ByVal value As String)
            If value.Length < 12 Then value.PadLeft(12, "0")
            If value.Length > 12 Then Throw New Exception("TransactionMessage.Amount cannot be more than 12 characters")
            If value.Contains(".") Then Throw New Exception("TransactionMessage.Amount cannot contain decimal points")
            SetField(4, value)
        End Set
    End Property

    ''' <summary>
    ''' ISO Field 7.  Transmission date and time MMDDhhmmss
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TransmissionDateTime() As String
        Get
            TransmissionDateTime = GetField(7)
        End Get
        Set(ByVal value As String)
            SetField(7, value.Substring(0, 10))
        End Set
    End Property

    ''' <summary>
    ''' ISO Field 11.  Systems Trace Audit Number
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property STAN() As String
        Get
            STAN = GetField(11)
        End Get
        Set(ByVal value As String)
            SetField(11, value.Substring(0, 6))
        End Set
    End Property

    ''' <summary>
    ''' Expiry year of ISO Field 14. 2 digits. 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ExpiryYear() As String
        Get
            ExpiryYear = ""
            If GetField(14).Length = 4 Then ExpiryYear = GetField(14).Substring(0, 2)

        End Get
        Set(ByVal value As String)
            If value.Length <> 2 Then Throw New Exception("ExpiryYear must be 2 characters")

            If fieldValues.ContainsKey(14) AndAlso GetField(14).Length = 4 Then
                SetField(14, value & fieldValues(14).Substring(2, 2))
            Else
                SetField(14, value & "  ")
            End If

        End Set
    End Property

    ''' <summary>
    ''' Expiry month of ISO Field 14.  2 digits. 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ExpiryMonth() As String
        Get
            ExpiryMonth = ""
            If GetField(14).Length = 4 Then ExpiryMonth = GetField(14).Substring(2, 2)
        End Get
        Set(ByVal value As String)
            If value.Length <> 2 Then Throw New Exception("ExpiryMonth must be 2 characters")
            If fieldValues.ContainsKey(14) AndAlso GetField(14).Length = 4 Then
                SetField(14, fieldValues(14).Substring(0, 2) & value)
            Else
                SetField(14, "  " & value)
            End If
        End Set
    End Property



    ''' <summary>
    ''' ISO Field 32. Acquirer ID.  6 digits.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property AcquirerId() As String
        Get
            AcquirerId = GetField(32)
        End Get
        Set(ByVal value As String)
            If value.Length > 11 Then Throw New Exception("AcquirerId must be less than 12 digits")
            SetField(32, value)
        End Set
    End Property

    ''' <summary>
    ''' ISO Field 37.  Retrieval Reference Number. 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property RetrievalReferenceNumber() As String
        Get
            RetrievalReferenceNumber = GetField(37)
        End Get
        Set(ByVal value As String)
            If value.Length > 12 Then Throw New Exception("Retrieval Reference Number must be less than 12 digits.")
            SetField(37, value)
        End Set
    End Property

    ''' <summary>
    ''' ISO Field 38.  AuthId.  6 characters.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property AuthId() As String
        Get
            Return GetField(38)
        End Get
        Set(ByVal value As String)
            If value.Length > 6 Then Throw New Exception("AuthId must be 6 characters or less")
            SetField(38, value)
        End Set
    End Property

    ''' <summary>
    ''' ISO Field 39.  Response Code. 2 characters.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ISOResponseCode() As String
        Get
            ISOResponseCode = GetField(39)
        End Get
        Set(ByVal value As String)
            If value.Length <> 2 Then Throw New Exception("ISOResponseCode must be 2 characters")
            SetField(39, value.Substring(0, 2))
        End Set
    End Property

    ''' <summary>
    ''' ISO Field 41. Card Acceptor Terminal ID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TerminalID() As String
        Get
            TerminalID = GetField(41)
        End Get
        Set(ByVal value As String)
            If value.Length <> 8 Then Throw New Exception("TerminalID must be 8 characters")
            SetField(41, value)
        End Set
    End Property

    ''' <summary>
    ''' ISO Field 42. Card Acceptor Identification Code. 15 characters. 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>MerchantId</remarks>
    Public Property ProcessingMid() As String
        Get
            Return GetField(42).Trim
        End Get
        Set(ByVal value As String)
            If value.Length > 15 Then Throw New Exception("MerchantId must be 15 characters or less")
            SetField(42, value)
        End Set
    End Property

    ''' <summary>
    ''' ISO Field 43.  Card Acceptor Name/Location.  40 Characters
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CardAcceptorNameLocation() As String
        Get
            CardAcceptorNameLocation = GetField(43)
        End Get
        Set(ByVal value As String)
            If value.Length > 40 Then Throw New Exception("CardAcceptorNameLocation must be 40 characters or less")
            SetField(43, value)
        End Set
    End Property

    ''' <summary>
    ''' ISO Field 43.1 Card Acceptor Name.  25 Characters
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>Internally, field 43 must be 40 characters</remarks>
    Public Property CardAcceptorName() As String
        'Unit Tested
        Get
            CardAcceptorName = GetField(43)
            If CardAcceptorName.Length > 24 Then CardAcceptorName = CardAcceptorName.Substring(0, 25).Trim
        End Get
        Set(ByVal value As String)
            If value.Length > 25 Then Throw New Exception("CardAcceptorName must be 25 characters or less.")
            Dim s As String = GetField(43)
            If s.Length <> 40 Then
                SetField(43, value.PadRight(40, " "))
            Else
                SetField(43, value.PadRight(25, " ") & s.Substring(25, 15))
            End If
        End Set
    End Property

    ''' <summary>
    ''' ISO Field 43.2 Card Acceptor City.  12 Characters
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>Internally, field 43 must be 40 characters</remarks>
    Public Property CardAcceptorCity() As String
        'Unit Tested
        Get
            CardAcceptorCity = GetField(43)
            If CardAcceptorCity.Length > 36 Then
                CardAcceptorCity = CardAcceptorCity.Substring(25, 12).Trim
            Else
                CardAcceptorCity = ""
            End If

        End Get
        Set(ByVal value As String)
            If value.Length > 12 Then Throw New Exception("CardAcceptorCity must be 12 characters or less.")
            Dim s As String = GetField(43)
            If s.Length <> 40 Then
                SetField(43, New String(" ", 25) & value.PadRight(15, " "))
            Else
                SetField(43, s.Substring(0, 25) & value.PadRight(13, " ") & s.Substring(38, 2))
            End If
        End Set
    End Property

    ''' <summary>
    ''' ISO Field 43.3 Card Acceptor Country.  2 Characters
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>Internally, field 43 must be 40 characters</remarks>
    Public Property CardAcceptorCountry() As String
        'Unit Tested
        Get
            CardAcceptorCountry = GetField(43)
            If CardAcceptorCountry.Length = 40 Then
                CardAcceptorCountry = CardAcceptorCountry.Substring(38, 2).Trim
            Else
                CardAcceptorCountry = ""
            End If

        End Get
        Set(ByVal value As String)
            If value.Length <> 2 Then Throw New Exception("CardAcceptorCountry must be 2 characters.")
            Dim s As String = GetField(43)
            If s.Length <> 40 Then
                SetField(43, New String(" ", 38) & value)
            Else
                SetField(43, s.Substring(0, 38) & value)
            End If
        End Set
    End Property






    ''' <summary>
    ''' ISO Field 49.  Currency.  3 characters. 
    ''' Internally this is padded left with a 0
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Currency() As String
        Get
            Dim s As String = GetField(49)
            If s.Length = 4 Then
                Return GetField(49).Substring(1, 3)
            Else
                Return s
            End If

        End Get
        Set(ByVal value As String)
            If value.Length <> 3 Then Throw New Exception("Currency must be 3 digits")
            Dim decdigits As Integer = 2

            If decdigits >= 0 Then
                CurrencyDecimalDigits = 2
                SetField(49, value.Substring(0, 3))
            Else
                Throw New Exception("TransactionMessage.Currency Set - Invalid currency.")
            End If

        End Set
    End Property




#End Region

#Region "Miscellaneous properties"



    Public ReadOnly Property Fields() As Dictionary(Of Integer, String)
        Get
            Fields = fieldValues
        End Get
    End Property

    Public ReadOnly Property BinaryFields() As Dictionary(Of Integer, Byte())
        Get
            BinaryFields = binaryFieldValues
        End Get
    End Property

#End Region

#Region "Miscellaneous Methods"

    Private Function GetField(ByVal fieldnumber As Integer) As String
        If fieldValues.ContainsKey(fieldnumber) Then
            GetField = fieldValues(fieldnumber)
        Else
            GetField = ""
        End If
    End Function


    Private Sub SetField(ByVal fieldnumber As Integer, ByVal value As String)
        If fieldValues.ContainsKey(fieldnumber) Then
            fieldValues(fieldnumber) = value
        Else
            fieldValues.Add(fieldnumber, value)


        End If
    End Sub





    ''' <summary>
    ''' Return a binary field.
    ''' </summary>
    ''' <param name="fieldnumber"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetBinaryField(ByVal fieldnumber As Integer) As Byte()
        If binaryFieldValues.ContainsKey(fieldnumber) Then
            GetBinaryField = binaryFieldValues(fieldnumber)
        Else
            GetBinaryField = Nothing
        End If
    End Function

    ''' <summary>
    ''' Set a binary field with binary data.
    ''' </summary>
    ''' <param name="fieldnumber"></param>
    ''' <param name="value"></param>
    ''' <remarks></remarks>
    Public Sub SetBinaryField(ByVal fieldnumber As Integer, ByVal value() As Byte)
        If binaryFieldValues.ContainsKey(fieldnumber) Then
            binaryFieldValues(fieldnumber) = value
        Else
            binaryFieldValues.Add(fieldnumber, value)


        End If
    End Sub







#End Region


End Class

#End Region



