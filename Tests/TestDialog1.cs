using System;

public class TestDialog1 : GDialog
{
    bool testError = false;

    public TestDialog1 (GDialogFunction[] commands) : base(commands)
    {

    }

    protected override string Start(string[] args)
    {
        SetCharactersPerSecond(12);

        if (testError) return "LOLLOLOLOLO WORKING /n hell yes!";
        
        return "LOLLOLOLOLO WORKING \n/Wait(1000)hell /Wait(375)/SkipCharacters(4)yes!";
    }
}