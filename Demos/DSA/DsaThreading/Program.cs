using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DsaThreading;

Console.WriteLine("Hello, World!");

await ThreadingDemo();


static async Task ThreadingDemo()
{
    // How C# manages Thread (OS threads not CPU threads)
    //In C# threads are an object, they are managed by the CLR. For example, when this main runs to print "Hello, World!" a thread is created to run the main method. The CLR manages the thread and its lifecycle. When the main method completes, the thread is terminated.

    Console.WriteLine($"Main runs on thread #{Environment.CurrentManagedThreadId}");

    //We can create our own threads - using the Thread Class. Its constructor takes one argument: a delegate. The delegate can be defined with a method or a lambda expression. The delegate represents the code that will be executed on the new thread.

    var workerThread = new Thread(() =>
    {
        Console.WriteLine($"Hello from thread #{Environment.CurrentManagedThreadId}");
    });

    //Once we hve a thread setup - we have to manually start it.
    Console.WriteLine($"Before Start() call, isAlive = {workerThread.IsAlive}");

    workerThread.Start(); //Thread is now running.
    Console.WriteLine($"During thread delegate code, isAlive = {Thread.CurrentThread.IsAlive}");
    workerThread.Join(); // our thread was called from the Main thread, so we can wait for it to finish before continuing. This is called "joining" the thread.
    //Calling .Join() blocks the outer/caller thread similar to await in async programming.


    //Parallelism vs Concurrency
    //Interleaving - Below even the runtime the actual OS scheduler (the thing that the kernel uses to map OS threads to CPU threads) interleaves the threads - switches them on and off CPU threads really fast according to rules that we can't influence from our program -  so our threads dont really complete in the same order 100% of the time. This is called concurrency - multiple threads are running at the same time, but not necessarily on different CPU threads. The OS scheduler decides which thread runs on which CPU thread and when.

    // Threads give us concurrency, true parallelism depends on the hardware (and kernel)
    var threads = new List<Thread>();

    for (int i = 1; i <= 5; i++)
    {
        int id = i;

        var th = new Thread(() =>
        {
            Thread.Sleep(Random.Shared.Next(5, 40));
            Console.WriteLine($"Worker {id} finished on thread #{Environment.CurrentManagedThreadId}");
        });

        threads.Add(th);
        th.Start();
    }

    foreach (Thread thread in threads) thread.Join();

    // Thread Safe collection

    //Ordinary collections are not optimized or built with multiple threads in mind - they would corrupt or more likely throw runtime exceptions if two threads delegate access them concurrently.
    // Thankfully there are thread safe version of common collections and methods.
    var counts = new ConcurrentDictionary<int, int>();

    var threadPool = new List<Thread>();


    for (int i = 1; i <= 8; i++)
    {
        int id = i;

        var th = new Thread(() =>
        {
            for (int k = 0; k < 1000; k++)
                counts.AddOrUpdate(id, 1, (_, prev) => prev + 1);
            // In the line above, AddOrUpdate takes the key, the value, and a third argument
            // a delegate to execute if they already exist
            // _ = C# discard - indicates the key parameter is intentionally ignores because the delegate wont use it
            // prev - the existing integer value currently stored for that key
            // prev +  1 = increment that value giving us a new key to insert
        });

        threadPool.Add(th);
        th.Start();
    }

    foreach (var th in threadPool) th.Join();

    Console.WriteLine($"Recorded {counts.Values.Sum()} increments across {counts.Count} threads");

    // When working with threads, it's common to not manually create the threads ourselves
    // For short work items like what we did above, we ca use ThreadPool
    //The threadPool is just a runtime managed set of background threads that we don't have to
    //create or destroy - they're already there we can just borrow one.

    //Lets make a ConcurrentQueue for FIFO work, we'll just have it store ints
    var done = new ConcurrentQueue<int>();

    for (int i = 0; i < 5; i++)
    {
        int n = i;

        //Instead of creating a thread manually nd starting it I can just ask for a thread from
        //the background ThreadPool and pass it some delegate or method to execute
        ThreadPool.QueueUserWorkItem(_ => done.Enqueue(n * n));
    }

    //Because we dont actually have the Threads themselves at our disposal -  we'll
    //do like a crude await
    while (done.Count < 5) Thread.Sleep(5); // awaut - but way dumber

    Console.WriteLine($"Threadpool finished. {string.Join(", ", done.OrderBy(x => x))}");

    //Tasks. We've already seen Tasks. Creating threads, starting
    ParallelSum();

    static void ParallelSum()
    {
        // Just a big int array
        int[] data = Enumerable.Range(1, 8000000).ToArray();

        // First - let do this totally sequentially -one thread without tasks
        var sw = Stopwatch.StartNew();
        long sequential = SumRange(data, 0, data.Length);
        sw.Stop();
        Console.WriteLine($"Sequential sum = {sequential}. {sw.ElapsedTicks} ticks, 1 thread");

        // Before we parallelize this, lets play with Tasks
        Task<long> half1 = Task.Run(() => SumRange(data, 0, data.Length / 2));
        Task<long> half2 = Task.Run(() => SumRange(data, data.Length / 2, data.Length));

        long total = half1.Result + half2.Result;
        Console.WriteLine($"Two task sum: {total}");

        // Lets parallelize this with Tasks and the TPL library
        long parallelTotal = 0;

        sw.Restart();

        Parallel.For(0, data.Length,
            //After we give it start start and end value fro the loop - this is a For loop
            // We give it an accumulator
            () => 0L,
            //body: for each loop iterator on a given thread do somethin
            //threads subtotal for the sum
            (i, _, local) => local + data[i],
            //localFinaly: AFTER a thread finished all its assigned items this is called
            //Adds the Thread's local Sum (the thing that start with a value of 0L (long))
            //to the global parallelTotal
            local => Interlocked.Add(ref parallelTotal, local) //combine per Thread to the outer variable
        );

        sw.Stop();

        Console.WriteLine($"Parallel sum = {parallelTotal}. {sw.ElapsedTicks} ticks, multi-thread");
    }

    static long SumRange(int[] a, int start, int end)
    {
        long sum = 0;
        for (int i = start; i < end; i++)
        {
            sum += a[i];
        }
        return sum;
    }

    RaceDemo();

    static void RaceDemo()
    {
        var bank = new Bank();
        Parallel.For(0, 1000000, _ => bank.DepositUnsafe(1));
        Console.WriteLine($"Unsafe Balance = {bank.Balance} (expected 100000)");
    }

    SafeDemo();

    static void SafeDemo()
    {
        var bank = new Bank();
        Parallel.For(0, 1000000, _ => bank.DepositSage(1));
        Console.WriteLine($"Safe Balance = {bank.Balance} (expected 100000)");
    }

    // Onterlocked - lockfree atomic operations against one variable

    InterlockedDemo();

    static void InterlockedDemo()
    {
        long counter = 0;
        //Interlock - faster than a lock when doing single atomic operations
        // if all you need is that - use an interlock over a lock
        Parallel.For(0, 100000, _ => Interlocked.Increment(ref counter));
        Console.WriteLine($"Interlocked = {counter} (expected 100000)");

        //Deadlocks and Starvation

        //Deadlock - If two tasks create locks on resources the other ends up needing
        //they can deadlock. In this case they never resolve . our console app
        //would be waiting forever

        //Starvation - A thread gets blocked by another thread world - and stays alive
        // but cannot progress. Different from a deadlock - because the other thread is able to resolve
        //This starved thread persists - potentially starving the ThreadPool

        // Cancellation Tokens

        CancellationDemo();
        // Rather than abruptly killing a thread or having it die via some exception
        //Potentially leading to data loss -  we can use a cancellation token to ASK a thread to be ended and it will do so once it has the chance to exit gracefully
        static void CancellationDemo()
        {
            // Calling for a CancellationToken, having it auto-cancel after 100ms
            // Side not using: Once we exit the scope where the variable created with using
            // lives in - dispose of it
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

            CancellationToken token = cts.Token;

            var work = Task.Run(() =>
            {
                for (long i = 0; ; i++)
                {
                    token.ThrowIfCancellationRequested();
                    if (i % 5000000 == 0) {/*some simulated work*/}
                }
            }, token);

            try
            {
                work.Wait();
            }
            catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
            {
                Console.WriteLine("Work was cancelled cooperatively");
            } //When doing tasks parallel library stuff, we need to unwrap the AggregateExceptions
            //to allow for specific catch. Same logic as multiple blocks
            //Just more convoluted because AggregateExceptions are like an exception list
            catch(AggregateException ex) when (ex.InnerException is InvalidOperationException)
            {
                Console.WriteLine("How'd you get here?");
            }
        }
    }

    ExceptionDemo();

    static void ExceptionDemo()
    {
        //Our task start up here when we call run...
        var t = Task.Run(() => throw new InvalidOperationException("oops"));

        //Counter-intuitively, an exception inside a task DOES NOT crash on the spot
        //Because its actually thrown during the t.Wait() below.
        try
        {
            t.Wait();
        }
        catch (AggregateException ex)
        {
            // Aggregate exceptions themselves are kind of weird
            // One task have several fault - so they get thrown inside an AggregateException
            Console.WriteLine($"Caught . {ex.InnerException!.Message}");
        }
    }

    await AsyncDemo();

    static async Task AsyncDemo()
    {
        Console.WriteLine($"Before await on thread #{Environment.CurrentManagedThreadId}");
        await Task.Delay(50);
        Console.WriteLine($"After await on thread #{Environment.CurrentManagedThreadId}");
    }
}
