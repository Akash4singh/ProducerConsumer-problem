using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ProducerConsumer
{
    class ProducerConsumerBlockingCollection
    {
        // Fisrt Of all we need a bounded (like 15 Cells) queue/Buffer of integer so that it does not get overflown & stop the responsible process.
        static BlockingCollection<int> messages = new BlockingCollection<int>(new ConcurrentBag<int>(), 15);

        // required static number to be produced and consumed
        private static int number = 0;

        // Cancellation token is for stoping the processes after pressing any button
        static CancellationTokenSource cts = new CancellationTokenSource();

        // Producer function that increments our "number" on an interval of 100 ms, and adds it to a messages queue
        private static void Producer()
        {

            while (true)
            {
                // Checking for the cancellation request
                cts.Token.ThrowIfCancellationRequested();

                //If cancellation is not requested
                //Increase the number by 1
                //add the no. to the queue
                // Suspend the current process for 100ms
                number += 1;
                messages.Add(number);
                Console.WriteLine($"producer process inserting {number}\t");
                Thread.Sleep(100);
            }
        }

        // Consumer function to print & delete the number whenever a number is available in the queue.
        private static void Consumer()
        {
            // To get and clear/Empty each slots filled in queue's item we will use 
            // special method of BlockingCollection ie-> GetConsumingEnumerable
            foreach (var item in messages.GetConsumingEnumerable())
            {
                // Checking for the cancellation request
                cts.Token.ThrowIfCancellationRequested();
                //If cancellation is not requested
                //print the number being consumed
                //add the no. to the queue
                // Suspend the current process for 1000ms
                Console.WriteLine($"producer process consuming {item}");
                Thread.Sleep(1000);
            }
        }

        // creating the main thread as ProduceAndConsume
        public static void ProduceAndConsume()
        {
            // Running both Producer and Consumer Threads
            var producer = Task.Factory.StartNew(Producer);
            var consumer = Task.Factory.StartNew(Consumer);

            // this process will wait till all secondary process terminates
            try
            {
                Task.WaitAll(new[] { producer, consumer }, cts.Token);
            }
            catch (AggregateException e)
            {
                e.Handle(ec => true);
            }
        }

        static void Main(string[] args)
        {
            // starting the ProduceAndConsume process 
            Task.Factory.StartNew(ProduceAndConsume, cts.Token);

            // get the termination key and stopping the processes
            Console.ReadKey();
            cts.Cancel();
        }
    }
}