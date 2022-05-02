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

    int DialogDelayMS = 1000;

    int LastDialogMS = 0;

    int WaitTime = 0;

    bool DoNotTypeNextCharacter = false;

    int SkipValue = 0;

    bool ResetDialogAfterCompletion = true;

    List<GDialogFunction> Commands = new List<GDialogFunction>();

    protected bool SkippingDialog = false;

    public GDialog()
    {
        stopwatch = new Stopwatch();

        ResetDialog();
        stopwatch = new Stopwatch();
        InitDefaultCommands();
    }
    
    ///<summary>
    ///Reinitializes dialog if you want to reuse this object
    ///in the future. 
    ///Already happens altomatically when
    ///the dialog ends before OnEndEntireDialog.
    ///If you want to disable that you can call
    ///SetResetDialog(false);
    ///</summary>
    public void ResetDialog ()
    {
        NextDialog = Start;
        if (stopwatch.IsRunning) stopwatch.Stop();
        if (stopwatch.ElapsedTicks>0) stopwatch.Reset();

        SkipValue = 0;
        DoNotTypeNextCharacter = false;
        WaitTime = 0;
        DialogDelayMS = 1000;
        LastDialogMS = 0;
        DialogRaw = "";
        DialogPieced = "";
        DialogIndex = 0;
    }

    ///<summary>
    ///Sets wether or not the dialog should reinitialize
    ///recreate itself after it's completion.
    ///</summary>
    public void SetResetDialog(bool doIt)
    {
        ResetDialogAfterCompletion = doIt;
    }

    private void InitDefaultCommands ()
    {
        Commands.Clear();
        Commands.Add(new GDialogFunction("Wait", Wait));
        Commands.Add(new GDialogFunction("SetSpeed", CharPerSecond));
        Commands.Add(new GDialogFunction("EndDialog", End));
        Commands.Add(new GDialogFunction("SkipCharacters", Skip));
    }

    public void AddCommands(GDialogFunction[] _commands)
    {
        Commands.AddRange(_commands);
    }

    public void ResetDialogCommands() => InitDefaultCommands();

    ///<summary>
    ///Sets dialog speed in a different way, it sets the amount
    ///of milliseconds the system waits to type a character.
    ///</summary>
    protected void SetDialogDelayMS (int dialogDelayMS) => DialogDelayMS = dialogDelayMS;

    protected void SetCharactersPerSecond(int speed) => SetDialogDelayMS(1000/speed);

    ///<summary>
    ///Sets the number of characters per second the system types.
    ///If this isn't set at least one time, the system types 1
    ///character per second, wich is very slow.
    ///</summary>
    protected void SetSpeed (int speed) => SetCharactersPerSecond(speed);

    ///<summary>
    ///This function must be called only when you advance
    ///the dialog, example: the player is reading a texbox
    ///and now he presses a button to go to the next textbox, this is
    ///when this function should be called.
    ///It calls the next dialog function with the string array
    ///you can provide as argument here.
    ///When this function is first called, it starts the dialog
    ///by calling the 'Start' function, for more information see the description
    ///of 'Start'.
    ///</summary>
    public void ChangeDialog() => ChangeDialog(new string[0]);

    ///<summary>
    ///This function must be called only when you advance
    ///the dialog, example: the player is reading a texbox
    ///and now he presses a button to go to the next textbox, this is
    ///when this function should be called.
    ///It calls the next dialog function with the string array
    ///you can provide as argument here.
    ///When this function is first called, it starts the dialog
    ///by calling the 'Start' function, for more information see the description
    ///of 'Start'.
    ///</summary>
    public void ChangeDialog(string[] args)
    {
        LastDialogMS = 0;

        DialogIndex = 0;
        DialogRaw= "";
        DialogPieced = "";

        SkippingDialog = false;
        
        if (stopwatch.ElapsedTicks == 0) stopwatch.Start();
        else stopwatch.Restart();

        if (NextDialog == null)
        {
            if (ResetDialogAfterCompletion) ResetDialog();
            if (OnEndEntireDialog != null) OnEndEntireDialog.Invoke();
            return;
        }

        var nd = NextDialog;

        NextDialog = null;

        DialogRaw = nd.Invoke(args);
    }

    ///Event that's called when the system
    ///fully typed a dialog. Is disposed
    ///when the dialog is disposed.
    ///See Dispose method description for more
    ///information.
    public event Action<GDialog>? OnEndWrite;

    ///<summary>
    ///Called when the dialog ends, very useful.
    ///</summary>
    public event Action? OnEndEntireDialog;

    ///<summary>
    ///Sets the next dialog that should be executed
    ///after calling ChangeDialog, if this function
    ///isn't executed in the current dialog function
    ///the dialog just ends. See: OnEndEntireDialog
    ///</summary>
    protected void SetNextDialog(GStringString dialog) => NextDialog = dialog;

    ///<summary>
    ///Skips dialog at a speed (characters per second)
    ///if you provide a int as argument,
    ///else it's just instantaneous.
    ///While the dialog is being skipped a bool called
    ///'SkippingDialog' is set to true, use this
    ///information to correct potential bugs!
    ///</summary>
    public void SkipDialog(int speed)
    {
        SetSpeed(speed);

        SkippingDialog = true;
    }

    public void SkipDialog()
    {
        SkippingDialog = true;
        
        bool l = true;

        while (l)
        {
            l = Piece();
        }

        SkippingDialog = false;
    }

    ///<summary>
    //If this object is the current dialog
    ///This function should be executed every frame,
    ///and it's result should be the texbox's text.
    ///It returns the dialog in it's current form,
    ///being typed and everything, it also handles
    ///everything in this system.
    ///If the current dialog is fully typed it calls
    ///OnEndWrite, for more information read it's description.
    ///</summary>
    public string RetrieveDialog ()
    {

        if (DialogIndex >= DialogRaw.Length)
        {
            if (DialogIndex != DialogRaw.Length + 5)
            {
                if(OnEndWrite != null) OnEndWrite.Invoke(this);

                DialogIndex = DialogRaw.Length + 5;
            }

            return DialogPieced;
        }

        long elapsed = stopwatch.ElapsedMilliseconds - LastDialogMS;

        if (WaitTime > 0)
        {
            if(elapsed > WaitTime)
            {
                LastDialogMS += WaitTime;

                elapsed -= WaitTime;

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

                if(SkipValue!=0)
                {
                    LastDialogMS-= DialogDelayMS * SkipValue;

                    elapsed += DialogDelayMS * SkipValue;

                    SkipValue = 0;
                }
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

        if (DoNotTypeNextCharacter)
        {
            DoNotTypeNextCharacter = false;
            return true;
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

        throw new Exception($"Command called \"{command}\" doesn't exist!");
    }

    ///<summary>
    ///First dialog function called in ChangeDialog
    ///Dialog functions return the dialog that should
    ///be typed by the system in conjunction with executing
    ///any code you would like before that, so you can make
    ///systems of multiple choices etc.
    ///SetNextDialog and SetSpeed should be called inside dialog functions,
    ///see their description to know more.
    ///ALSO: see dialog commands in the DialogCommands text file.
    ///</summary>
    protected virtual string Start(string[] args)
    {
        return "";
    }

    bool disposed = false;
    
    ///<summary>
    ///Should be called when this object should be deleted,
    ///it cleans all events so no memory leaks occur.
    ///</summary>
    public void Dispose()
    {
        if(disposed) return;

        Commands.Clear();

        OnEndWrite = null;

        OnEndEntireDialog = null;

        NextDialog = null;

        stopwatch.Stop();

        disposed = true;

        GC.SuppressFinalize(this);
    }

    private string Wait (string[] args)
    {
        if(SkippingDialog) return "";

        WaitTime = int.Parse(args[0]);
        DoNotTypeNextCharacter = true;
        return "";
    }

    private string CharPerSecond(string[] args)
    {
        if(SkippingDialog) return "";

        SetCharactersPerSecond(int.Parse(args[0]));
        return "";
    }

    private string End(string[] args)
    {
        ChangeDialog(new string[0]);

        return "";
    }

    private string Skip(string[] args)
    {
        SkipValue = int.Parse(args[0]);
        DoNotTypeNextCharacter=true;
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