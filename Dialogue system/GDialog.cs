using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

public delegate string GStringString (string[] args);

public class GDialog: IDisposable
{
    GStringString? NextDialog;

    string DialogRaw = "";

    string DialogPieced = "";

    int DialogIndex = 0;

    Stopwatch stopwatch;

    int DialogDelayMS = 0;

    int LastDialogMS = 0;

    int WaitTime = 0;

    List<GDialogFunction> Commands = new List<GDialogFunction>(new GDialogFunction[]
    {
        
    });

    public GDialog (GDialogFunction[]? _commands)
    {
        stopwatch = new Stopwatch();

        NextDialog = Start;

        Commands.Add(new GDialogFunction("Wait", Wait));
        Commands.Add(new GDialogFunction("SetSpeed", CharPerSecond));
        Commands.Add(new GDialogFunction("EndDialog", End));

        if (_commands != null)
        {
            Commands.AddRange(_commands);
        }
    }

    protected void SetDialogDelayMS (int dialogDelayMS) => DialogDelayMS = dialogDelayMS;

    protected void SetCharactersPerSecond(int speed) => SetDialogDelayMS(1000/speed);

    public void ChangeDialog() => ChangeDialog(new string[0]);

    public void ChangeDialog(string[] args)
    {
        LastDialogMS = 0;
        
        if (stopwatch.ElapsedTicks == 0) stopwatch.Start();
        else stopwatch.Restart();

        if (NextDialog == null)
        {
            if (OnEndEntireDialog != null) OnEndEntireDialog.Invoke();
            return;
        }

        var nd = NextDialog;

        NextDialog = null;

        DialogRaw = nd.Invoke(args);
    }

    public event Action? OnEndEntireDialog;

    protected void SetNextDialog(GStringString dialog) => NextDialog = dialog;

    public string RetrieveDialog ()
    {

        if (DialogIndex >= DialogRaw.Length) return DialogPieced;

        long elapsed = stopwatch.ElapsedMilliseconds - LastDialogMS;

        if (WaitTime > 0)
        {
            if(elapsed > WaitTime)
            {
                LastDialogMS += WaitTime;

                WaitTime = 0;
            }
            else
            {
                return DialogPieced;
            }
        }

        if (elapsed >= DialogDelayMS)
        {
            while (elapsed  >= DialogDelayMS)
            {
                if (!Piece()) break;

                elapsed -= DialogDelayMS;

                LastDialogMS += DialogDelayMS;
            }
        }
        
        return DialogPieced;
    }

    private bool Piece()
    {
        if (DialogIndex >= DialogRaw.Length) return false;
        
        if(DialogRaw[DialogIndex] == '/')
        {
            while (DialogRaw[DialogIndex] == '/')
            {
                if(DialogIndex >= DialogRaw.Length) break;

                string command = "";

                int currArg = 0;
                List<string> args = new List<string>();
                bool ArgsExist = false;

                bool beforeParenthesis = true;
                int i = 0;
                for(i = DialogIndex + 1; i<DialogRaw.Length; ++i)
                {
                    char currChar = DialogRaw[i];

                    if (beforeParenthesis)
                    {
                        if (currChar == '/') break;

                        if (currChar == '(')
                        {
                            beforeParenthesis = false;
                            ArgsExist = true;
                            continue;
                        }

                        command += currChar;
                    }
                    else
                    {
                        if (currChar == ')')
                        {
                            ++i;
                            break;
                        }

                        if (currArg + 1 != args.Count) args.Add("");
                        if (currChar == ',')
                        {
                            currArg++;
                            continue;
                        }

                        args[currArg] += currChar;
                    }
                }

                DialogIndex = i;

                if (!ArgsExist) throw new Exception($"Command not spelled correctly on dialog {this.GetType().ToString()}!\n In the text \"{DialogRaw}\" exactly HERE: \"/{command}\"");
                if (command.Length != 0) DialogPieced += ExecuteCommand(command, args.ToArray());
            }
        }

        if (DialogIndex >= DialogRaw.Length) return false;

        DialogPieced += DialogRaw[DialogIndex];

        ++DialogIndex;

        return true;
    }

    private string ExecuteCommand(string command, string[] args)
    {
        for (int i = 0; i<Commands.Count; ++i)
        {
            if (Commands[i].Name == command)
            {
                return Commands[i].Func.Invoke(args);
            }
        }

        return "";
    }

    protected virtual string Start(string[] args)
    {
        return "";
    }
    
    public void Dispose()
    {
        Commands.Clear();

        OnEndEntireDialog = null;

        NextDialog = null;

        stopwatch.Stop();

        GC.SuppressFinalize(this);
    }

    private string Wait (string[] args)
    {
        WaitTime = int.Parse(args[0]);
        return "";
    }

    private string CharPerSecond(string[] args)
    {
        SetCharactersPerSecond(int.Parse(args[0]));
        return "";
    }

    private string End(string[] args)
    {
        ChangeDialog(new string[0]);

        return "";
    }
}

public class GDialogFunction 
{
    public string Name;
    public GStringString Func;

    public GDialogFunction (string funcName, GStringString function)
    {
        Name = funcName;
        Func = function;
    }
}