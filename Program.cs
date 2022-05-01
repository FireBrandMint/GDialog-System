using System;
using System.Threading;

public class Program
{

    static bool Read = false;

    static bool dialogEnded = false;

    public static void Main(String[] args)
    {
        TestDialog1 dialog = new TestDialog1();

        dialog.OnEndWrite += OnEndR;
        dialog.OnEndEntireDialog+= OnEndDialog;

        dialog.ChangeDialog();
        
        while(true)
        {
            if(dialogEnded)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("END OF DIALOG");
                    Console.Write("> ");
                    string? line = Console.ReadLine();
                    if (line == "end") break;
                }
                catch
                {

                }
                continue;
            }

            try
            {
                Console.Clear();

                Console.WriteLine(dialog.RetrieveDialog());

                if(Read)
                {
                    Console.Write("> ");
                    string? line = Console.ReadLine();

                    if(line == null)
                    dialog.ChangeDialog(new string[0]);
                    else dialog.ChangeDialog(new string[]{ line });
                    Read = false;
                }

                Thread.Sleep(16);
            }
            catch
            {

            }
        }
    }

    public static void OnEndR(GDialog dlog)
    {
        Read = true;
    }

    public static void OnEndDialog ()
    {
        dialogEnded = true;
    }
}