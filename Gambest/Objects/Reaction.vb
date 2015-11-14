Namespace Objects

    Public Class Reaction
        Public Type As Reactions
        Public Arg As String = ""

        Public Sub New(ByVal _Type As String, ByVal _argument As String)
            Arg = _argument
            Select Case _Type.ToLower
                Case "attack" : Type = Reactions.ATTACK
                Case "input" : Type = Reactions.INPUT
                Case "ability" : Type = Reactions.JOB_ABILITY
                Case "spell" : Type = Reactions.SPELL
                Case "weaponskill" : Type = Reactions.WEAPONSKILL
                Case "macro" : Type = Reactions.MACRO
                Case "keydown" : Type = Reactions.KEYDOWN
                Case "keypress" : Type = Reactions.KEYPRESS
                Case "keyup" : Type = Reactions.KEYUP
                Case "track" : Type = Reactions.TRACK
                Case "find" : Type = Reactions.FIND
            End Select
        End Sub
    End Class

End Namespace
