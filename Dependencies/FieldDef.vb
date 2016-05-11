Imports System.Text

Partial Public Class DHIISOMessageDefinition

    Public Class FieldDef
        Public Property Length As Integer
        Public Property Name As String
        Public Property FieldType As FieldTypes
        Public Property RequiredIn110 As Boolean
        Public Property RequiredIn410 As Boolean
        Public Property Extended As Boolean = False
        Private _custompadding As Char

        'Fields added 2014 to support FDMS ISO8583 and other hosts
        Public Property SubFieldCount As Integer
        Public Property Justification As JustificationTypes
        Public Property PaddingCharacter As Char
        Public Property CharacterEncoding As Encoding

        'Conversion
        'Public CustomFieldConverter As IIso8583Conversion = Nothing

        Public Function ConvertToAlphanumeric(bytes As Byte()) As String
            Return CharacterEncoding.GetChars(bytes)
        End Function

        Public Function ConvertToBytes(sinput As String) As Byte()
            Return CharacterEncoding.GetBytes(sinput)
        End Function


        Public Function GetPaddedString(input As String, Optional totalWidth As Integer = 0) As String
            GetPaddedString = Nothing

            If input Is Nothing Then Return Nothing
            If totalWidth = 0 Then totalWidth = Length

            input = input.Trim

            Select Case Justification
                Case JustificationTypes.Left
                    GetPaddedString = input.PadRight(totalWidth, PaddingCharacter)
                Case JustificationTypes.Right
                    GetPaddedString = input.PadLeft(totalWidth, PaddingCharacter)
            End Select
        End Function

        Public Function GetUnpaddedString(input As String, Optional unpaddedWidth As Integer = 0) As String
            GetUnpaddedString = Nothing

            If input Is Nothing Then Return Nothing
            If unpaddedWidth = 0 Then unpaddedWidth = Length

            If input.Length <= unpaddedWidth Then Return input

            Select Case Justification
                Case JustificationTypes.Left
                    GetUnpaddedString = Left(input, unpaddedWidth)
                Case JustificationTypes.Right
                    GetUnpaddedString = Right(input, unpaddedWidth)
            End Select
        End Function

        Public Sub New(ByVal length_ As Integer, ByVal name_ As String, Optional ByVal fieldtype_ As FieldTypes = FieldTypes.BCD, Optional ByVal requiredIn110_ As Boolean = False, Optional ByVal requiredIn410_ As Boolean = False, Optional ByVal extended_ As Boolean = False)
            Length = length_
            Name = name_
            FieldType = fieldtype_
            RequiredIn110 = requiredIn110_
            RequiredIn410 = requiredIn410_
            Extended = extended_


            'Setup default justification and padding characters
            Select Case fieldtype_
                Case FieldTypes.BCD, FieldTypes.LBCD_BCD, FieldTypes.LLBCD_BCD, FieldTypes.LN_BCD, FieldTypes.L_BCD
                    Justification = JustificationTypes.Right
                    PaddingCharacter = "0"

                Case FieldTypes.AN, FieldTypes.LBCD_AN, FieldTypes.LLBCD_AN, FieldTypes.LLLAN_AN, FieldTypes.LLAN_AN, FieldTypes.L_AN
                    Justification = JustificationTypes.Left
                    PaddingCharacter = " "
            End Select

            CharacterEncoding = Encoding.GetEncoding(500) 'Default encoding is IBM EBCDIC (International)

        End Sub
    End Class
End Class
