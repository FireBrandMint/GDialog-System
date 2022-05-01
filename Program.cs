using System;
using System.Threading;

public class Program
{

    public static bool Read = false;

    public static void Main(String[] args)
    {
        TestDialog1 dialog = new TestDialog1();

        dialog.OnEndWrite += OnEndR;

        dialog.ChangeDialog();
        
        while(true)
        {
            try
            {
                Console.Clear();

                Console.WriteLine(dialog.RetrieveDialog());

                if(Read)
                {
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
}