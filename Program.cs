using System;
using System.Threading;

public class Program
{
    public static bool l = true;

    public static void Main(String[] args)
    {
        TestDialog1 dialog = new TestDialog1(new GDialogFunction[0]);

        dialog.ChangeDialog();
        
        while(true)
        {
            try
            {
                Console.Clear();
            }
            catch
            {

            }

            Console.WriteLine(dialog.RetrieveDialog());

            Thread.Sleep(16);
        }
    }
}