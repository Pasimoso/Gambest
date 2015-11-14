Imports Gambest.Objects
Imports FFACETools

Namespace Commands
    ''' <summary>
    ''' 
    ''' </summary>
    Public Class MonitorObject
        Private _player As Character
        Private _delay As Integer
        Private _partyCount As Integer

        Public Sub New(ByVal player As Character, ByVal delay As Integer, ByVal partyCount As Integer)
            _player = player
            _delay = delay
            _partyCount = partyCount
        End Sub

        ''' <summary>
        ''' Main loop for monitoring the player and reacting to its environment. 
        ''' </summary>
        Public Sub Monitor()
            If _player.Leader Then _player.INSTANCE.Windower.SendString("/echo Commencing Gambit, assigning " + _player.Name + " as the leader.")
            _player.INSTANCE.Windower.SendString("/lockstyle on")
            _player.INSTANCE.Navigator.HeadingTolerance = 10 'Setting the threshhold in degrees.
            While True
                If _player.INSTANCE.Player.Status = Status.Fighting Then
                    _player.INSTANCE.Navigator.FaceHeading(_player.INSTANCE.Target.ID)
                End If

                'for each gambit
                For Each Gambit As Gambit In _player.gambits
                    Dim check As Boolean = True
                    Dim target As String = "<me>"

                    If Gambit.triggerGate = "AND" Then
                        For Each Trigger As Trigger In Gambit.triggers
                            target = Trigger.Target
                            If Not checkTrigger(Trigger, _player.INSTANCE, target, _partyCount) Then
                                check = False
                                Exit For
                            End If
                        Next
                    Else
                        check = False
                        For Each Trigger As Trigger In Gambit.triggers
                            target = Trigger.Target
                            If checkTrigger(Trigger, _player.INSTANCE, target, _partyCount) Then
                                check = True
                            End If
                        Next
                    End If

                    If Gambit.NotGate Then check = Not check

                    If check Then
                        If React(Gambit.reaction, _player.INSTANCE, target) Then
                            Threading.Thread.Sleep(_delay) 'prevents sending command multiple times
                            Exit For
                        End If
                    End If
                Next
                Threading.Thread.Sleep(_delay)
            End While
        End Sub



        Public Function checkTrigger(ByVal trigger As Trigger, ByVal INSTANCE As FFACE, ByRef target As String, ByVal partycount As Integer) As Boolean
            Dim VAL As Object = ""
            Select Case trigger.Target
                Case Targets.party
                    For i = 0 To partycount - 1
                        Select Case trigger.Type
                            Case Triggers.HP
                                VAL = INSTANCE.PartyMember(i).HPCurrent
                            Case Triggers.HPP
                                VAL = INSTANCE.PartyMember(i).HPPCurrent
                            Case Triggers.MP
                                VAL = INSTANCE.PartyMember(i).MPCurrent
                            Case Triggers.MPP
                                VAL = INSTANCE.PartyMember(i).MPPCurrent
                        End Select
                        Select Case trigger.Gate
                            Case Gates.Equals
                                If VAL = trigger.Arg Then
                                    target = INSTANCE.PartyMember(i).Name
                                    Return True
                                End If
                            Case Gates.GreaterThan
                                If VAL > trigger.Arg Then
                                    target = INSTANCE.PartyMember(i).Name
                                    Return True
                                End If
                            Case Gates.LessThan
                                If VAL < trigger.Arg Then
                                    target = INSTANCE.PartyMember(i).Name
                                    Return True
                                End If
                            Case Gates.NotEquals
                                If VAL <> trigger.Arg Then
                                    target = INSTANCE.PartyMember(i).Name
                                    Return True
                                End If
                        End Select
                    Next
                    Return False
                Case Targets.self
                    target = "<me>"
                    Select Case trigger.Type
                        Case Triggers.HP
                            VAL = INSTANCE.Player.HPCurrent
                        Case Triggers.HPP
                            VAL = INSTANCE.Player.HPPCurrent
                        Case Triggers.MP
                            VAL = INSTANCE.Player.MPCurrent
                        Case Triggers.MPP
                            VAL = INSTANCE.Player.MPPCurrent
                    End Select
                Case Targets.target
                    target = "<t>"
                    Select Case trigger.Type
                        Case Triggers.HPP
                            VAL = INSTANCE.NPC.HPPCurrent(INSTANCE.Target.ID)
                    End Select
            End Select
            Select Case trigger.Type
                Case Triggers.ASSIST
                    target = "<t>"
                    Dim name1 As String = INSTANCE.Target.Name
                    INSTANCE.Windower.SendString("/assist " + trigger.Arg)
                    Threading.Thread.Sleep(1000)
                    If Not name1 = INSTANCE.Target.Name Then
                        Return True
                    End If
                Case Triggers.DISTANCE
                    target = "<t>"
                    VAL = INSTANCE.NPC.Distance(INSTANCE.Target.ID)
                Case Triggers.TP
                    target = "<me>"
                    VAL = INSTANCE.Player.TPCurrent
                Case Triggers.NAME
                    target = "<t>"
                    Dim tName As String = INSTANCE.Target.Name.ToLower
                    If tName.Contains(trigger.Arg.ToLower) Then
                        Return True
                    Else
                        Return False
                    End If

                Case Triggers.STATUS
                    target = "<me>"
                    If INSTANCE.Player.Status = [Enum].Parse(GetType(Status), trigger.Arg) Then Return True
                Case Triggers.EFFECT
                    target = "<me>"
                    If Gates.NotEquals Then
                        For Each Stat As StatusEffect In INSTANCE.Player.StatusEffects()
                            If Stat = [Enum].Parse(GetType(StatusEffect), trigger.Arg) Then Return False
                        Next
                        Return True
                    Else
                        For Each Stat As StatusEffect In INSTANCE.Player.StatusEffects()
                            If Stat = [Enum].Parse(GetType(StatusEffect), trigger.Arg) Then Return True
                        Next
                        Return False
                    End If


            End Select

            Select Case trigger.Gate
                Case Gates.Equals
                    If VAL = trigger.Arg Then Return True
                Case Gates.GreaterThan
                    If VAL > trigger.Arg Then Return True
                Case Gates.LessThan
                    If VAL < trigger.Arg Then Return True
                Case Gates.NotEquals
                    If VAL <> trigger.Arg Then Return True
            End Select
            Return False
        End Function

        Public Function React(ByVal reaction As Reaction, ByVal INSTANCE As FFACE, ByVal target As String) As Boolean
            Select Case reaction.Type
                Case Reactions.ATTACK
                    INSTANCE.Windower.SendString("/attack")
                    Threading.Thread.Sleep(1000)
                Case Reactions.INPUT
                    INSTANCE.Windower.SendString(reaction.Arg)
                Case Reactions.JOB_ABILITY
                    Dim fixedstring As String = ConvertAbility(reaction.Arg)
                    Dim ID As AbilityList = [Enum].Parse(GetType(AbilityList), fixedstring)
                    'If Not INSTANCE.Player.HasAbility(ID) Then
                    'INSTANCE.Windower.SendString("/echo You do not have ability:" + reaction.Arg)
                    'End
                    'End If
                    If INSTANCE.Timer.GetAbilityRecast(ID) > 0 Then
                        Return False
                    End If

                    INSTANCE.Windower.SendString("/ja """ + reaction.Arg + """ " + target)

                Case Reactions.SPELL
                    Dim fixedstring As String = ConvertSpell(reaction.Arg)
                    Dim ID As SpellList = [Enum].Parse(GetType(SpellList), fixedstring)
                    'If Not INSTANCE.Player.KnowsSpell(ID) Then
                    'INSTANCE.Windower.SendString("/echo You do not know the spell:" + reaction.Arg)
                    'End
                    'End If
                    If INSTANCE.Timer.GetSpellRecast(ID) > 0 Then
                        Return False
                    End If

                    INSTANCE.Windower.SendString("/ma """ + reaction.Arg + """ " + target)
                Case Reactions.WEAPONSKILL
                    If Not INSTANCE.Player.TPCurrent > 1000 Then
                        Return False
                    End If
                    INSTANCE.Windower.SendString("/ws """ + reaction.Arg + """ <t>")
                Case Reactions.KEYDOWN
                    Dim ID As Byte = [Enum].Parse(GetType(KeyCode), reaction.Arg)
                    INSTANCE.Windower.SendKey(ID, True)
                Case Reactions.KEYUP
                    Dim ID As Byte = [Enum].Parse(GetType(KeyCode), reaction.Arg)
                    INSTANCE.Windower.SendKey(ID, False)
                Case Reactions.KEYPRESS
                    Dim ID As Byte = [Enum].Parse(GetType(KeyCode), reaction.Arg)
                    INSTANCE.Windower.SendKeyPress(ID)
                Case Reactions.MACRO
                    INSTANCE.Windower.SendString("/echo Macros are not enabled yet, y u do dis?")
                Case Reactions.TRACK
                    INSTANCE.Windower.SendString("/lockon on")
                    While INSTANCE.NPC.Distance(INSTANCE.Target.ID) > reaction.Arg
                        If INSTANCE.Player.ViewMode = ViewMode.ThirdPerson Then
                            INSTANCE.Windower.SendKeyPress(KeyCode.LetterV)
                        End If
                        INSTANCE.Windower.SendKey(KeyCode.LetterW, True)
                    End While
                    INSTANCE.Windower.SendKey(KeyCode.LetterW, False)
                    INSTANCE.Windower.SendString("/lockon off")
                Case Reactions.FIND
                    For i = 0 To 20
                        For n = 0 To 8
                            If INSTANCE.Player.ViewMode = ViewMode.ThirdPerson Then
                                INSTANCE.Windower.SendKeyPress(KeyCode.LetterV)
                            End If
                            INSTANCE.Windower.SendKeyPress(KeyCode.BackspaceKey)
                            Threading.Thread.Sleep(100)
                            INSTANCE.Windower.SendKeyPress(KeyCode.TabKey)
                            Threading.Thread.Sleep(200)
                            If INSTANCE.Target.Name.ToLower.Contains(reaction.Arg.ToLower) Then
                                Return True
                            End If
                        Next
                        For n = 0 To 8
                            INSTANCE.Windower.SendKeyPress(KeyCode.LetterD)
                            Threading.Thread.Sleep(50)
                        Next
                    Next
                    Threading.Thread.Sleep(500)
                    Return False
            End Select

            Return True
        End Function

        Function ConvertAbility(ByVal raw As String) As String
            Dim out As String = raw.Replace("_", " ")
            out = out.Replace("'", "")
            out = out.Replace(" ", "_")
            If out.Contains("Curing_Waltz_") Then
                out = "Curing_Waltz"
            ElseIf out.ToLower.Contains("step") Then
                Return "Steps"
            ElseIf out.Contains("Divine_Waltz_") Then
                Return "Divine_Waltz"
            ElseIf out.Contains("Samba") Then
                Return "Sambas"
            ElseIf out.Contains("Jig") Then
                Return "Jigs"
            ElseIf out = "Animated_Flourish" Or out = "Desperate_Flourish" Or out = "Violent_Flourish" Then
                Return "Flourishes_I"
            ElseIf out = "Reverse Flourish" Or out = "Wild_Flourish" Or out = "Building_Flourish" Then
                Return "Flourishes_II"
            ElseIf out = "Climactic_Flourish" Or out = "Striking_Flourish" Or out = "Ternary_Flourish" Then
                Return "Flourishes_III"
            End If
            Return out
        End Function

        Function ConvertSpell(ByVal raw As String) As String
            Dim out As String = raw

            out = out.Replace(":", "")
            out = out.Replace("-", "")

            out = out.Replace("'", "")
            out = out.Replace("Winds of ", "Winds_")

            out = out.Replace(" ", "_")

            Return out
        End Function

    End Class

End Namespace
