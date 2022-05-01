using System;

public class TestDialog1 : GDialog
{
    bool testError = false;

    protected override string Start(string[] args)
    {
        SetCharactersPerSecond(12);

        if (testError) return "LOLLOLOLOLO WORKING /n hell yes!";

        SetNextDialog(Dialog1);
        
        return "LOLLOLOLOLO WORKING \n/Wait(1000)hell /Wait(375)/SkipCharacters(4)yes!";
    }

    string Dialog1(string[] args)
    {
        SetCharactersPerSecond(12);
        return "Yes! Changing dialog is working too!";
    }
}