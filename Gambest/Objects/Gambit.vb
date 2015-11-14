Namespace Objects

    Public Class Gambit
        Public triggers As New List(Of Trigger)
        Public triggerGate As String
        Public NotGate As Boolean = False
        Public reaction As Reaction

        Public Sub New(ByVal LogicString As String)
            If LogicString.ToLower.Contains("and") Then
                triggerGate = "AND"
            ElseIf LogicString.ToLower.Contains("or") Then
                triggerGate = "OR"
            End If

            If LogicString.ToLower.Contains("not") Then NotGate = True
        End Sub
    End Class

End Namespace
