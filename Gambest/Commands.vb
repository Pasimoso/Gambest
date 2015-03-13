﻿Imports Gambest.Objects
Imports System.Xml
Imports FFACETools.FFACETools
Module Commands
    Public Sub OutError(ByVal Line As Integer, ByVal Message As String, ByVal Kill As Boolean)
        Dim Outmessage As String = "Error at line #" + Line.ToString() + vbNewLine _
        + Message + vbNewLine _
        + "Press any button to "
        If Kill Then
            Outmessage = Outmessage + "end the program."
        Else
            Outmessage = Outmessage + "continue."
        End If
        Console.Write(Outmessage)
        Console.ReadKey()
        Console.WriteLine("")
        If Kill Then End
    End Sub

    Public Function Parse(ByVal filepath As String, ByRef delay As Integer) As List(Of character)

        Dim outlist As New List(Of character)




        Dim reader As New XmlDocument()
        reader.Load(filepath)
        ''primed

        Dim SettingNode As XmlNode = reader.SelectSingleNode("/ROOT/SETTINGS")
        delay = SettingNode.SelectSingleNode("./DELAY").FirstChild.Value

        For Each player As XmlNode In reader.SelectNodes("/ROOT/PLAYER")
            Dim name = player.Attributes("name").Value.Trim
            Dim leader As Boolean = player.Attributes("leader").Value.Trim
            Dim character As New character(name, leader)

            For Each GambitNode As XmlNode In player.SelectNodes("./GAMBIT")
                Dim triggerNode As XmlNode = GambitNode.SelectSingleNode("./TRIGGER")
                Dim logicstring As String = triggerNode.Attributes("gate").Value

                Dim Gambit As New Gambit(logicstring)

                For Each Node As XmlNode In triggerNode.ChildNodes
                    Dim target As String = Node.Name.Trim
                    Dim ttype As String = Node.Attributes("type").Value.Trim
                    Dim gate As String = Node.Attributes("gate").Value.Trim
                    Dim targ As String = Node.Attributes("arg").Value.Trim
                    Dim trigger As New Trigger(target, gate, ttype, targ)

                    Gambit.triggers.Add(trigger)
                Next

                Dim ReactionNode As XmlNode = GambitNode.SelectSingleNode("./REACTION").FirstChild
                Dim type As String = ReactionNode.Name.Trim
                Dim arg As String = ReactionNode.FirstChild.Value.Trim
                Gambit.reaction = New Reaction(type, arg)

                character.gambits.Add(Gambit)
            Next
            outlist.Add(character)
        Next

        Return outlist
    End Function

    Public Sub Monitor(ByVal player As character, ByVal delay As Integer, ByVal partycount As Integer)
        While True
            Threading.Thread.Sleep(delay)

            'for each gambit
            For Each Gambit As Gambit In player.gambits
                Dim check As Boolean = True
                Dim target As String = "<me>"

                If Gambit.triggerGate = "AND" Then
                    For Each Trigger As Trigger In Gambit.triggers
                        target = Trigger.Target
                        If Not checkTrigger(Trigger, player.INSTANCE, target, partycount) Then
                            check = False
                            Exit For
                        End If
                    Next
                Else
                    check = False
                    For Each Trigger As Trigger In Gambit.triggers
                        target = Trigger.Target
                        If checkTrigger(Trigger, player.INSTANCE, target, partycount) Then
                            check = True
                        End If
                    Next
                End If

                If Gambit.NotGate Then check = Not check

                If check Then
                    If React(Gambit.reaction, player.INSTANCE, target) Then
                        Exit For
                    End If
                End If
            Next


            'get gate

            'check each trigger and compile gate

            'attempt reaction

        End While
    End Sub

    Public Function checkTrigger(ByVal trigger As Trigger, ByVal INSTANCE As FFACE, ByRef target As String, ByVal partycount As Integer) As Boolean
        Dim VAL As Object = ""
        Select Case trigger.Target
            Case Targets.party
                For i = 1 To partycount - 1
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
            Case Triggers.DISTANCE
                target = "<t>"
                VAL = INSTANCE.NPC.Distance(INSTANCE.Target.ID)
            Case Triggers.TP
                target = "<me>"
                VAL = INSTANCE.Player.TPCurrent
            Case Triggers.NAME
                target = "<t>"
                VAL = INSTANCE.Target.Name
            Case Triggers.STATUS
                target = "<me>"
                If INSTANCE.Player.Status = [Enum].Parse(GetType(Status), trigger.Arg) Then Return True
            Case Triggers.EFFECT
                target = "<me>"
                For Each Stat As StatusEffect In INSTANCE.Player.StatusEffects()
                    If Stat = [Enum].Parse(GetType(StatusEffect), trigger.Arg) Then Return True
                Next
        End Select

        Select Case trigger.Gate
            Case Gates.Equals
                If VAL = trigger.Arg Then Return True
            Case Gates.GreaterThan
                If VAL > trigger.Arg Then Return True
            Case Gates.LessThan
                If VAL < trigger.Arg Then Return True
        End Select
        Return False
    End Function

    Public Function React(ByVal reaction As Reaction, ByVal INSTANCE As FFACE, ByVal target As String) As Boolean
        Select Case reaction.Type
            Case Reactions.ATTACK
                INSTANCE.Windower.SendString("/attack")
            Case Reactions.INPUT
                INSTANCE.Windower.SendString(reaction.Arg)
            Case Reactions.JOB_ABILITY
                Dim ID As Byte = [Enum].Parse(GetType(AbilityList), reaction.Arg)
                'If Not INSTANCE.Player.HasAbility(ID) Then
                'INSTANCE.Windower.SendString("/echo You do not have ability:" + reaction.Arg)
                'End
                'End If
                If INSTANCE.Timer.GetAbilityRecast(ID) > 0 Then
                    Return False
                End If

                Dim fixedstring As String = convertAbility(reaction.Arg)
                INSTANCE.Windower.SendString("/ja """ + fixedstring + """ " + target)

            Case Reactions.SPELL
                Dim ID As Byte = [Enum].Parse(GetType(SpellList), reaction.Arg)
                'If Not INSTANCE.Player.KnowsSpell(ID) Then
                'INSTANCE.Windower.SendString("/echo You do not know the spell:" + reaction.Arg)
                'End
                'End If
                If INSTANCE.Timer.GetSpellRecast(ID) > 0 Then
                    Return False
                End If

                Dim fixedstring As String = convertSpell(reaction.Arg)
                INSTANCE.Windower.SendString("/ma """ + fixedstring + """ " + target)
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
        End Select

        Return True
    End Function

    Function convertAbility(ByVal raw As String) As String
        Dim out As String = raw.Replace("_", " ")
        out.Replace("Avatars", "Avatar's")
        out.Replace("Assasins", "Assasin's")
        Return out
    End Function

    Function convertSpell(ByVal raw As String) As String
        Dim out As String = raw

        out.Replace("_Ichi", " :Ichi")
        out.Replace("_Ni", " :Ni")
        out.Replace("_San", " :San")

        out.Replace("Teleport_", "Teleport: ")
        out.Replace("Recall_", "Recall-")

        out.Replace("Adventurers", "Adventurer's")
        out.Replace("Archers", "Archer's")
        out.Replace("Armys", "Army's")
        out.Replace("Everyones", "Everyone's")
        out.Replace("Goddesss", "Goddess's")
        out.Replace("Hunters", "Hunter's")
        out.Replace("s_Operetta", "'s Operetta")
        out.Replace("Knights", "Knight's")
        out.Replace("Mages", "Mage's")
        out.Replace("Maidens", "Maiden's")
        out.Replace("Sentinels", "Sentinel's")
        out.Replace("Winds_", "Winds of ")

        out.Replace("_", " ")

        Return out
    End Function

End Module
