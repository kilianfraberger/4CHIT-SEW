using System;
using System.Threading;

namespace ParallelProgrammingExample
{
    class Program
    {
        // Semaphoren zur Synchronisation
        static SemaphoreSlim semCraneToA = new SemaphoreSlim(1, 1);     // Start: Kran darf zuerst
        static SemaphoreSlim semA = new SemaphoreSlim(0, 1);
        static SemaphoreSlim semCraneToB = new SemaphoreSlim(0, 1);
        static SemaphoreSlim semB = new SemaphoreSlim(0, 1);
        static SemaphoreSlim semCraneToStorage = new SemaphoreSlim(0, 1);

        static void Main(string[] args)
        {
            MachineA machineA = new MachineA();
            MachineB machineB = new MachineB();
            Crane crane = new Crane();

            Thread tA = new Thread(machineA.Run);
            Thread tB = new Thread(machineB.Run);
            Thread tCrane = new Thread(crane.Run);

            tA.Start();
            tB.Start();
            tCrane.Start();

            Console.ReadLine();
        }

        // --- Klassen -----------------------------------------------------

        public class MachineA
        {
            public void Run()
            {
                while (true)
                {
                    semA.Wait(); // wartet, bis der Kran das Werkstück gebracht hat
                    Process();
                    semCraneToB.Release(); // Kran darf zu B transportieren
                }
            }

            private void Process()
            {
                Thread.Sleep(100);
                Console.WriteLine("MachineA: finished work");
            }
        }

        public class MachineB
        {
            public void Run()
            {
                while (true)
                {
                    semB.Wait(); // wartet, bis der Kran von A gebracht hat
                    Process();
                    semCraneToStorage.Release(); // Kran darf zurück ins Lager
                }
            }

            private void Process()
            {
                Thread.Sleep(150);
                Console.WriteLine("MachineB: finished work");
            }
        }

        public class Crane
        {
            public void Run()
            {
                while (true)
                {
                    semCraneToA.Wait();
                    Move("Storage", "MachineA");
                    semA.Release(); // Maschine A darf arbeiten

                    semCraneToB.Wait();
                    Move("MachineA", "MachineB");
                    semB.Release(); // Maschine B darf arbeiten

                    semCraneToStorage.Wait();
                    Move("MachineB", "Storage");
                    semCraneToA.Release(); // Zyklus von vorne
                }
            }

            private void Move(string from, string to)
            {
                Thread.Sleep(200);
                Console.WriteLine($"Crane: moving from {from} to {to}");
            }
        }
    }
}

