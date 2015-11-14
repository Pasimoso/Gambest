Namespace Objects

    Public Class Character
        Public Leader As Boolean = True
        Public gambits As New List(Of Gambit)
        Public Name As String = ""
        Public INSTANCE As FFACETools.FFACE = Nothing

        Public Sub New(ByVal _Name As String, ByVal _Leader As Boolean)
            Leader = _Leader
            Name = _Name
        End Sub
    End Class

End Namespace