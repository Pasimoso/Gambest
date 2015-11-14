Namespace Objects

    Public Class Trigger
        Public Gate As Gates
        Public Target As Targets
        Public Type As Triggers
        Public Arg As String = ""

        Public Sub New(ByVal _Target As String, ByVal _Gate As String, ByVal _Type As String, ByVal _arg As String)
            Arg = _arg

            Select Case _Gate.ToLower
                Case "less"
                    Gate = Gates.LessThan
                Case "greater"
                    Gate = Gates.GreaterThan
                Case "equals"
                    Gate = Gates.Equals
                Case "notequals"
                    Gate = Gates.NotEquals
            End Select

            Select Case _Target.ToLower
                Case "party"
                    Target = Targets.party
                Case "self"
                    Target = Targets.self
                Case "target"
                    Target = Targets.target
            End Select

            Select Case _Type.ToLower
                Case "distance"
                    Type = Triggers.DISTANCE
                Case "effect"
                    Type = Triggers.EFFECT
                Case "hp"
                    Type = Triggers.HP
                Case "hpp"
                    Type = Triggers.HPP
                Case "mp"
                    Type = Triggers.MP
                Case "mpp"
                    Type = Triggers.MPP
                Case "name"
                    Type = Triggers.NAME
                Case "tp"
                    Type = Triggers.TP
                Case "status"
                    Type = Triggers.STATUS
                Case "assist"
                    Type = Triggers.ASSIST
            End Select
        End Sub
    End Class

End Namespace
