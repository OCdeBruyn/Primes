using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Primes {
    class Program {
        static async Task Main(string[] args) {
            //BenchmarkRunner.Run<Primes>();
            //BenchmarkRunner.Run<AlternativePrimes>();

            var multiResult1 = Primes.single(10).ToList();
            Console.WriteLine($"Multi Result: {multiResult1.Count}");
            Console.WriteLine($"Success: {multiResult1.Count == 4}");
            var multiResult2 = Primes.single(100).ToList();
            Console.WriteLine($"Multi Result: {multiResult2.Count}");
            Console.WriteLine($"Success: {multiResult2.Count == 25}");
            var multiResult3 = Primes.single(1000).ToList();
            Console.WriteLine($"Multi Result: {multiResult3.Count}");
            Console.WriteLine($"Success: {multiResult3.Count == 168}");
            var multiResult4 = Primes.single(10000).ToList();
            Console.WriteLine($"Multi Result: {multiResult4.Count}");
            Console.WriteLine($"Success: {multiResult4.Count == 1229}");
            var multiResult5 = Primes.single(100000).ToList();
            Console.WriteLine($"Multi Result: {multiResult5.Count}");
            Console.WriteLine($"Success: {multiResult5.Count == 9592}");
            //var singleResult = Primes.single();
            //Console.WriteLine($"Single Result: {singleResult.Count}");
            Console.ReadKey();
        }

        public async void tenToBillion() {
            var multiResult1 = await Primes.multi(10);
            Console.WriteLine($"Multi Result: {multiResult1.Count}");
            Console.WriteLine($"Success: {multiResult1.Count == 4}");
            var multiResult2 = await Primes.multi(100);
            Console.WriteLine($"Multi Result: {multiResult2.Count}");
            Console.WriteLine($"Success: {multiResult2.Count == 25}");
            var multiResult3 = await Primes.multi(1000);
            Console.WriteLine($"Multi Result: {multiResult3.Count}");
            Console.WriteLine($"Success: {multiResult3.Count == 168}");
            var multiResult4 = await Primes.multi(10000);
            Console.WriteLine($"Multi Result: {multiResult4.Count}");
            Console.WriteLine($"Success: {multiResult4.Count == 1229}");
            var multiResult5 = await Primes.multi(100000);
            Console.WriteLine($"Multi Result: {multiResult5.Count}");
            Console.WriteLine($"Success: {multiResult5.Count == 9592}");
            var multiResult6 = await Primes.multi(1000000);
            Console.WriteLine($"Multi Result: {multiResult6.Count}");
            Console.WriteLine($"Success: {multiResult6.Count == 78498}");
            var multiResult7 = await Primes.multi(10000000);
            Console.WriteLine($"Multi Result: {multiResult7.Count}");
            Console.WriteLine($"Success: {multiResult7.Count == 664579}");
            var multiResult8 = await Primes.multi(100000000);
            Console.WriteLine($"Multi Result: {multiResult8.Count}");
            Console.WriteLine($"Success: {multiResult8.Count == 5761455}");
            var multiResult9 = await Primes.multi(1000000000);
            Console.WriteLine($"Multi Result: {multiResult9.Count}");
            Console.WriteLine($"Success: {multiResult9.Count == 50847534}");
        }
    }

    public class Primes {
        [Benchmark]
        public async Task<List<int>> Multi() => await multi();
        [Benchmark]
        public List<int> Single() => single();
        /// <summary>
        /// Loops through values from 0 to {{total}}
        /// Checks if they are primes 
        /// Adds them to the return list if they are.
        /// </summary>
        /// <param name="total"></param>
        /// <returns>List of Primes</returns>
        public static List<int> single(int total = 1000000) {
            var list = new List<int>();
            for (int i = 0; i <= total; i++) {
                if (IsPrime(i)) list.Add(i);
            }
            return list;
        }

        /// <summary>
        /// Checks the total to determine Threads and Batch size
        /// Splits the work into batches according to the determined thread pool
        /// Loops through each batch and checks each value to see if it is a prime
        /// Adds primes to the list of prime results List<List<int>>
        /// Combines the results into single list and returns the Task containing the list of primes
        /// Just use await to get List<int> result
        /// </summary>
        /// <param name="total"></param>
        /// <returns>Task<List<int>>"</returns>
        public static async Task<List<int>> multi(int total = 1000000) {
            List<List<int>> lists = new List<List<int>>();
            int threads = 32;
            int batch = 100000;
            if (total <= 100000) {
                threads = 1;
                batch = total;
            } else if (total <= 3200000) {
                threads = (int)Math.Ceiling((double)(total / 100000));
            } else {
                batch = (int)Math.Ceiling((double)(total / threads));
            }

            lists = new List<List<int>>();
            var taskPool = new List<Task>();
            for (int i = 0; i < threads; i++) {
                var j = i;

                taskPool.Add(Task.Run(() => {
                    lists.Add(populateList(j, batch, total));
                }));
            }

            await Task.WhenAll(taskPool);
            var retVal = getFromMulti(ref lists);

            return retVal;
        }

        static List<int> getFromMulti(ref List<List<int>> lists) {
            var retVal = new List<int>();

            if (lists.Count > 0) {
                foreach (var list in lists) {
                    foreach (var item in list) {
                        retVal.Add(item);
                    }
                }
            } else {
                //Catering for timing issues encountered when system under too much load.
                //Task.WhenAll(taskPool) should make this irrelevant
                Console.WriteLine("Waiting");
                Thread.Sleep(100);
                getFromMulti(ref lists);
            }
            return retVal;
        }
        static List<int> populateList(int index, int batch, int total) {
            List<int> list = new List<int>();
            for (int i = (batch * index) + 1; i < batch * (index + 1) + 1 && i < total; i++) {
                if (IsPrime(i))
                    list.Add(i);
            }
            return list;
        }

        /// <summary>
        /// For some reason Mathematicians don't know 1 is a prime, what's next? Pluto is not a planet?
        /// Math check to see if a number is a prime
        /// </summary>
        /// <param name="number"></param>
        /// <returns>bool</returns>
        public static bool IsPrime(int number) {
            if (number <= 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            var root = (int)Math.Floor(Math.Sqrt(number));

            for (int i = 3; i <= root; i += 2) {
                if (number % i == 0) {
                    return false;
                }
            }

            return true;
        }
    }

    public class AlternativePrimes {
        /// <summary>
        /// This is still incomplete and not nearly as efficient as hoped.
        /// The idea is to work from an existing list of numbers and eliminate non prime numbers by just removing all values that are multiples of the primes found
        /// Starting with 2 you remove all even numbers
        /// Going to 3 you remove all multiples of 3
        /// 4 is already removed when you get to it
        /// 5 will remove all multiples of 5
        /// 6 is already removed
        /// 7 will remove all multiples of 7
        /// etc...
        /// </summary>

        [Benchmark]
        public async Task<List<int>> Multiple() => await multi();
        [Benchmark]
        public List<int> Single() => single().ToList();

        public static IEnumerable<int> single(int total = 1000000) {
            List<int> val = new List<int>();
            populateLists(0, val, total, total);
            for (int i = 0; i < val.Count; i++) {
                if (val[i] == -1) continue;
                eliminateNonPrimes(i, val[i], val);
            }

            foreach (var item in val) {
                if (!item.Equals(-1)) yield return item;
            }
        }

        static ConcurrentDictionary<int, List<int>> valueDictionary = new ConcurrentDictionary<int, List<int>>();
        public static async Task<List<int>> multi(int total = 1000000) {
            int threads = 32;
            int batch = 100000;
            if (total <= 100000) {
                threads = 1;
                batch = total;
            } else if (total < 1000000) {
                threads = (int)Math.Ceiling((double)(total / 100000));
            } else {
                batch = (int)Math.Ceiling((double)(total / threads));
            }

            valueDictionary = new ConcurrentDictionary<int, List<int>>();
            var taskPool = new List<Task>();

            for (int i = 0; i < threads; i++) {
                var j = i;
                valueDictionary.GetOrAdd(i, new List<int>());
                taskPool.Add(Task.Run(() => populateLists(j, valueDictionary[j], batch, total)));
            }

            await Task.WhenAll(taskPool);

            var counter = 0;
            while (counter < threads) {
                for (int i = 0; i < valueDictionary[counter].Count; i++) {
                    if (valueDictionary[counter][i] == -1) continue;
                    await queuePrimeElimination(valueDictionary[counter][i], counter, threads);
                }
                counter++;
            }

            List<int> retVal = new List<int>();
            foreach (var item in valueDictionary) {
                foreach (var value in item.Value) {
                    if (value != -1) retVal.Add(value);
                }
            }
            return retVal;
        }

        static void populateLists(int index, List<int> list, int batch, int total) {
            for (int i = (batch * index) + 1; i < batch * (index + 1) + 1 && i < total; i++) {
                if (i == 1) continue;
                list.Add(i);
            }
        }

        static async Task queuePrimeElimination(int value, int counter, int threads) {
            if (value == 1) return;
            var taskPool = new List<Task>();
            for (int i = counter; i < threads; i++) {
                var j = i;
                var v = value;
                taskPool.Add(Task.Run(() => eliminateNonPrimes(j == counter ? j : 0, v, valueDictionary[j])));
            }

            await Task.WhenAll(taskPool);
        }
        static void eliminateNonPrimes(int index, int value, List<int> list) {
            if (value == 1) return;
            for (int i = list.Count - 1; i >= index; i--) {
                if (list[i] % value == 0 && list[i] != value) {
                    list[i] = -1;
                }
            }
        }
    }
}
