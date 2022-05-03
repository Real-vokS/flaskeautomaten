using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace flaskeautomaten
{
    class Program
    {
        static int sodaLabel = 1;
        static int beerLabel = 1;

        static int MaxQueueSize = 24;


        static Queue<string> queue = new Queue<string>();
        static Queue<string> sodaQueue = new Queue<string>();
        static Queue<string> beerQueue = new Queue<string>();


        static Random rnd = new Random();
        static void Main(string[] args)
        {
            Thread t1 = new Thread(Produce);
            t1.Name = "[Producent]";
            t1.Start();

            Thread t2 = new Thread(Splitter);
            t2.Name = "[Flaske Splitter]";
            t2.Start();

            Thread t3 = new Thread(SodaConsumer);
            t3.Name = "[Soda Consumer]";
            t3.Start();

            Thread t4 = new Thread(BeerConsumer);
            t4.Name = "[Beer Consumer]";
            t4.Start();

            Thread.Sleep(1000);

            t1.Join();
            t2.Join();
            t3.Join();
            t4.Join();


        }

        static void BeerConsumer()
        {
            while (true)
            {
                Monitor.Enter(beerQueue);
                if (beerQueue.TryDequeue(out string result))
                {
                    Console.WriteLine("{0} is drinking " + result, Thread.CurrentThread.Name);
                    Monitor.PulseAll(beerQueue);
                }
                Monitor.Exit(beerQueue);
                Thread.Sleep(1000);
            }
        }
        static void SodaConsumer()
        {
            while (true)
            {
                Monitor.Enter(sodaQueue);
                if (sodaQueue.TryDequeue(out string result))
                {
                    Console.WriteLine("{0} is drinking " + result, Thread.CurrentThread.Name);
                    Monitor.PulseAll(sodaQueue);
                }
                Monitor.Exit(sodaQueue);
                Thread.Sleep(1000);
            }
        }

        static void Splitter()
        {
            while (true)
            {
                Monitor.Enter(queue);
                if (queue.TryDequeue(out string result))
                {
                    Monitor.Enter(beerQueue);
                    Monitor.Enter(sodaQueue);
                    if (beerQueue.Count < MaxQueueSize)
                    {
                        if (result.Contains("Beer"))
                        {
                            beerQueue.Enqueue(result);
                            Monitor.PulseAll(queue);
                        }
                    }
                    else
                    {
                        Monitor.Wait(beerQueue);
                    }
                    Monitor.Exit(beerQueue);

                    if (sodaQueue.Count < MaxQueueSize)
                    {
                        if (result.Contains("Soda"))
                        {
                            sodaQueue.Enqueue(result);
                            Monitor.PulseAll(queue);
                        }
                    }
                    else
                    {
                        Monitor.Wait(sodaQueue);
                    }
                    Monitor.Exit(sodaQueue);
                }
                Monitor.Exit(queue);

            }


        }

        static void Produce()
        {
            while (true)
            {
                Monitor.Enter(queue);
                if (queue.Count < MaxQueueSize)
                {
                    int bottle = rnd.Next(1, 3);
                    if (bottle == 1)
                    {
                        queue.Enqueue("Soda" + sodaLabel);
                        sodaLabel++;
                    }
                    if (bottle == 2)
                    {
                        queue.Enqueue("Beer" + beerLabel);
                        beerLabel++;
                    }
                }
                else
                {
                    Monitor.Wait(queue);
                }
                Monitor.Exit(queue);
            }
        }
    }
}
