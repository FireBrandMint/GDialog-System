using System;
using System.Threading;

public class Program
{
    public static void Main(String[] args)
    {
        var dialog = new TestDialog1(new GDialogFunction[0]);

        dialog.ChangeDialog();
        
        while(true)
        {
            Console.WriteLine(dialog.RetrieveDialog());

            Thread.Sleep(16);
        }
    }
}