using System;

public class TestDialog1 : GDialog
{
    bool testError = false;

    public TestDialog1 (GDialogFunction[] commands) : base(commands)
    {

    }

    protected override string Start(string[] args)
    {
        SetCharactersPerSecond(2);

        if (testError) return "LOLLOLOLOLO WORKING /n hell yes!";
        
        return "LOLLOLOLOLO WORKING \n hell yes!";
    }
}