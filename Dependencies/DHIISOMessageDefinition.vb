
Imports System.Text

''' <summary>
''' Contains the field definitions for iso8583 messages. 
''' </summary>
''' <remarks>
''' 2014: Support added for FDMS implementation of ISO8583</remarks>
Public Class DHIISOMessageDefinition

    Private Shared FieldDefs As Dictionary(Of Integer, FieldDef)


    'Collection of definitions for use in multi-host scenarios
    Private Shared FieldDefinitionsCollection As New Dictionary(Of String, Dictionary(Of Integer, FieldDef))
    Public Shared Property ActiveDefinitionsName As String

    Public Shared maxlength As Integer



    Public Enum FieldTypes
        'AN = Alphanumeric (Either EBCDIC or ASCII - default is EBCDIC)
        'L = Length
        'BCD = Binary Coded Decimal

        BCD = 1                 'Field: BCD; Length: fixed
        L_BCD = 2               'Field: BCD; Length: one byte, byte count of the payload. 
        AN = 3                  'Field: AlphaNumeric; Length: fixed
        BITMAP = 4
        TLV = 5                 'Visa specific. Note that the length parameter for TLV is used to hold the datasetID instead
        L_AN = 6                'Field: AlphaNumeric; Length: one byte payload character length
        LLLAN_AN = 7            'Field: AlphaNumeric; Length: 3 ebcdic bytes indicating payload character length
        BINARY = 8              'Field: Binary; Length: fixed
        LN_BCD = 9              'Field: BCD; Length: one byte, nibble count of the payload. By default odd number of digits are right justified

        '**** All of the following fields were added Feb/March 2014 to support FDMS ISO8583

        LBCD_BCD = 10           'Field: BCD;  Length: one byte BCD, payload nibble count. By default extra nibble for odd number of digits is on the left (right justified).
        LLBCD_BCD = 11          'Field: BCD;  Length: two byte (three digit) BCD, payload nibble count
        LBCD_AN = 12            'Field: AlphaNumeric;  Length: one byte BCD, payload character length
        LLBCD_AN = 13           'Field: AlphaNumeric;  Length: two byte (three digit) BCD, payload character length

        FDMSTABLES = 14         'First Data specific. Private Data Field tables (collection of FDMSTABLE)
        FDMSTABLE = 15          'First Data specific. Private Data Field table

        CUSTOM1 = 16            'Custom fields for host-specific field additions.
        CUSTOM2 = 17
        CUSTOM3 = 18
        CUSTOM4 = 19

        LLAN_AN = 20           'Field: AlphaNumeric;  Length: 2 ebcdic bytes indicating payload character length




    End Enum

    Friend Shared ReadOnly Property Fields() As Dictionary(Of Integer, FieldDef)
        Get
            Fields = FieldDefs
        End Get
    End Property

    Shared Sub New()
        'By default use standard definitions
        AddOrReplaceDefinitionSet("standard", GetStandardDefinitions())

        SwitchActiveDefinitions("standard")

        'Maximum length of a message.  VISA ISO max length without extended fields is 800 bytes
        maxlength = 1500 'Best guess maximum length for all message types for all definitions. 
    End Sub

    ''' <summary>
    ''' Switch the current static field definition set.  WARNING: This is NOT thread-safe. Do not use this mechanism in multithreaded scenarios. 
    ''' </summary>
    ''' <param name="key"></param>
    ''' <remarks></remarks>
    Public Shared Sub SwitchActiveDefinitions(key As String)

        If FieldDefinitionsCollection.ContainsKey(key.ToLower) Then
            FieldDefs = FieldDefinitionsCollection(key)
            ActiveDefinitionsName = key
        Else
            Throw New ArgumentException("No such field definitionSet key: " & key)
        End If

    End Sub

    Public Shared Sub AddOrReplaceDefinitionSet(key As String, fields As Dictionary(Of Integer, FieldDef))
        If FieldDefinitionsCollection.ContainsKey(key.ToLower()) Then
            FieldDefinitionsCollection(key) = fields
        Else
            FieldDefinitionsCollection.Add(key, fields)
        End If
    End Sub

    Public Shared ReadOnly Property FieldDefinition(fieldnumber As Integer, Optional definitionSetKey As String = Nothing) As FieldDef
        Get
            If String.IsNullOrEmpty(definitionSetKey) Then
                'Current default set
                If FieldDefs.ContainsKey(fieldnumber) Then
                    Return FieldDefs(fieldnumber)
                Else
                    Return Nothing
                End If

            Else
                'Specific set
                If FieldDefinitionsCollection.ContainsKey(definitionSetKey) AndAlso FieldDefinitionsCollection(definitionSetKey).ContainsKey(fieldnumber) Then
                    Return FieldDefinitionsCollection(definitionSetKey)(fieldnumber)
                Else
                    Return Nothing
                End If

            End If
        End Get
    End Property

    Public Shared ReadOnly Property FieldDefinitions(Optional definitionSetKey As String = Nothing) As Dictionary(Of Integer, FieldDef)
        Get
            If String.IsNullOrEmpty(definitionSetKey) Then
                'Current default set
                Return FieldDefs
            Else
                'Specific set
                If FieldDefinitionsCollection.ContainsKey(definitionSetKey) Then
                    Return FieldDefinitionsCollection(definitionSetKey)
                Else
                    Return Nothing
                End If
            End If
        End Get
    End Property




    ''' <summary>
    ''' Create a set of standard field definitions.
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared Function GetStandardDefinitions() As Dictionary(Of Integer, FieldDef)

        'BCD is the default field type if not specified.

        Dim fdefs As New Dictionary(Of Integer, FieldDef)
        fdefs.Add(1, New FieldDef(0, "Primary bitmap subfields", FieldTypes.BITMAP))
        fdefs.Add(2, New FieldDef(10, "PAN", FieldTypes.LN_BCD, True, True))
        fdefs.Add(3, New FieldDef(3, "Processing Code", , True, True))
        fdefs.Add(4, New FieldDef(6, "Transaction Amount", , True, True))
        fdefs.Add(7, New FieldDef(5, "Transmission Date and Time", , True, True))
        fdefs.Add(11, New FieldDef(3, "Systems Trace Audit Number", , True, True))
        fdefs.Add(14, New FieldDef(2, "Expiration Date"))
        fdefs.Add(18, New FieldDef(2, "Merchant Type"))
        fdefs.Add(19, New FieldDef(2, "Acquiring Country Code", , True, True))
        fdefs.Add(22, New FieldDef(2, "POS Entry Mode Code"))
        fdefs.Add(25, New FieldDef(1, "POS Condition Code", , True, True))

        fdefs.Add(32, New FieldDef(6, "Aquirer ID", FieldTypes.LN_BCD, True, True))
        fdefs.Add(37, New FieldDef(12, "Retrieval Reference Number", FieldTypes.AN, True, True))
        fdefs.Add(38, New FieldDef(6, "Authorization Identification Response", FieldTypes.AN, True, True))
        fdefs.Add(39, New FieldDef(2, "Response Code", FieldTypes.AN, True, True))
        fdefs.Add(41, New FieldDef(8, "Card Acceptor Terminal ID", FieldTypes.AN, True, True))
        fdefs.Add(42, New FieldDef(15, "Card Acceptor ID Code", FieldTypes.AN, True, True))
        fdefs.Add(43, New FieldDef(40, "Card Acceptor Name/Location", FieldTypes.AN))

        'Field 44 will be stored as an EBCDIC string and internal padding (e.g. for 44.1) 
        'will be spaces.  
        fdefs.Add(44, New FieldDef(13, "Additional Response Data", FieldTypes.L_AN, True, True))
        fdefs.Add(48, New FieldDef(128, "Additional Data - Private", FieldTypes.LLLAN_AN))
        fdefs.Add(49, New FieldDef(2, "Transaction Currency Code", , True, True))
        fdefs.Add(54, New FieldDef(60, "Additional Amounts", FieldTypes.L_BCD))
        fdefs.Add(55, New FieldDef(255, "BER-TLV Chip Data - nonIso AVS", FieldTypes.L_AN))
        fdefs.Add(60, New FieldDef(3, "Additional POS Information", FieldTypes.L_BCD))
        fdefs.Add(70, New FieldDef(2, "Network Management Information Code"))
        fdefs.Add(90, New FieldDef(21, "Original Data Elements", , False, True))
        fdefs.Add(95, New FieldDef(42, "Replacement Amounts", FieldTypes.AN, False, True))

        fdefs.Add(123, New FieldDef(&H66, "Address Verification Data", FieldTypes.TLV))
        fdefs.Add(123192, New FieldDef(9, "PostalCode", FieldTypes.L_AN))
        fdefs.Add(123207, New FieldDef(40, "Street Address", FieldTypes.L_AN))

        fdefs.Add(126, New FieldDef(8, "Field 126 Subfields", FieldTypes.BITMAP))

        'Field 126 subfields
        fdefs.Add(12606, New FieldDef(17, "Cardholder Certificate Serial Number"))
        fdefs.Add(12607, New FieldDef(17, "Merchant Certificate Serial Number"))
        fdefs.Add(12608, New FieldDef(20, "Transaction ID (XID)", FieldTypes.BINARY))
        fdefs.Add(12609, New FieldDef(20, "Transaction Stain/CAVV Data", FieldTypes.BINARY))
        fdefs.Add(12610, New FieldDef(6, "CVV2 Authorization Request Data", FieldTypes.AN))

        fdefs.Add(12611, New FieldDef(2, "IGOTS Transaction Description", FieldTypes.AN))
        fdefs.Add(12612, New FieldDef(3, "Service Indicators"))
        fdefs.Add(12613, New FieldDef(1, "POS Environment", FieldTypes.AN))
        fdefs.Add(12614, New FieldDef(1, "Payment Guarantee Option Indicator", FieldTypes.AN))
        fdefs.Add(12615, New FieldDef(1, "MasterCard UCAF Collection Indicator", FieldTypes.AN))
        fdefs.Add(12616, New FieldDef(33, "MasterCard UCAF Field", FieldTypes.BINARY))





        'Non-ISO fields (extended message)
        fdefs.Add(80, New FieldDef(1, "Extended message indicator", FieldTypes.AN, , , True))
        fdefs.Add(81, New FieldDef(20, "Host Response Code", FieldTypes.L_AN, , , True))
        fdefs.Add(82, New FieldDef(255, "Host Response Text", FieldTypes.L_AN, , , True))
        fdefs.Add(83, New FieldDef(10, "Sentry isotrxnlog serno", FieldTypes.L_BCD, , , True))
        fdefs.Add(84, New FieldDef(25, "Host Transaction Id", FieldTypes.L_AN, , , True))
        fdefs.Add(85, New FieldDef(12, "Host Retrieval Reference Number", FieldTypes.AN, , , True))
        fdefs.Add(86, New FieldDef(1, "CardType", FieldTypes.AN, , , True))
        fdefs.Add(87, New FieldDef(255, "CustomData", FieldTypes.L_AN, , , True))
        fdefs.Add(88, New FieldDef(150, "MerchantOrderId", FieldTypes.L_AN, , , True))
        fdefs.Add(89, New FieldDef(19, "TransactionCode", FieldTypes.L_AN, , , True))


        Return fdefs

    End Function


    Public Enum JustificationTypes
        Left = 1
        Right = 2
    End Enum

 


End Class

