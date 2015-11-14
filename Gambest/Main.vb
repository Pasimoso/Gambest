Imports FFACETools
Imports Gambest.Objects
Imports Gambest.Commands
Imports System.Xml

Module Main
    Private Delay As Integer = 0
    Private partycount As Integer = 0
    Sub Main()
        Dim path As String = My.Application.Info.DirectoryPath()

        Dim statelist As String = path + "\states.txt"
        Dim effectlist As String = path + "\effects.txt"
        Dim filepath As String = path + "\main.xml" ' path to default xml

        Dim statewriter As New System.IO.StreamWriter(statelist)
        statewriter.Write(String.Join(vbNewLine, (System.Enum.GetNames(GetType(FFACETools.Status)))))
        statewriter.Close() ' creates spelllist.txt reference file for user.

        Dim effectwriter As New System.IO.StreamWriter(effectlist)
        effectwriter.Write(String.Join(vbNewLine, (System.Enum.GetNames(GetType(StatusEffect)))))
        effectwriter.Close() ' creates spelllist.txt reference file for user.

        Dim processes As Process() = Process.GetProcessesByName("pol") 'find all open ffxi instances
        If processes.Length = 0 Then ' none exist
            OutError(0, "Please make sure ffxi is running and open.", True)
        End If


        Dim Args As String() = Environment.GetCommandLineArgs() 'see if user loaded a non-default xml

        If Args.Length > 1 Then  'User loaded a file, 
            filepath = Args(1)   'use that instead of main.xml
        End If

        If Not My.Computer.FileSystem.FileExists(filepath) Then 'File couldn't be found.
            OutError(0, "xml not found.", True)
        End If

        Dim CharList As List(Of Character) = Parse(filepath, Delay, partycount) 'parse the xml

        Dim threadlist As New List(Of System.Threading.Thread) ' time to make threads!

        For Each character As Character In CharList
            For Each ffxi As Process In processes
                If ffxi.MainWindowTitle = character.Name Then 'verify character exists in xml AND is logged on
                    Dim instance As New FFACE(ffxi.Id)
                    character.INSTANCE = instance 'bind their fface instance to their gambits
                    Dim charObj As MonitorObject = New MonitorObject(character, Delay, partycount) ' initialize them
                    threadlist.Add(New System.Threading.Thread(AddressOf charObj.Monitor)) 'add it to the stack
                End If
            Next
        Next

        If threadlist.ToArray.Length = 0 Then
            OutError(0, "No characters found, check charnames in xml.", True)
        End If

        For Each thread As System.Threading.Thread In threadlist
            thread.Start() 'commence running
        Next

    End Sub

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

    Public Function Parse(ByVal filepath As String, ByRef delay As Integer, ByRef partycount As Integer) As List(Of Character)

        Dim outlist As New List(Of Character)

        Dim reader As New XmlDocument()
        reader.Load(filepath)
        ''primed

        Dim SettingNode As XmlNode = reader.SelectSingleNode("/ROOT/SETTINGS")
        delay = SettingNode.SelectSingleNode("./DELAY").FirstChild.Value
        partycount = SettingNode.SelectSingleNode("./PARTYCOUNT").FirstChild.Value

        For Each player As XmlNode In reader.SelectNodes("/ROOT/PLAYER")
            Dim name = player.Attributes("name").Value.Trim
            Dim leader As Boolean = player.Attributes("leader").Value.Trim
            Dim character As New Character(name, leader)

            For Each GambitNode As XmlNode In player.SelectNodes("./GAMBIT")
                Dim triggerNode As XmlNode = GambitNode.SelectSingleNode("./TRIGGER")
                Dim logicstring As String = triggerNode.Attributes("gate").Value

                Dim Gambit As New Gambit(logicstring)

                For Each Node As XmlNode In triggerNode.ChildNodes
                    Dim target As String = Node.Name.Trim
                    Dim ttype As String = Node.Attributes("type").Value
                    Dim gate As String = Node.Attributes("gate").Value
                    Dim targ As String = Node.Attributes("arg").Value
                    Dim trigger As New Trigger(target, gate, ttype, targ)

                    Gambit.triggers.Add(trigger)
                Next

                Dim ReactionNode As XmlNode = GambitNode.SelectSingleNode("./REACTION")
                Dim type As String = ReactionNode.Attributes("type").Value
                Dim arg As String = ReactionNode.Attributes("arg").Value
                Gambit.reaction = New Reaction(type, arg)

                character.gambits.Add(Gambit)
            Next
            outlist.Add(character)
        Next

        Return outlist
    End Function

End Module
